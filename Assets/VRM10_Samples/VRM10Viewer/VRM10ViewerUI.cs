using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10ViewerUI : MonoBehaviour
    {
        [Header("UI")]
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

        [SerializeField]
        Toggle m_useAsync = default;

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

            public void UpdateMeta(Texture2D thumbnail, UniGLTF.Extensions.VRMC_vrm.Meta meta, Migration.Vrm0Meta meta0)
            {
                m_thumbnail.texture = thumbnail;

                if (meta != null)
                {
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
            m_open = buttons.First(x => x.name == "Open");

            var toggles = GameObject.FindObjectsOfType<Toggle>();
            m_enableLipSync = toggles.First(x => x.name == "EnableLipSync");
            m_enableAutoBlink = toggles.First(x => x.name == "EnableAutoBlink");
            m_enableAutoExpression = toggles.First(x => x.name == "EnableAutoExpression");

            var texts = GameObject.FindObjectsOfType<Text>();
            m_version = texts.First(x => x.name == "Version");
        }

        VRM10ViewerState m_state;

        private async void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}",
                VRMVersion.MAJOR, VRMVersion.MINOR);
            m_texts.Start();
            m_state = new VRM10ViewerState(() =>
            {
                m_ui.IsBvhEnabled = true;
            });
            m_open.onClick.AddListener(() =>
            {
                m_state.OpenFileDialog(m_useAsync.enabled, m_useUrpMaterial.isOn, m_texts.UpdateMeta);
            });

            string[] cmds = System.Environment.GetCommandLineArgs();
            for (int i = 1; i < cmds.Length; ++i)
            {
                if (File.Exists(cmds[i]))
                {
                    m_state.LoadModel(cmds[i], m_useAsync.enabled, m_useUrpMaterial.isOn, m_texts.UpdateMeta);
                }
            }
        }

        private void OnDestroy()
        {
            m_state.Dispose();
        }

        private void Update()
        {
            if (m_state.Loaded is Loaded loaded)
            {
                loaded.EnableLipSyncValue = m_enableLipSync.isOn;
                loaded.EnableBlinkValue = m_enableAutoBlink.isOn;
                loaded.EnableAutoExpressionValue = m_enableAutoExpression.isOn;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_state.ToggleActive();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                m_state.Cancel();
            }

            m_state.Update(m_ui.IsBvhEnabled);
        }
    }
}
