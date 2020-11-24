using System;
using System.Collections.Generic;
using UnityEngine;

namespace DepthFirstScheduler
{
    /// <summary>
    /// UniRxのMainThreadDispatcherを参考にした。
    /// * https://github.com/neuecc/UniRx/blob/master/Assets/Plugins/UniRx/Scripts/UnityEngineBridge/MainThreadDispatcher.cs
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour
    {

        [Header("Debug")]
        public int TaskCount;

        IEnumerable<Transform> Ancestors(Transform t)
        {
            yield return t;

            if (t.parent != null)
            {
                foreach (var x in Ancestors(t.parent))
                {
                    yield return x;
                }
            }
        }

        private void Update()
        {
            TaskCount = Scheduler.MainThread.UpdateAndGetTaskCount();
        }

        static MainThreadDispatcher instance;
        static bool initialized;
        static bool isQuitting = false;

        public static bool IsInitialized
        {
            get { return initialized && instance != null; }
        }

        [ThreadStatic]
        static object mainThreadToken;

        public static MainThreadDispatcher Instance
        {
            get
            {
                Initialize();
                return instance;
            }
        }

        public static void Initialize()
        {
            if (!initialized)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    return;
                }
#endif
                MainThreadDispatcher dispatcher = null;

                try
                {
                    dispatcher = GameObject.FindObjectOfType<MainThreadDispatcher>();
                }
                catch
                {
                    // Throw exception when calling from a worker thread.
                    var ex = new Exception(
                        "DepthFirstScheduler requires a MainThreadDispatcher component created on the main thread."
                        + " Make sure it is added to the scene before calling DepthFirstScheduler from a worker thread.");
                    UnityEngine.Debug.LogException(ex);
                    throw ex;
                }

                if (isQuitting)
                {
                    // don't create new instance after quitting
                    // avoid "Some objects were not cleaned up when closing the scene find target" error.
                    return;
                }

                if (dispatcher == null)
                {
                    // awake call immediately from UnityEngine
                    new GameObject("DepthFirstScheduler").AddComponent<MainThreadDispatcher>();
                }
                else
                {
                    dispatcher.Awake(); // force awake
                }
            }
        }

        public static bool IsInMainThread
        {
            get
            {
                return (mainThreadToken != null);
            }
        }


        void Awake()
        {
            if (instance == null)
            {
                Debug.Log("Initialize UniTask.MainThreadDispatcher");

                instance = this;
                mainThreadToken = new object();
                initialized = true;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (this != instance)
                {
                    Debug.LogWarning("There is already a MainThreadDispatcher in the scene.");
                }
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = GameObject.FindObjectOfType<MainThreadDispatcher>();
                initialized = instance != null;
            }

            if (Scheduler.SingleWorkerThread != null)
            {
                Scheduler.SingleWorkerThread.Dispose();
            }
        }

        void OnApplicationQuit()
        {
            isQuitting = true;
        }
    }
}
