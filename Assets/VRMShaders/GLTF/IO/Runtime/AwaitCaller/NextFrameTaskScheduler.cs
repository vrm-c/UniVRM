using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRMShaders
{
    internal sealed class NextFrameTaskScheduler
    {
        public bool IsSupported => Application.isPlaying;

        public NextFrameTaskScheduler()
        {
            if (!IsSupported)
            {
                throw new NotSupportedException($"{nameof(NextFrameTaskScheduler)} is supported at runtime only.");
            }
        }

        public bool Enqueue(Action action)
        {
            var currentFrame = Time.frameCount;

            UnityLoopTaskScheduler.Instance.Scheduler.Enqueue(action, () => Time.frameCount != currentFrame);

            return true;
        }

        private sealed class UnityLoopTaskScheduler : MonoBehaviour
        {
            private static UnityLoopTaskScheduler _instance;

            public static UnityLoopTaskScheduler Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        var go = new GameObject("UniGLTF UnityThreadScheduler");
                        Object.DontDestroyOnLoad(go);
                        _instance = go.AddComponent<UnityLoopTaskScheduler>();
                    }
                    return _instance;
                }
            }

            public TinyManagedTaskScheduler Scheduler { get; } = new TinyManagedTaskScheduler();

            private void Update()
            {
                Scheduler.ManagedUpdate();
            }
        }
    }
}