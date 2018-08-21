using System;
using UniGLTF;
using UniJSON;

namespace VRM
{
    public enum AllowedUser
    {
        OnlyAuthor,
        ExplicitlyLicensedPerson,
        Everyone,
    }

    public enum LicenseType
    {
        Redistribution_Prohibited,
        CC0,
        CC_BY,
        CC_BY_NC,
        CC_BY_SA,
        CC_BY_NC_SA,
        CC_BY_ND,
        CC_BY_NC_ND,
        Other
    }

    public enum UssageLicense
    {
        Disallow,
        Allow,
    }

    [Serializable]
    [JsonSchema(Title = "vrm.meta")]
    public class glTF_VRM_Meta : JsonSerializableBase
    {
        static UssageLicense FromString(string src)
        {
            return EnumUtil.TryParseOrDefault<UssageLicense>(src);
        }
        public string title;
        public string version;
        public string author;
        public string contactInformation;
        public string reference;
        public int texture = -1;

        #region Ussage Permission
        public string allowedUserName;
        public AllowedUser allowedUser
        {
            get
            {
                return EnumUtil.TryParseOrDefault<AllowedUser>(allowedUserName);
            }
            set
            {
                allowedUserName = value.ToString();
            }
        }

        public string violentUssageName;
        public UssageLicense violentUssage
        {
            get { return FromString(violentUssageName); }
            set { violentUssageName = value.ToString(); }
        }

        public string sexualUssageName;
        public UssageLicense sexualUssage
        {
            get { return FromString(sexualUssageName); }
            set { sexualUssageName = value.ToString(); }
        }

        public string commercialUssageName;
        public UssageLicense commercialUssage
        {
            get { return FromString(commercialUssageName); }
            set { commercialUssageName = value.ToString(); }
        }

        public string otherPermissionUrl;
        #endregion

        #region Distribution License
        public string licenseName;
        public LicenseType licenseType
        {
            get
            {
                return EnumUtil.TryParseOrDefault<LicenseType>(licenseName);
            }
            set
            {
                licenseName = value.ToString();
            }
        }
        public string otherLicenseUrl;
        #endregion

        protected override void SerializeMembers(GLTFJsonFormatter f)
        {
            f.KeyValue(() => version);

            f.KeyValue(() => author);
            f.KeyValue(() => contactInformation);
            f.KeyValue(() => reference);

            f.KeyValue(() => title);
            f.KeyValue(() => texture);

            f.KeyValue(() => allowedUserName);
            f.KeyValue(() => violentUssageName);
            f.KeyValue(() => sexualUssageName);
            f.KeyValue(() => commercialUssageName);
            f.KeyValue(() => otherPermissionUrl);

            f.KeyValue(() => licenseName);
            f.KeyValue(() => otherLicenseUrl);
        }
    }
}
