using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using static UniVRM10.Vrm10;


namespace UniVRM10.VRM10Viewer
{
    [Serializable]
    class VRM10ViewerController : IDisposable
    {
        [SerializeField]
        TextAsset m_motion;

        [SerializeField]
        public RenderTexture m_faceCameraTarget = default;

        [SerializeField]
        public Transform m_faceCamera = default;
        [SerializeField]
        public GameObject m_lookAtTarget = default;

        [Header("Material")]
        [SerializeField]
        Material m_pbrOpaqueMaterial = default;
        [SerializeField]
        Material m_pbrAlphaBlendMaterial = default;
        [SerializeField]
        Material m_mtoonMaterialOpaque = default;
        [SerializeField]
        Material m_mtoonMaterialAlphaBlend = default;

        private CancellationTokenSource _cancellationTokenSource;
        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
        public void Cancel()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private TinyMToonrMaterialImporter m_mtoonImporter;
        private TinyPbrMaterialImporter m_pbrImporter;

        private Loaded m_loaded;
        Loaded Loaded
        {
            get
            {
                return m_loaded;
            }
            set
            {
                if (m_loaded != null)
                {
                    m_loaded.Dispose();
                    m_loaded = null;
                }
                m_loaded = value;
                if (OnLoaded != null)
                {
                    OnLoaded(m_loaded);
                }
            }
        }
        public event Action<Loaded> OnLoaded;

        IVrm10Animation m_src = default;
        public IVrm10Animation TPose;
        public IVrm10Animation Motion
        {
            get { return m_src; }
            set
            {
                if (m_src != null)
                {
                    m_src.Dispose();
                }
                m_src = value;

                TPose = new Vrm10TPose(m_src.ControlRig.Item1.GetRawHipsPosition());
            }
        }

        public event VrmMetaInformationCallback OnUpdateMeta;
        void RaiseUpdateMeta(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta vrm10Meta, Migration.Vrm0Meta vrm0Meta)
        {
            if (OnUpdateMeta != null)
            {
                OnUpdateMeta(thumbnail, vrm10Meta, vrm0Meta);
            }
        }

        public void Init()
        {
            m_mtoonImporter = (m_mtoonMaterialOpaque != null && m_mtoonMaterialAlphaBlend != null) ? new TinyMToonrMaterialImporter(m_mtoonMaterialOpaque, m_mtoonMaterialAlphaBlend) : null;
            m_pbrImporter = (m_pbrOpaqueMaterial != null && m_pbrAlphaBlendMaterial != null) ? new TinyPbrMaterialImporter(m_pbrOpaqueMaterial, m_pbrAlphaBlendMaterial) : null;
        }

        public void Start()
        {
            // load initial bvh
            if (m_motion != null)
            {
                Motion = BvhMotion.LoadBvhFromText(m_motion.text);
                if (GraphicsSettings.renderPipelineAsset != null
                    && m_pbrAlphaBlendMaterial != null)
                {
                    Motion.SetBoxManMaterial(GameObject.Instantiate(m_pbrOpaqueMaterial));
                }
            }

            if (ArgumentChecker.TryGetFirstLoadable(out var cmd))
            {
                var _ = LoadModelPath(cmd, new());
            }
        }

        public void ShowBoxMan(bool show)
        {
            if (Motion != null)
            {
                Motion.ShowBoxMan(show);
            }
        }

        [DllImport("__Internal")]
        public static extern void WebGL_VRM10_VRM10Viewer_FileDialog(string target, string message);

        string FileDialog(string messageTarget, string messageName)
        {
#if UNITY_EDITOR 
            return UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm,glb,gltf,zip");
#elif UNITY_STANDALONE_WIN
            return VRM10FileDialogForWindows.FileDialog("open VRM", "vrm", "glb", "gltf", "zip");
#elif UNITY_WEBGL
            // Open WebGL_VRM10_VRM10Viewer_FileDialog
            // see: Assets\VRM10_Samples\VRM10Viewer\Plugins\WebGL_VRM10_VRM10Viewer.jslib
            WebGL_VRM10_VRM10Viewer_FileDialog(messageTarget, messageName);
            // Control flow does not return here. return empty string with dummy
            return null;
#else
            return Application.dataPath + "/default.vrm";
#endif
        }

        public IEnumerator LoadCoroutine(string url, LoadOptions opts)
        {
            var www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            var _ = LoadModelBytes("WebGL.vrm", www.downloadHandler.data, opts);
        }

        public void OnOpenModelClicked(LoadOptions opts, string messageTarget, string messageName)
        {
            var path = FileDialog(messageTarget, messageName);
            _ = LoadModelPath(path, opts);
        }

        public async void OnOpenMotionClicked()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open Motion", "", "bvh,gltf,glb,vrma");
#elif UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open Motion", "bvh", "gltf", "glb", "vrma");
#else
            var path = Application.dataPath + "/default.bvh";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            if (ext == ".bvh")
            {
                Motion = BvhMotion.LoadBvhFromPath(path);
                return;
            }

