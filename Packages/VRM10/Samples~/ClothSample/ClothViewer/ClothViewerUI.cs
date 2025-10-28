using System;
using System.IO;
using System.Linq;
using System.Threading;
using UniVRM10.ClothWarp.Components;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;


namespace UniVRM10.Cloth.Viewer
{
    public class ClothViewerUI : MonoBehaviour
    {
        [SerializeField] Text m_version = default;

        [Header("Model")]
        [SerializeField] Toggle m_useAsync = default;
        [SerializeField] Button m_openModel = default;
        [SerializeField] Toggle m_showBoxMan = default;

        [Header("Cloth")]
        [SerializeField] Toggle m_useJob = default;
        [SerializeField] Toggle m_addClothToHips = default;
        [SerializeField] Button m_reconstructSprngBone = default;
        [SerializeField] Button m_resetSpringBone = default;
        [SerializeField] Toggle m_pauseSpringBone = default;

        [Header("Motion")]
        [SerializeField] Button m_openMotion = default;
        [SerializeField] Button m_pastePose = default;
        [SerializeField] Toggle ToggleMotionTPose = default;
        [SerializeField] Toggle ToggleMotionBVH = default;
        [SerializeField] ToggleGroup ToggleMotion = default;
        public bool IsTPose
        {
            get => ToggleMotion.ActiveToggles().FirstOrDefault() == ToggleMotionTPose;
            set
            {
                ToggleMotionTPose.isOn = value;
                ToggleMotionBVH.isOn = !value;
            }
        }

        [Header("Expression")]
        [SerializeField] Toggle m_enableLipSync = default;
        [SerializeField] Toggle m_enableAutoBlink = default;
        [SerializeField] Toggle m_enableAutoExpression = default;

        [SerializeField] GameObject m_target = default;
        [SerializeField] TextAsset m_motion;
        [SerializeField] TextFields m_texts = default;

        private void Reset()
        {
            var map = new ObjectMap(gameObject);

            m_version = map.Get<Text>("VrmVersion");

            m_useAsync = map.Get<Toggle>("UseAsync");
            m_openModel = map.Get<Button>("OpenModel");
            m_showBoxMan = map.Get<Toggle>("ShowBoxMan");

            m_useJob = map.Get<Toggle>("UseJob");
            m_addClothToHips = map.Get<Toggle>("AddClothToHips");
            m_reconstructSprngBone = map.Get<Button>("ReconstcutSpringBone");
            m_resetSpringBone = map.Get<Button>("ResetSpringBone");
            m_pauseSpringBone = map.Get<Toggle>("PauseSpringBone");

            m_openMotion = map.Get<Button>("OpenMotion");
            m_pastePose = map.Get<Button>("PastePose");
            ToggleMotionTPose = map.Get<Toggle>("TPose");
            ToggleMotionBVH = map.Get<Toggle>("BVH");
            ToggleMotion = map.Get<ToggleGroup>("_Motion_");

            m_enableLipSync = map.Get<Toggle>("EnableLipSync");
            m_enableAutoBlink = map.Get<Toggle>("EnableAutoBlink");
            m_enableAutoExpression = map.Get<Toggle>("EnableAutoExpression");
            m_texts.Reset(map);
            m_target = GameObject.FindObjectOfType<ClothTargetMover>().gameObject;
        }


        // Runtime
        GameObject m_root = default;
        IVrm10Animation m_src = default;
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

        public IVrm10Animation TPose;

        private CancellationTokenSource _cancellationTokenSource;

        Loaded m_loaded;
        ClothWarp.HumanoidPose m_init;

        static class ArgumentChecker
        {
            static string[] Supported = {
                ".gltf",
                ".glb",
                ".vrm",
                ".zip",
            };

            static string UnityHubPath => System.Environment.GetEnvironmentVariable("ProgramFiles") + "\\Unity\\Hub";

            public static bool IsLoadable(string path)
            {
                if (!File.Exists(path))
                {
                    // not exists
                    return false;
                }

                if (Application.isEditor)
                {
                    // skip editor argument
                    // {UnityHub_Resources}\PackageManager\ProjectTemplates\com.unity.template.3d-5.0.4.tgz
                    if (path.StartsWith(UnityHubPath))
                    {
                        return false;
                    }
                }

                var ext = Path.GetExtension(path).ToLower();
                if (!Supported.Contains(ext))
                {
                    // unknown extension
                    return false;
                }

                return true;
            }

            public static bool TryGetFirstLoadable(out string cmd)
            {
                foreach (var arg in System.Environment.GetCommandLineArgs())
                {
                    if (ArgumentChecker.IsLoadable(arg))
                    {
                        cmd = arg;
                        return true;
                    }
                }

                cmd = default;
                return false;
            }
        }

        [SerializeField]
        public int Iteration = 32;

        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                    VRM10SpecVersion.MAJOR, VRM10SpecVersion.MINOR);

            m_openModel.onClick.AddListener(OnOpenModelClicked);
            m_openMotion.onClick.AddListener(OnOpenMotionClicked);
            m_pastePose.onClick.AddListener(OnPastePoseClicked);
            m_reconstructSprngBone.onClick.AddListener(OnReconstruct);
            m_resetSpringBone.onClick.AddListener(OnReset);

            // load initial bvh
            if (m_motion != null)
            {
                Motion = BvhMotion.LoadBvhFromText(m_motion.text);
            }

