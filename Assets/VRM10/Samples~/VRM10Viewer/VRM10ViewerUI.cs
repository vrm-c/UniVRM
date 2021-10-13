using System;
using System.IO;
using System.Linq;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using UnityEngine.UI;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10ViewerUI : MonoBehaviour
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
        Toggle m_enableAutoExpression = default;

        [SerializeField]
        Toggle m_useUrpMaterial = default;
        #endregion

        [SerializeField]
        HumanPoseTransfer m_src = default;

        [SerializeField]
        GameObject m_target = default;

        [SerializeField]
        GameObject Root = default;

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

            public void UpdateMeta(Migration.Vrm0Meta meta, Texture2D thumbnail)
            {
                if (meta == null)
                {
                    return;
                }

                m_textModelTitle.text = meta.title;
                m_textModelVersion.text = meta.version;
                m_textModelAuthor.text = meta.author;
                m_textModelContact.text = meta.contactInformation;
                m_textModelReference.text = meta.reference;
                m_textPermissionAllowed.text = meta.allowedUser.ToString();
                m_textPermissionViolent.text = meta.violentUsage.ToString();
                m_textPermissionSexual.text = meta.sexualUsage.ToString();
                m_textPermissionCommercial.text = meta.commercialUsage.ToString();
                m_textPermissionOther.text = meta.otherPermissionUrl;

                // m_textDistributionLicense.text = meta.ModificationLicense.ToString();
                m_textDistributionOther.text = meta.otherLicenseUrl;

                m_thumbnail.texture = thumbnail;
            }

            public void UpdateMeta(UniGLTF.Extensions.VRMC_vrm.Meta meta, Texture2D thumbnail)
            {
                if (meta == null)
                {
                    return;
                }

                m_textModelTitle.text = meta.Name;
                m_textModelVersion.text = meta.Version;
                m_textModelAuthor.text = meta.Authors[0];
                m_textModelContact.text = meta.ContactInformation;
                if (meta.References != null && meta.References.Count > 0)
                {
                    m_textModelReference.text = meta.References[0];
                }
                // m_textPermissionAllowed.text = meta.AllowedUser.ToString();
                m_textPermissionViolent.text = meta.AllowExcessivelyViolentUsage.ToString();
                m_textPermissionSexual.text = meta.AllowExcessivelySexualUsage.ToString();
                m_textPermissionCommercial.text = meta.CommercialUsage.ToString();
                // m_textPermissionOther.text = meta.OtherPermissionUrl;

                // m_textDistributionLicense.text = meta.ModificationLicense.ToString();
                m_textDistributionOther.text = meta.OtherLicenseUrl;

                m_thumbnail.texture = thumbnail;
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

            var toggles = GameObject.FindObjectsOfType<Toggle>();
            m_enableLipSync = toggles.First(x => x.name == "EnableLipSync");
            m_enableAutoBlink = toggles.First(x => x.name == "EnableAutoBlink");
            m_enableAutoExpression = toggles.First(x => x.name == "EnableAutoExpression");

            var texts = GameObject.FindObjectsOfType<Text>();
            m_version = texts.First(x => x.name == "Version");

            m_src = GameObject.FindObjectOfType<HumanPoseTransfer>();

            m_target = GameObject.FindObjectOfType<VRM10TargetMover>().gameObject;
        }

        class Loaded : IDisposable
        {
            RuntimeGltfInstance m_instance;
            HumanPoseTransfer m_pose;
            Vrm10Instance m_controller;

            VRM10AIUEO m_lipSync;
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

            VRM10AutoExpression m_autoExpression;
            bool m_enableAutoExpressionValue;
            public bool EnableAutoExpressionValue
            {
                set
                {
                    if (m_enableAutoExpressionValue == value) return;
                    m_enableAutoExpressionValue = value;
                    if (m_autoExpression != null)
                    {
                        m_autoExpression.enabled = m_enableAutoExpressionValue;
                    }
                }
            }

            VRM10Blinker m_blink;
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

            public Loaded(RuntimeGltfInstance instance, HumanPoseTransfer src, Transform lookAtTarget)
            {
                m_instance = instance;

                m_controller = instance.GetComponent<Vrm10Instance>();
                if (m_controller != null)
                {
                    // VRM
                    m_controller.UpdateType = Vrm10Instance.UpdateTypes.LateUpdate; // after HumanPoseTransfer's setPose
                    {
                        m_pose = instance.gameObject.AddComponent<HumanPoseTransfer>();
                        m_pose.Source = src;
                        m_pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;

                        m_lipSync = instance.gameObject.AddComponent<VRM10AIUEO>();
                        m_blink = instance.gameObject.AddComponent<VRM10Blinker>();
                        m_autoExpression = instance.gameObject.AddComponent<VRM10AutoExpression>();

                        m_controller.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.CalcYawPitchToGaze;
                        m_controller.Gaze = lookAtTarget;
                    }
                }

                var animation = instance.GetComponent<Animation>();
                if (animation && animation.clip != null)
                {
                    // GLTF animation
                    animation.Play(animation.clip.name);
                }
            }

            public void Dispose()
            {
                // destroy GameObject
                GameObject.Destroy(m_instance.gameObject);
            }

            public void EnableBvh(HumanPoseTransfer src)
            {
                if (m_pose != null)
                {
                    m_pose.Source = src;
                    m_pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;
                }
            }

            public void EnableTPose(HumanPoseClip pose)
            {
                if (m_pose != null)
                {
                    m_pose.PoseClip = pose;
                    m_pose.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseClip;
                }
            }
        }
        Loaded m_loaded;

        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                VRMVersion.MAJOR, VRMVersion.MINOR);
            m_open.onClick.AddListener(OnOpenClicked);

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
            if (m_loaded != null)
            {
                m_loaded.EnableLipSyncValue = m_enableLipSync.isOn;
                m_loaded.EnableBlinkValue = m_enableAutoBlink.isOn;
                m_loaded.EnableAutoExpressionValue = m_enableAutoExpression.isOn;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Root != null) Root.SetActive(!Root.activeSelf);
            }

            m_ui.UpdateToggle(() => m_loaded?.EnableBvh(m_src), () => m_loaded?.EnableTPose(m_pose));
        }

        void OnOpenClicked()
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open VRM", "vrm", "glb", "bvh", "gltf", "zip");
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
            switch (ext)
            {
                case ".gltf":
                case ".glb":
                case ".vrm":
                case ".zip":
                    LoadModel(path);
                    break;

                case ".bvh":
                    LoadMotion(path);
                    break;
            }
        }

        static IMaterialDescriptorGenerator GetVrmMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new Vrm10UrpMaterialDescriptorGenerator();
            }
            else
            {
                return new Vrm10MaterialDescriptorGenerator();
            }
        }

        static IMaterialDescriptorGenerator GetMaterialDescriptorGenerator(bool useUrp)
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

        async void LoadModel(string path)
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
                Debug.LogWarning(ex);
                return;
            }

            if (Vrm10Data.TryParseOrMigrate(data, doMigrate: true, out Vrm10Data vrm))
            {
                // vrm
                using (var loader = new Vrm10Importer(vrm, materialGenerator: GetVrmMaterialDescriptorGenerator(m_useUrpMaterial.isOn)))
                {
                    // migrate しても thumbnail は同じ
                    var thumbnail = await loader.LoadVrmThumbnailAsync();

                    if (vrm.OriginalMetaBeforeMigration != null)
                    {
                        // migrated from vrm-0.x. use OldMeta
                        m_texts.UpdateMeta(vrm.OriginalMetaBeforeMigration, thumbnail);
                    }
                    else
                    {
                        // load vrm-1.0. use newMeta
                        m_texts.UpdateMeta(vrm.VrmExtension.Meta, thumbnail);
                    }

                    var instance = await loader.LoadAsync();
                    SetModel(instance);
                }
            }
            else
            {
                // gltf
                using (var loader = new UniGLTF.ImporterContext(data, materialGenerator: GetMaterialDescriptorGenerator(m_useUrpMaterial.isOn)))
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

            instance.ShowMeshes();
            instance.EnableUpdateWhenOffscreen();
            m_loaded = new Loaded(instance, m_src, m_target.transform);
        }

        void SetMotion(HumanPoseTransfer src)
        {
            m_src = src;
            src.GetComponent<Renderer>().enabled = false;

            if (m_loaded != null)
            {
                m_loaded.EnableBvh(m_src);
            }
        }
    }
}
