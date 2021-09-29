using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using UnityEngine.UI;
using VRMShaders;


namespace VRM.SimpleViewer
{


    public class ViewerUI : MonoBehaviour
    {
        #region UI
        [SerializeField]
        Text m_version = default;

        [SerializeField]
        Button m_open = default;

        [SerializeField]
        Toggle m_enableLipSync = default;

        [SerializeField]
        Toggle m_enableAutoBlink = default;

        [SerializeField]
        Toggle m_useUrpMaterial = default;
        
        [SerializeField]
        Toggle m_useFastSpringBone = default;
        #endregion

        [SerializeField]
        HumanPoseTransfer m_src = default;

        [SerializeField]
        GameObject m_target = default;

        [SerializeField]
        GameObject Root = default;

        [SerializeField]
        Button m_reset = default;

        [SerializeField]
        TextAsset m_motion;

        [Serializable]
        class TextFields
        {
            [SerializeField, Header("Info")]
            Text m_textModelTitle = default;
            [SerializeField]
            Text m_textModelVersion = default;
            [SerializeField]
            Text m_textModelAuthor = default;
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

            public void Start()
            {
                m_textModelTitle.text = "";
                m_textModelVersion.text = "";
                m_textModelAuthor.text = "";
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

            public async Task UpdateMetaAsync(VRMImporterContext context)
            {
                var meta = await context.ReadMetaAsync(new ImmediateCaller(), true);

                m_textModelTitle.text = meta.Title;
                m_textModelVersion.text = meta.Version;
                m_textModelAuthor.text = meta.Author;
                m_textModelContact.text = meta.ContactInformation;
                m_textModelReference.text = meta.Reference;

                m_textPermissionAllowed.text = meta.AllowedUser.ToString();
                m_textPermissionViolent.text = meta.ViolentUssage.ToString();
                m_textPermissionSexual.text = meta.SexualUssage.ToString();
                m_textPermissionCommercial.text = meta.CommercialUssage.ToString();
                m_textPermissionOther.text = meta.OtherPermissionUrl;

                m_textDistributionLicense.text = meta.LicenseType.ToString();
                m_textDistributionOther.text = meta.OtherLicenseUrl;

                m_thumbnail.texture = meta.Thumbnail;
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

            Toggle m_activeToggleMotion = default;

            public void UpdateToggle(Action onBvh, Action onTPose)
            {
                var value = ToggleMotion.ActiveToggles().FirstOrDefault();
                if (value == m_activeToggleMotion)
                    return;

                m_activeToggleMotion = value;
                if (value == ToggleMotionTPose)
                {
                    onTPose();
                }
                else if (value == ToggleMotionBVH)
                {
                    onBvh();
                }
                else
                {
                    Debug.Log("motion: no toggle");
                }
            }
        }
        [SerializeField]
        UIFields m_ui = default;

        [SerializeField]
        HumanPoseClip m_pose = default;

        private void Reset()
        {
            var buttons = GameObject.FindObjectsOfType<Button>();
            m_open = buttons.First(x => x.name == "Open");

            m_reset = buttons.First(x => x.name == "ResetSpringBone");

            var toggles = GameObject.FindObjectsOfType<Toggle>();
            m_useFastSpringBone = toggles.First(x => x.name == "UseFastSpringBone");
            m_enableLipSync = toggles.First(x => x.name == "EnableLipSync");
            m_enableAutoBlink = toggles.First(x => x.name == "EnableAutoBlink");

            var texts = GameObject.FindObjectsOfType<Text>();
            m_version = texts.First(x => x.name == "Version");

            m_src = GameObject.FindObjectOfType<HumanPoseTransfer>();

            m_target = GameObject.FindObjectOfType<TargetMover>().gameObject;
        }

        class Loaded : IDisposable
        {
            RuntimeGltfInstance _instance;
            HumanPoseTransfer _pose;
            VRMBlendShapeProxy m_proxy;

            Blinker m_blink;
            bool m_enableBlinkValue;
            public bool EnableBlinkValue
            {
                set
                {
                    if (m_blink == value) return;
                    m_enableBlinkValue = value;
                    if (m_blink != null)
                    {
                        m_blink.enabled = m_enableBlinkValue;
                    }
                }
            }

            AIUEO m_lipSync;
            bool m_enableLipSyncValue;
            public bool EnableLipSyncValue
            {
                set
                {
                    if (m_enableLipSyncValue == value) return;
                    m_enableLipSyncValue = value;
                    if (m_lipSync != null)
                    {
                        m_lipSync.enabled = m_enableLipSyncValue;
                    }
                }
            }

            public Loaded(RuntimeGltfInstance instance, HumanPoseTransfer src, Transform lookAtTarget)
            {
                _instance = instance;

                var lookAt = instance.GetComponent<VRMLookAtHead>();
                if (lookAt != null)
                {
                    // vrm
                    _pose = _instance.gameObject.AddComponent<HumanPoseTransfer>();
                    _pose.Source = src;
                    _pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;

                    m_lipSync = instance.gameObject.AddComponent<AIUEO>();
                    m_blink = instance.gameObject.AddComponent<Blinker>();

                    lookAt.Target = lookAtTarget;
                    lookAt.UpdateType = UpdateType.LateUpdate; // after HumanPoseTransfer's setPose

                    m_proxy = instance.GetComponent<VRMBlendShapeProxy>();
                }

                // not vrm
                var animation = instance.GetComponent<Animation>();
                if (animation && animation.clip != null)
                {
                    animation.Play(animation.clip.name);
                }
            }

            public void Dispose()
            {
                // Destroy game object. not RuntimeGltfInstance
                GameObject.Destroy(_instance.gameObject);
            }

            public void EnableBvh(HumanPoseTransfer src)
            {
                if (_pose != null)
                {
                    _pose.Source = src;
                    _pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;
                }
            }

            public void EnableTPose(HumanPoseClip pose)
            {
                if (_pose != null)
                {
                    _pose.PoseClip = pose;
                    _pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseClip;
                }
            }

            public void OnResetClicked()
            {
                if (_pose != null)
                {
                    foreach (var spring in _pose.GetComponentsInChildren<VRMSpringBone>())
                    {
                        spring.Setup();
                    }
                }
            }

            public void Update()
            {
                if (m_proxy != null)
                {
                    m_proxy.Apply();
                }
            }
        }
        Loaded m_loaded;



        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                VRMVersion.MAJOR, VRMVersion.MINOR);
            m_open.onClick.AddListener(OnOpenClicked);
            m_useFastSpringBone.onValueChanged.AddListener(OnUseFastSpringBoneValueChanged);
            OnUseFastSpringBoneValueChanged(m_useFastSpringBone.isOn);