            if (ArgumentChecker.TryGetFirstLoadable(out var cmd))
            {
                LoadModel(cmd);
            }

            m_texts.Start();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Dispose();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (m_root != null) m_root.SetActive(!m_root.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
            }

            if (Motion != null)
            {
                Motion.ShowBoxMan(m_showBoxMan.isOn);
            }

            if (m_loaded != null)
            {
                m_loaded.EnableLipSyncValue = m_enableLipSync.isOn;
                m_loaded.EnableBlinkValue = m_enableAutoBlink.isOn;
                m_loaded.EnableAutoExpressionValue = m_enableAutoExpression.isOn;

                if (IsTPose)
                {
                    m_loaded.Runtime.VrmAnimation = TPose;
                }
                else if (Motion != null)
                {
                    // Automatically retarget in Vrm10Runtime.Process
                    m_loaded.Runtime.VrmAnimation = Motion;
                }
            }
        }

        void OnOpenModelClicked()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#elif UNITY_STANDALONE_WIN
            var path = ClothFileDialogForWindows.FileDialog("open VRM", "vrm");
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
                UniGLTFLogger.Warning($"{path} is not vrm");
                return;
            }

            LoadModel(path);
        }

        async void OnOpenMotionClicked()
        {
#if UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open Motion", "", "bvh");
#elif UNITY_STANDALONE_WIN
            var path = ClothFileDialogForWindows.FileDialog("open Motion", "bvh", "gltf", "glb", "vrma");
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

        async void OnPastePoseClicked()
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

        void OnReconstruct()
        {
            if (m_loaded == null)
            {
                return;
            }
            m_loaded.Runtime.SpringBone.ReconstructSpringBone();
            // var system = m_loaded.Instance.GetComponent<ClothWarp.RotateParticleSystem>();
            // system.ResetParticle();
        }

        void OnReset()
        {
            if (m_loaded == null)
            {
                return;
            }
            m_loaded.Runtime.SpringBone.RestoreInitialTransform();
            // ResetStrandPose();
        }

        // Action<float> MakeSetPose()
        // {
        //     var start = m_init;
        //     var animator = m_loaded.Instance.GetComponent<Animator>();
        //     var end = new ClothWarp.HumanoidPose(animator);
        //     return (float t) =>
        //     {
        //         ClothWarp.HumanoidPose.ApplyLerp(animator, start, end, t);
        //     };
        // }

        // void ResetStrandPose()
        // {
        //     ResetStrandPose(MakeSetPose(), 32, 1.0f / 30, 60);
        // }

        // void ResetStrandPose(Action<float> setPose, int iteration, float timeDelta, int finish)
        // {
        //     var system = m_loaded.Instance.GetComponent<ClothWarp.RotateParticleSystem>();

        //     // init
        //     setPose(0);
        //     system.ResetParticle();

        //     // lerp
        //     var t = 0.0f;
        //     var d = 1.0f / iteration;
        //     for (int i = 0; i < iteration; ++i, t += d)
        //     {
        //         setPose(t);
        //         system.Process(timeDelta);
        //     }

        //     // finish
        //     setPose(1.0f);
        //     for (int i = 0; i < finish; ++i)
        //     {
        //         system.Process(timeDelta);
        //     }
        // }

        void OnInit(Vrm10Instance vrm)
        {
            var animator = vrm.GetComponent<Animator>();

            try
            {
                if (vrm.SpringBone.Springs.Count == 0)
                {
                    ClothGuess.Guess(animator);
                    if (vrm.SpringBone.ColliderGroups.Count == 0)
                    {
                        HumanoidCollider.AddColliders(animator);
                        var warps = animator.GetComponentsInChildren<ClothWarpRoot>();
                        var colliderGroups = animator.GetComponentsInChildren<VRM10SpringBoneColliderGroup>();
                        foreach (var warp in warps)
                        {
                            warp.ColliderGroups = colliderGroups.ToList();
                        }
                    }
                }
                else
                {
                    ClothWarpRuntimeProvider.FromVrm10(vrm,
                        go => go.AddComponent<ClothWarpRoot>());
                }

                if (m_addClothToHips.isOn)
                {
                    if (animator.GetBoneTransform(HumanBodyBones.Hips) is var hips)
                    {
                        var cloth = hips.GetComponent<ClothGrid>();
                        if (cloth == null)
                        {
                            cloth = hips.gameObject.AddComponent<ClothGrid>();
                            cloth.Reset();
                            cloth.LoopIsClosed = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniGLTFLogger.Exception(ex);
            }
        }

        async void LoadModel(string path)
        {
            // cleanup
            m_loaded?.Dispose();
            m_loaded = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                UniGLTFLogger.Log($"{path}");
                var vrm10Instance = await Vrm10.LoadPathAsync(path,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    awaitCaller: m_useAsync.isOn
                        ? new RuntimeOnlyAwaitCaller()
                        : new ImmediateCaller(),
                    vrmMetaInformationCallback: m_texts.UpdateMeta,
                    springboneRuntime: m_useJob.isOn
                        ? new UniVRM10.ClothWarp.Jobs.ClothWarpJobRuntime(OnInit)
                        : new UniVRM10.ClothWarp.ClothWarpRuntime(OnInit)
                        );
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
                m_loaded = new Loaded(instance, m_target.transform);
                m_init = new ClothWarp.HumanoidPose(vrm10Instance.GetComponent<Animator>());
                m_showBoxMan.isOn = false;
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
}