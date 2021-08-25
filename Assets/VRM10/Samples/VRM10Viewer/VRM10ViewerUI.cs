using System;
using System.IO;
using System.Linq;
using UniGLTF;
using UniHumanoid;
using UnityEngine;
using UnityEngine.UI;


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

            public void UpdateMeta(VRM10ObjectMeta meta)
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
                m_textPermissionAllowed.text = meta.AllowedUser.ToString();
                m_textPermissionViolent.text = meta.ViolentUsage.ToString();
                m_textPermissionSexual.text = meta.SexualUsage.ToString();
                m_textPermissionCommercial.text = meta.CommercialUsage.ToString();
                m_textPermissionOther.text = meta.OtherPermissionUrl;

                m_textDistributionLicense.text = meta.ModificationLicense.ToString();
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

            var toggles = GameObject.FindObjectsOfType<Toggle>();
            m_enableLipSync = toggles.First(x => x.name == "EnableLipSync");
            m_enableAutoBlink = toggles.First(x => x.name == "EnableAutoBlink");
            m_enableAutoExpression = toggles.First(x => x.name == "EnableAutoExpression");

            var texts = GameObject.FindObjectsOfType<Text>();
            m_version = texts.First(x => x.name == "Version");

            m_src = GameObject.FindObjectOfType<HumanPoseTransfer>();

            m_target = GameObject.FindObjectOfType<VRM10TargetMover>().gameObject;
        }

        HumanPoseTransfer m_loaded;
        VRM10Controller m_controller;

        VRM10AIUEO m_lipSync;
        bool m_enableLipSyncValue;
        bool EnableLipSyncValue
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
        bool EnableAutoExpressionValue
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
        bool EnableBlinkValue
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
            EnableLipSyncValue = m_enableLipSync.isOn;
            EnableBlinkValue = m_enableAutoBlink.isOn;
            EnableAutoExpressionValue = m_enableAutoExpression.isOn;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Root != null) Root.SetActive(!Root.activeSelf);
            }

            m_ui.UpdateToggle(EnableBvh, EnableTPose);

            // if (m_controller != null)
            // {
            //     m_controller.Expression.Apply();
            // }
        }

        void EnableBvh()
        {
            if (m_loaded != null)
            {
                m_loaded.Source = m_src;
                m_loaded.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;
            }
        }

        void EnableTPose()
        {
            if (m_loaded != null)
            {
                m_loaded.PoseClip = m_pose;
                m_loaded.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseClip;
            }
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

        void LoadModel(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            Debug.LogFormat("{0}", path);
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".vrm":
                    {
                        if (!Vrm10Parser.TryParseOrMigrate(path, doMigrate: true, out Vrm10Parser.Result result))
                        {
                            Debug.LogError(result.Message);
                            return;
                        }
                        using (var loader = new Vrm10Importer(result.Data, result.Vrm))
                        {
                            var loaded = loader.Load();
                            loaded.ShowMeshes();
                            loaded.EnableUpdateWhenOffscreen();
                            SetModel(loaded.gameObject);
                        }
                        break;
                    }

                case ".glb":
                    {
                        var data = new GlbFileParser(path).Parse();

                        using (var loader = new UniGLTF.ImporterContext(data))
                        {
                            var loaded = loader.Load();
                            loaded.ShowMeshes();
                            loaded.EnableUpdateWhenOffscreen();
                            SetModel(loaded.gameObject);
                        }
                        break;
                    }

                case ".gltf":
                    {
                        var data = new GltfFileWithResourceFilesParser(path).Parse();

                        using (var loader = new UniGLTF.ImporterContext(data))
                        {
                            var loaded = loader.Load();
                            loaded.ShowMeshes();
                            loaded.EnableUpdateWhenOffscreen();
                            SetModel(loaded.gameObject);
                        }
                        break;
                    }
                case ".zip":
                    {
                        var data = new ZipArchivedGltfFileParser(path).Parse();

                        using (var loader = new UniGLTF.ImporterContext(data))
                        {
                            var loaded = loader.Load();
                            loaded.ShowMeshes();
                            loaded.EnableUpdateWhenOffscreen();
                            SetModel(loaded.gameObject);
                        }
                        break;
                    }

                default:
                    Debug.LogWarningFormat("unknown file type: {0}", path);
                    break;
            }
        }

        void SetModel(GameObject go)
        {
            // cleanup
            var loaded = m_loaded;
            m_loaded = null;

            if (loaded != null)
            {
                Debug.LogFormat("destroy {0}", loaded);
                GameObject.Destroy(loaded.gameObject);
            }

            if (go != null)
            {
                m_controller = go.GetComponent<VRM10Controller>();
                if (m_controller != null)
                {

                    m_texts.UpdateMeta(m_controller.Vrm.Meta);

                    m_controller.UpdateType = VRM10Controller.UpdateTypes.LateUpdate; // after HumanPoseTransfer's setPose
                    {
                        m_loaded = go.AddComponent<HumanPoseTransfer>();
                        m_loaded.Source = m_src;
                        m_loaded.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;

                        m_lipSync = go.AddComponent<VRM10AIUEO>();
                        m_blink = go.AddComponent<VRM10Blinker>();
                        m_autoExpression = go.AddComponent<VRM10AutoExpression>();

                        m_controller.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.CalcYawPitchToGaze;
                        m_controller.Gaze = m_target.transform;
                    }
                }

                var animation = go.GetComponent<Animation>();
                if (animation && animation.clip != null)
                {
                    animation.Play(animation.clip.name);
                }
            }
        }

        void SetMotion(HumanPoseTransfer src)
        {
            m_src = src;
            src.GetComponent<Renderer>().enabled = false;

            EnableBvh();
        }
    }
}