            // gltf, glb etc...
            using GltfData data = new AutoGltfFileParser(path).Parse();
            var vrmaData = new VrmAnimationData(data);
            using var loader = new VrmAnimationImporter(vrmaData);
            var instance = await loader.LoadAsync(new ImmediateCaller());
            Motion = instance.GetComponent<Vrm10AnimationInstance>();
            instance.GetComponent<Animation>().Play();
        }

        public async void OnPastePoseClicked()
        {
            var text = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                Motion = await Vrm10PoseLoader.LoadVrmAnimationPose(text);
            }
            catch (UniJSON.ParserException)
            {
                UniGLTFLogger.Warning("UniJSON.ParserException");
            }
            catch (UniJSON.DeserializationException)
            {
                UniGLTFLogger.Warning("UniJSON.DeserializationException");
            }
        }

        public void OnResetSpringBoneClicked()
        {
            if (Loaded != null)
            {
                if (Loaded.Runtime != null)
                {
                    Loaded.Runtime.SpringBone.RestoreInitialTransform();
                }
            }
        }

        public void OnReconstructSpringBoneClicked()
        {
            if (Loaded != null)
            {
                if (Loaded.Runtime != null)
                {
                    Loaded.Runtime.SpringBone.ReconstructSpringBone();
                }
            }
        }

        public async Task LoadModelPath(string path, LoadOptions opts)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            await LoadModelBytes(path, bytes, opts);
        }

        IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(LoadOptions opts)
        {
            var useCustomPbr = opts.UseCustomPbrMaterial && m_pbrImporter != null;
            var useCustomMToon = opts.UseCustomMToonMaterial && m_mtoonImporter != null;
            if (!useCustomPbr && !useCustomMToon)
            {
                // カスタムしない。デフォルトのローダーを使う
                return default;
            }

            return OrderedMaterialDescriptorGenerator.CreateCustomGenerator(
                useCustomPbr ? m_pbrImporter : null,
                useCustomMToon ? m_mtoonImporter : null);
        }

        IAwaitCaller GetIAwaitCaller(bool useAsync)
        {
            if (useAsync)
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    return new RuntimeOnlyNoThreadAwaitCaller();
                }
                else
                {
                    return new RuntimeOnlyAwaitCaller();
                }
            }
            else
            {
                return new ImmediateCaller();
            }
        }


        public async Task LoadModelBytes(string path, byte[] bytes, LoadOptions opts)
        {
            // cleanup
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                //
                // try VRM
                //
                UniGLTFLogger.Log($"{path}");
                var vrm10Instance = await Vrm10.LoadBytesAsync(bytes,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    awaitCaller: GetIAwaitCaller(opts.UseAsync),
                    materialGenerator: GetMaterialDescriptorGenerator(opts),
                    vrmMetaInformationCallback: RaiseUpdateMeta,
                    ct: cancellationToken,
                    springboneRuntime: opts.UseSpringboneSingelton ? new Vrm10FastSpringboneRuntime() : new Vrm10FastSpringboneRuntimeStandalone());
                if (cancellationToken.IsCancellationRequested)
                {
                    UnityObjectDestroyer.DestroyRuntimeOrEditor(vrm10Instance.gameObject);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (vrm10Instance == null)
                {
                    UniGLTFLogger.Warning("LoadPathAsync is null");
                    return;
                }

                var instance = vrm10Instance.GetComponent<RuntimeGltfInstance>();
                instance.ShowMeshes();
                instance.EnableUpdateWhenOffscreen();
                Loaded = new Loaded(instance);
            }
            catch (Exception)
            {
                try
                {
                    //
                    // fallback gltf
                    //
                    var instance = await GltfUtility.LoadBytesAsync(path, bytes,
                        awaitCaller: GetIAwaitCaller(opts.UseAsync),
                        materialGenerator: GetMaterialDescriptorGenerator(opts)
                    );
                    instance.ShowMeshes();
                    instance.EnableUpdateWhenOffscreen();
                    Loaded = new Loaded(instance);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        UniGLTFLogger.Warning($"Canceled to Load: {path}");
                    }
                    else
                    {
                        UniGLTFLogger.Error($"Failed to Load: {path}");
                        UniGLTFLogger.Exception(ex);
                    }
                }
            }
        }

        public bool TryUpdate(
            bool useTPose,
            BlittableModelLevel blittableModelLevel,
            out Loaded loaded)
        {
            loaded = Loaded;
            if (loaded == null)
            {
                return false;
            }
            if (loaded.Runtime == null)
            {
                return false;
            }

            if (useTPose || Motion == null)
            {
                loaded.Runtime.VrmAnimation = TPose;
            }
            else
            {
                // Automatically retarget in Vrm10Runtime.Process
                loaded.Runtime.VrmAnimation = Motion;
            }

            loaded.Runtime.SpringBone.SetModelLevel(Loaded.Instance.transform, blittableModelLevel);

            return true;
        }
    }

    public class LoadOptions
    {
        public bool UseAsync;
        public bool UseSpringboneSingelton;
        public bool UseCustomPbrMaterial;
        public bool UseCustomMToonMaterial;
    }
}