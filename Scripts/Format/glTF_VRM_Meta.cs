using System;
using UniGLTF;


namespace VRM
{
    public enum AllowedUser
    {
        OnlyAuthor,
        Everyone,
    }


    public enum LicenseType
    {
        RedistributionProhibited,

        CC0,
        CC_BY,
        CC_BY_NC,
        CC_BY_SA,
        CC_BY_NC_SA,
        CC_BY_ND,
        CC_BY_NC_ND,

        Other
    }


    [Serializable]
    public class glTF_VRM_Meta : JsonSerializableBase
    {
        public string version;

        public string author;
        public string contactInformation;
        public string reference;

        public string title;
        public int texture = -1;

        #region PersonationCharacterizationPermission Permission;
        public string allowedUserName;
        public AllowedUser allowedUser
        {
            get
            {
                try
                {
                    return (AllowedUser)Enum.Parse(typeof(AllowedUser), allowedUserName, true);
                }
                catch (Exception)
                {
                    return AllowedUser.OnlyAuthor;
                }
            }
            set
            {
                allowedUserName = value.ToString();
            }
        }

        public bool allowImmoralUssage;
        public bool allowCcertainBeliefsUssage;
        public bool allowPoliticalUssage;
        public bool allowCommercialUssage;
        #endregion

        #region Distribution License
        public string licenseName;
        public LicenseType licenseType
        {
            get
            {
                try
                {
                    return (LicenseType)Enum.Parse(typeof(LicenseType), licenseName, true);
                }
                catch (Exception)
                {
                    return default(LicenseType);
                }
            }
            set
            {
                licenseName = value.ToString();
            }
        }
        public string otherLicenseUrl;
        #endregion

        protected override void SerializeMembers(JsonFormatter f)
        {
            f.KeyValue(() => version);

            f.KeyValue(() => author);
            f.KeyValue(() => contactInformation);
            f.KeyValue(() => reference);

            f.KeyValue(() => title);
            f.KeyValue(() => texture);

            f.KeyValue(() => allowedUserName);
            f.KeyValue(() => allowImmoralUssage);
            f.KeyValue(() => allowCcertainBeliefsUssage);
            f.KeyValue(() => allowPoliticalUssage);
            f.KeyValue(() => allowCommercialUssage);

            f.KeyValue(() => licenseName);
            f.KeyValue(() => otherLicenseUrl);
        }
    }
}
