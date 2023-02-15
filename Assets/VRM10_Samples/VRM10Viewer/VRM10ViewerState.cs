using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRMShaders;
using static UniVRM10.Vrm10;

namespace UniVRM10.VRM10Viewer
{
    class VRM10ViewerState : IDisposable
    {
        Animator m_src = default;
        GameObject m_target = default;

        GameObject Root = default;
        public void ToggleActive()
        {
            if (Root != null)
            {
                Root.SetActive(!Root.activeSelf);
            }
        }

        TextAsset m_motion;

        CancellationTokenSource _cancellationTokenSource;
        public void Cancel()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        Loaded m_loaded;
        public Loaded Loaded => m_loaded;

        public event Action MotionLoaded;
        void RaiseMotionLoaded()
        {
            var callback = MotionLoaded;
            if (callback != null)
            {
                callback();
            }
        }

        public VRM10ViewerState(Action onMotionLoaded)
        {
            onMotionLoaded += onMotionLoaded;

            m_src = GameObject.FindObjectOfType<Animator>();
            m_target = GameObject.FindObjectOfType<VRM10TargetMover>().gameObject;
            // load initial bvh
            if (m_motion != null)
            {
                LoadMotion(m_motion.text);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }

        private void LoadMotion(string source)
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse("tmp.bvh", source);
            context.Load();
            m_src = context.Root.GetComponent<Animator>();
            // hide box man
            context.Root.GetComponent<SkinnedMeshRenderer>().enabled = false;

            RaiseMotionLoaded();
        }

        public void Update(bool isBvhEnabled)
        {
            if (m_loaded != null)
            {
                if (isBvhEnabled && m_src != null)
                {
                    m_loaded.UpdateControlRigImplicit(m_src);
                }
                else
                {
                    m_loaded.TPoseControlRig();
                }
            }
        }

        public async void OpenFileDialog(bool useAsync, bool useUrp, VrmMetaInformationCallback metaCallback)
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open VRM", "vrm", "bvh");
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
            if (ext == ".bvh")
            {
                LoadMotion(path);
                return;
            }

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
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                Debug.LogFormat("{0}", path);
                var vrm10Instance = await Vrm10.LoadPathAsync(path,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    awaitCaller: useAsync ? (IAwaitCaller)new RuntimeOnlyAwaitCaller() : (IAwaitCaller)new ImmediateCaller(),
                    materialGenerator: GetVrmMaterialDescriptorGenerator(useUrp),
                    vrmMetaInformationCallback: metaCallback,
                    ct: cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    UnityObjectDestroyer.DestroyRuntimeOrEditor(vrm10Instance.gameObject);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (vrm10Instance == null)
                {
                    Debug.LogWarning("LoadPathAsync is null");
                    return;
                }

                var instance = vrm10Instance.GetComponent<RuntimeGltfInstance>();
                instance.ShowMeshes();
                instance.EnableUpdateWhenOffscreen();
                m_loaded = new Loaded(instance, m_target.transform);
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

        static IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new UrpGltfMaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInGltfMaterialDescriptorGenerator();
            }
        }

        static IMaterialDescriptorGenerator GetVrmMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new UrpVrm10MaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInVrm10MaterialDescriptorGenerator();
            }
        }
    }
}