            m_reset.onClick.AddListener(() => m_loaded.OnResetClicked());

            // load initial bvh
            if (m_motion != null)
            {
                LoadMotion(m_motion.text);
            }

            string[] cmds = System.Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                LoadModel(cmds[1]);
            }

            m_texts.Start();
        }

        private void LoadMotion(string source)
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse("tmp.bvh", source);
            context.Load();
            SetMotion(context.Root.GetComponent<HumanPoseTransfer>());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Root != null) Root.SetActive(!Root.activeSelf);
            }

            m_ui.UpdateToggle(() => m_loaded?.EnableBvh(m_src), () => m_loaded?.EnableTPose(m_pose));

            if (m_loaded != null)
            {
                m_loaded.EnableLipSyncValue = m_enableLipSync.isOn;
                m_loaded.EnableBlinkValue = m_enableAutoBlink.isOn;
                m_loaded.Update();
            }
        }

        void OnOpenClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = FileDialogForWindows.FileDialog("open VRM", "vrm", "glb", "bvh", "gltf", "zip");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            LoadModel(path);
        }

        void LoadModel(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".gltf":
                case ".glb":
                case ".zip":
                    LoadModelAsync(path, false);
                    break;

                case ".vrm":
                    LoadModelAsync(path, true);
                    break;

                case ".bvh":
                    LoadMotion(path);
                    break;
            }
        }

        void OnUseFastSpringBoneValueChanged(bool flag)
        {
            m_reset.gameObject.SetActive(!flag);
        }

        static IMaterialDescriptorGenerator GetGltfMaterialGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new GltfUrpMaterialDescriptorGenerator();
            }
            else
            {
                return new GltfMaterialDescriptorGenerator();
            }
        }

        static IMaterialDescriptorGenerator GetVrmMaterialGenerator(bool useUrp, VRM.glTF_VRM_extensions vrm)
        {
            if (useUrp)
            {
                return new VRM.VRMUrpMaterialDescriptorGenerator(vrm);
            }
            else
            {
                return new VRM.VRMMaterialDescriptorGenerator(vrm);
            }
        }

        async void LoadModelAsync(string path, bool isVrm)
        {
            if (!File.Exists(path))
            {
                return;
            }

            Debug.LogFormat("{0}", path);

            GltfData data;
            try
            {
                data = new AutoGltfFileParser(path).Parse();
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("parse error: {0}", ex);
                return;
            }

            if (isVrm)
            {
                try
                {
                    var vrm = new VRMData(data);
                    using (var loader = new VRMImporterContext(vrm, materialGenerator: GetVrmMaterialGenerator(m_useUrpMaterial.isOn, vrm.VrmExtension)))
                    {
                        await m_texts.UpdateMetaAsync(loader);
                        var instance = await loader.LoadAsync();
                        SetModel(instance);
                    }
                }
                catch (NotVrm0Exception)
                {
                    // retry
                    Debug.LogWarning("file extension is vrm. but not vrm ?");
                    using (var loader = new UniGLTF.ImporterContext(data, materialGenerator: GetGltfMaterialGenerator(m_useUrpMaterial.isOn)))
                    {
                        var instance = await loader.LoadAsync();
                        SetModel(instance);
                    }
                }
            }
            else
            {
                using (var loader = new UniGLTF.ImporterContext(data, materialGenerator: GetGltfMaterialGenerator(m_useUrpMaterial.isOn)))
                {
                    var instance = await loader.LoadAsync();
                    SetModel(instance);
                }
            }
        }

        void SetModel(RuntimeGltfInstance instance)
        {
            // cleanup
            if (m_loaded != null)
            {
                m_loaded.Dispose();
                m_loaded = null;
            }

            if (m_useFastSpringBone.isOn)
            {
                FastSpringBoneReplacer.ReplaceAsync(instance.Root);
            }

            instance.EnableUpdateWhenOffscreen();
            instance.ShowMeshes();

            m_loaded = new Loaded(instance, m_src, m_target.transform);
        }

        void SetMotion(HumanPoseTransfer src)
        {
            m_src = src;
            src.GetComponent<Renderer>().enabled = false;
            if (m_loaded != null)
            {
                m_loaded.EnableBvh(src);
            }
        }
    }
}
