using System;
using System.IO;
using System.Threading;
using UniGLTF;
using UnityEngine;
using VRMShaders;
using static UniVRM10.Vrm10;

namespace UniVRM10.VRM10Viewer
{
    class VRM10ViewerState : IDisposable
    {
        GameObject m_target = default;

        GameObject Root = default;
        public void ToggleActive()
        {
            if (Root != null)
            {
                Root.SetActive(!Root.activeSelf);
            }
        }

        CancellationTokenSource _cancellationTokenSource;
        public void Cancel()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        VRM10Loaded m_loaded;
        public VRM10Loaded Model => m_loaded;

        VRM10Motion m_motion;
        public VRM10Motion Motion
        {
            get
            {
                return m_motion;
            }
            set
            {
                m_motion = value;
                if (value != null)
                {
                    m_motion.Root.localPosition = new Vector3(-0.5f, 0, -1);
                    RaiseMotionLoaded();
                }
            }
        }

        public event Action MotionLoaded;
        void RaiseMotionLoaded()
        {
            var callback = MotionLoaded;
            if (callback != null)
            {
                callback();
            }
        }

        public VRM10ViewerState(GameObject root)
        {
            Root = root;
            m_target = GameObject.FindObjectOfType<VRM10TargetMover>().gameObject;
        }

        public void OnLoadMotion(Action onMotionLoaded)
        {
            onMotionLoaded += onMotionLoaded;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }

        public void Update(bool isBvhEnabled)
        {
            if (m_loaded != null)
            {
                if (isBvhEnabled && Motion != null)
                {
                    Motion.Retarget(Model.Animator);
                }
                else
                {
                    m_loaded.TPoseControlRig();
                }
            }
        }

        public void OpenModelFileDialog(bool useAsync, bool useUrp, VrmMetaInformationCallback metaCallback)
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open VRM", "vrm");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            if (ext != ".vrm")
            {
                Debug.LogWarning($"{path} is not vrm");
                return;
            }

            LoadModel(path, useAsync, useUrp, metaCallback);
        }

        public async void LoadModel(string path, bool useAsync, bool useUrp, VrmMetaInformationCallback metaCallback)
        {
            // cleanup
            m_loaded?.Dispose();
            m_loaded = null;
            _cancellationTokenSource?.Dispose();

            // load
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            try
            {
                m_loaded = await VRM10Loaded.LoadAsync(path, useAsync, useUrp, metaCallback, cancellationToken, m_target.transform);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Debug.LogWarning($"Canceled to Load: {path}");
                }
                else
                {
                    Debug.LogError($"Failed to Load: {path}");
                    Debug.LogException(ex);
                }
            }
        }

        public void OpenMotionFileDialog()
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open BVH", "bvh", "gltf", "glb");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "bvh");
#else
            var path = Application.dataPath + "/default.bvh";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".bvh":
                    Motion = VRM10Motion.LoadBvhFromPath(path);
                    break;

                case ".gltf":
                    Motion = VRM10Motion.LoadVrmAnimationFromPath(path);
                    break;
            }
        }

        public void LoadBvhText(string source)
        {
            Motion = VRM10Motion.LoadBvhFromText(source);
        }
    }
}
