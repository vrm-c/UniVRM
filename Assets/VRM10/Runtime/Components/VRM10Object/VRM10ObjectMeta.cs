using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;


namespace UniVRM10
{
    [Serializable]
    public class VRM10ObjectMeta
    {
        #region Info
        [SerializeField]
        public string Name;

        [SerializeField]
        public string Version;

        [SerializeField]
        public List<string> Authors = new List<string>();

        [SerializeField]
        public string CopyrightInformation;

        [SerializeField]
        public string ContactInformation;

        [SerializeField]
        public List<string> References = new List<string>();

        [SerializeField]
        public string ThirdPartyLicenses;

        [SerializeField]
        public Texture2D Thumbnail;
        #endregion

        #region AvatarPermission
        [SerializeField, Tooltip("A person who can perform with this avatar")]
        public UniGLTF.Extensions.VRMC_vrm.AvatarPermissionType AvatarPermission;

        [SerializeField, Tooltip("Violent acts using this avatar")]
        public bool ViolentUsage;

        [SerializeField, Tooltip("Sexuality acts using this avatar")]
        public bool SexualUsage;

        [SerializeField, Tooltip("For commercial use")]
        public UniGLTF.Extensions.VRMC_vrm.CommercialUsageType CommercialUsage;

        [SerializeField]
        public bool PoliticalOrReligiousUsage;

        [SerializeField]
        public bool AntisocialOrHateUsage;
        #endregion

        #region Distribution License
        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.CreditNotationType CreditNotation;

        [SerializeField]
        public bool Redistribution;

        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.ModificationType Modification;

        [SerializeField]
        public string OtherLicenseUrl;
        #endregion

        public IEnumerable<Validation> Validate(GameObject _ = null)
        {
            if (string.IsNullOrEmpty(Name))
            {
                yield return Validation.Error("Require Name. ");
            }

            if (Authors == null || Authors.Count == 0)
            {
                yield return Validation.Error("Require at leaset one Author.");
            }

            if (Authors.All(x => string.IsNullOrWhiteSpace(x)))
            {
                yield return Validation.Error("All Authors is whitespace");
            }
        }

        public void CopyTo(VRM10ObjectMeta dst)
        {
            dst.Name = Name;
            dst.Version = Version;
            dst.CopyrightInformation = CopyrightInformation;
            if (Authors != null)
            {
                dst.Authors = Authors.Select(x => x).ToList();
            }
            else
            {
                dst.Authors = new List<string>();
            }
            dst.ContactInformation = ContactInformation;
            dst.References = References;
            dst.ThirdPartyLicenses = ThirdPartyLicenses;
            dst.Thumbnail = Thumbnail;
            dst.AvatarPermission = AvatarPermission;
            dst.ViolentUsage = ViolentUsage;
            dst.SexualUsage = SexualUsage;
            dst.CommercialUsage = CommercialUsage;
            dst.PoliticalOrReligiousUsage = PoliticalOrReligiousUsage;
            dst.CreditNotation = CreditNotation;
            dst.Redistribution = Redistribution;
            dst.Modification = Modification;
            dst.OtherLicenseUrl = OtherLicenseUrl;
        }
    }
}
