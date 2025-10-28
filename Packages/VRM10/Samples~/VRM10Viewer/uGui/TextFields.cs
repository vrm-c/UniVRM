using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UniVRM10.VRM10Viewer
{
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

        public void Reset(ObjectMap map)
        {
            m_textModelTitle = map.Get<Text>("Title (1)");
            m_textModelVersion = map.Get<Text>("Version (1)");
            m_textModelAuthor = map.Get<Text>("Author (1)");
            m_textModelCopyright = map.Get<Text>("Copyright (1)");
            m_textModelContact = map.Get<Text>("Contact (1)");
            m_textModelReference = map.Get<Text>("Reference (1)");

            m_textPermissionAllowed = map.Get<Text>("AllowedUser (1)");
            m_textPermissionViolent = map.Get<Text>("Violent (1)");
            m_textPermissionSexual = map.Get<Text>("Sexual (1)");
            m_textPermissionCommercial = map.Get<Text>("Commercial (1)");
            m_textPermissionOther = map.Get<Text>("Other (1)");

            m_textDistributionLicense = map.Get<Text>("LicenseType (1)");
            m_textDistributionOther = map.Get<Text>("OtherLicense (1)");

#if UNITY_2022_3_OR_NEWER
            var images = GameObject.FindObjectsByType<RawImage>(FindObjectsSortMode.InstanceID);
#else
                var images = GameObject.FindObjectsOfType<RawImage>();
#endif
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
}