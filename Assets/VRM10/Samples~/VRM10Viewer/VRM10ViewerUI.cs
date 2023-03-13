using System;
using System.IO;
using System.Linq;
using System.Threading;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10ViewerUI : MonoBehaviour
    {
        [SerializeField]
        Text m_version = default;

        [Header("UI")]
        [SerializeField]
        Button m_openModel = default;

        [SerializeField]
        Button m_openMotion = default;

        [SerializeField]
        Toggle m_showBoxMan = default;

        [SerializeField]
        Toggle m_enableLipSync = default;

        [SerializeField]
        Toggle m_enableAutoBlink = default;

        [SerializeField]
        Toggle m_enableAutoExpression = default;

        [SerializeField]
        Toggle m_useUrpMaterial = default;

        [SerializeField]
        Toggle m_useAsync = default;

        [SerializeField]
        GameObject m_target = default;

        [SerializeField]
        TextAsset m_motion;

        GameObject Root = default;

        IMotion m_src = default;
        public IMotion Motion
        {
            get { return m_src; }
            set
            {
                if (m_src != null)
                {
                    m_src.Dispose();
                }
                m_src = value;
            }
        }

        private CancellationTokenSource _cancellationTokenSource;

        [Serializable]
        class TextFields
        {
            [SerializeField]
            Text m_textModelTitle = default;
            [SerializeField]
            Text m_textModelVersion = default;
            [SerializeField]
            Text m_textModelAuthor = default;
            [SerializeField]
            Text m_textModelCopyright = default;
            [SerializeField]
            Text m_textModelContact = default;
            [SerializeField]
            Text m_textModelReference = default;
            [SerializeField]
            RawImage m_thumbnail = default;

            [SerializeField, Header("CharacterPermission")]
            Text m_textPermissionAllowed = default;
            [SerializeField]
            Text m_textPermissionViolent = default;
            [SerializeField]
            Text m_textPermissionSexual = default;
            [SerializeField]
            Text m_textPermissionCommercial = default;
            [SerializeField]
            Text m_textPermissionOther = default;

            [SerializeField, Header("DistributionLicense")]
            Text m_textDistributionLicense = default;
            [SerializeField]
            Text m_textDistributionOther = default;

            public void Reset()
            {
                var texts = GameObject.FindObjectsOfType<Text>();
                m_textModelTitle = texts.First(x => x.name == "Title (1)");
                m_textModelVersion = texts.First(x => x.name == "Version (1)");
                m_textModelAuthor = texts.First(x => x.name == "Author (1)");
                m_textModelCopyright = texts.First(x => x.name == "Copyright (1)");
                m_textModelContact = texts.First(x => x.name == "Contact (1)");
                m_textModelReference = texts.First(x => x.name == "Reference (1)");

                m_textPermissionAllowed = texts.First(x => x.name == "AllowedUser (1)");
                m_textPermissionViolent = texts.First(x => x.name == "Violent (1)");
                m_textPermissionSexual = texts.First(x => x.name == "Sexual (1)");
                m_textPermissionCommercial = texts.First(x => x.name == "Commercial (1)");
                m_textPermissionOther = texts.First(x => x.name == "Other (1)");

                m_textDistributionLicense = texts.First(x => x.name == "LicenseType (1)");
                m_textDistributionOther = texts.First(x => x.name == "OtherLicense (1)");

                var images = GameObject.FindObjectsOfType<RawImage>();
                m_thumbnail = images.First(x => x.name == "RawImage");
            }

            public void Start()
            {
                m_textModelTitle.text = "";
                m_textModelVersion.text = "";
                m_textModelAuthor.text = "";
                m_textModelCopyright.text = "";
                m_textModelContact.text = "";
                m_textModelReference.text = "";

                m_textPermissionAllowed.text = "";
                m_textPermissionViolent.text = "";
                m_textPermissionSexual.text = "";
                m_textPermissionCommercial.text = "";
                m_textPermissionOther.text = "";

                m_textDistributionLicense.text = "";
                m_textDistributionOther.text = "";
            }

            public void UpdateMeta(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta, Migration.Vrm0Meta meta0)
            {
                m_thumbnail.texture = thumbnail;

                if (meta != null)
                {
                    m_textModelTitle.text = meta.Name;
                    m_textModelVersion.text = meta.Version;
                    m_textModelAuthor.text = meta.Authors[0];
                    m_textModelCopyright.text = meta.CopyrightInformation;
                    m_textModelContact.text = meta.ContactInformation;
                    if (meta.References != null && meta.References.Count > 0)
                    {
                        m_textModelReference.text = meta.References[0];
                    }
                    m_textPermissionAllowed.text = meta.AvatarPermission.ToString();
                    m_textPermissionViolent.text = meta.AllowExcessivelyViolentUsage.ToString();
                    m_textPermissionSexual.text = meta.AllowExcessivelySexualUsage.ToString();
                    m_textPermissionCommercial.text = meta.CommercialUsage.ToString();
                    // m_textPermissionOther.text = meta.OtherPermissionUrl;

                    // m_textDistributionLicense.text = meta.ModificationLicense.ToString();
                    m_textDistributionOther.text = meta.OtherLicenseUrl;
                }

                if (meta0 != null)
                {
                    m_textModelTitle.text = meta0.title;
                    m_textModelVersion.text = meta0.version;
                    m_textModelAuthor.text = meta0.author;
                    m_textModelContact.text = meta0.contactInformation;
                    m_textModelReference.text = meta0.reference;
                    m_textPermissionAllowed.text = meta0.allowedUser.ToString();
                    m_textPermissionViolent.text = meta0.violentUsage.ToString();
                    m_textPermissionSexual.text = meta0.sexualUsage.ToString();
                    m_textPermissionCommercial.text = meta0.commercialUsage.ToString();
                    m_textPermissionOther.text = meta0.otherPermissionUrl;
                    // m_textDistributionLicense.text = meta0.ModificationLicense.ToString();
                    m_textDistributionOther.text = meta0.otherLicenseUrl;
                }
            }
        }
        [SerializeField]
        TextFields m_texts = default;

        [Serializable]
        class UIFields
        {
            [SerializeField]
            Toggle ToggleMotionTPose = default;

            [SerializeField]
            Toggle ToggleMotionBVH = default;

            [SerializeField]
            ToggleGroup ToggleMotion = default;

            public void Reset()
            {
                var toggles = GameObject.FindObjectsOfType<Toggle>();
                ToggleMotionTPose = toggles.First(x => x.name == "TPose");
                ToggleMotionBVH = toggles.First(x => x.name == "BVH");

                var groups = GameObject.FindObjectsOfType<ToggleGroup>();
                ToggleMotion = groups.First(x => x.name == "_Motion_");
            }

            public bool IsBvhEnabled
            {
                get => ToggleMotion.ActiveToggles().FirstOrDefault() == ToggleMotionBVH;
                set
                {
                    ToggleMotionTPose.isOn = !value;
                    ToggleMotionBVH.isOn = value;
                }
            }
        }
        [SerializeField]
        UIFields m_ui = default;

        private void Reset()
        {
            var buttons = GameObject.FindObjectsOfType<Button>();
            m_openModel = buttons.First(x => x.name == "OpenModel");
            m_openMotion = buttons.First(x => x.name == "OpenMotion");

            var toggles = GameObject.FindObjectsOfType<Toggle>();
            m_showBoxMan = toggles.First(x => x.name == "ShowBoxMan");
            m_enableLipSync = toggles.First(x => x.name == "EnableLipSync");
            m_enableAutoBlink = toggles.First(x => x.name == "EnableAutoBlink");
            m_enableAutoExpression = toggles.First(x => x.name == "EnableAutoExpression");
            m_useUrpMaterial = toggles.First(x => x.name == "UseUrpMaterial");
            m_useAsync = toggles.First(x => x.name == "UseAsync");

            var texts = GameObject.FindObjectsOfType<Text>();
            m_version = texts.First(x => x.name == "VrmVersion");

            m_texts.Reset();
            m_ui.Reset();

            m_target = GameObject.FindObjectOfType<VRM10TargetMover>().gameObject;
        }

        Loaded m_loaded;

        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                    VRMVersion.MAJOR, VRMVersion.MINOR);

            m_openModel.onClick.AddListener(OnOpenModelClicked);

            m_openMotion.onClick.AddListener(OnOpenMotionClicked);

            // load initial bvh
            if (m_motion != null)
            {
                Motion = BvhMotion.LoadBvhFromText(m_motion.text);
            }

            string[] cmds = System.Environment.GetCommandLineArgs();
            for (int i = 1; i < cmds.Length; ++i)
            {
                if (File.Exists(cmds[i]))
                {
                    LoadModel(cmds[i]);
                }
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
                if (Root != null) Root.SetActive(!Root.activeSelf);
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

                if (m_ui.IsBvhEnabled && Motion != null)
                {
                    VRM10Retarget.Retarget(Motion.ControlRig, (m_loaded.ControlRig, m_loaded.ControlRig));
                }
                else
                {
                    VRM10Retarget.EnforceTPose((m_loaded.ControlRig, m_loaded.ControlRig));
                }
            }
        }

        void OnOpenModelClicked()
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

            LoadModel(path);
        }

        async void OnOpenMotionClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open Motion", "bvh", "gltf", "glb");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open Motion", "", "bvh");
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
            Motion = await VrmAnimation.LoadVrmAnimationFromPathAsync(path);
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
                Debug.LogFormat("{0}", path);
                var vrm10Instance = await Vrm10.LoadPathAsync(path,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    awaitCaller: m_useAsync.enabled ? (IAwaitCaller)new RuntimeOnlyAwaitCaller() : (IAwaitCaller)new ImmediateCaller(),
                    materialGenerator: GetVrmMaterialDescriptorGenerator(m_useUrpMaterial.isOn),
                    vrmMetaInformationCallback: m_texts.UpdateMeta,
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
    }
}
