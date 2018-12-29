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

        [JsonSchema(Description = "Title of VRM model")]
        public string title;

        [JsonSchema(Description = "Version of VRM model")]
        public string version;

        [JsonSchema(Description = "Author of VRM model")]
        public string author;

        [JsonSchema(Description = "Contact Information of VRM model author")]
        public string contactInformation;

        [JsonSchema(Description = "Reference of VRM model")]
        public string reference;

        [JsonSchema(Description = "Thumbnail of VRM model")]
        public int texture = -1;

        #region Ussage Permission
        [JsonSchema(Description = "A person who can perform with this avatar ", EnumValues = new object[] {
            "OnlyAuthor",
            "ExplicitlyLicensedPerson",
            "Everyone",
        }, EnumSerializationType = EnumSerializationType.AsString)]
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

        [JsonSchema(Description = "Permission to perform violent acts with this avatar", EnumValues = new object[]
        {
        "Disallow",
        "Allow",
        }, EnumSerializationType = EnumSerializationType.AsString)]
        public string violentUssageName;
        public UssageLicense violentUssage
        {
            get { return FromString(violentUssageName); }
            set { violentUssageName = value.ToString(); }
        }

        [JsonSchema(Description = "Permission to perform sexual acts with this avatar", EnumValues = new object[]
        {
        "Disallow",
        "Allow",
        }, EnumSerializationType = EnumSerializationType.AsString)]
        public string sexualUssageName;
        public UssageLicense sexualUssage
        {
            get { return FromString(sexualUssageName); }
            set { sexualUssageName = value.ToString(); }
        }

        [JsonSchema(Description = "For commercial use", EnumValues = new object[]
        {
        "Disallow",
        "Allow",
        }, EnumSerializationType = EnumSerializationType.AsString)]
        public string commercialUssageName;
        public UssageLicense commercialUssage
        {
            get { return FromString(commercialUssageName); }
            set { commercialUssageName = value.ToString(); }
        }

        [JsonSchema(Description = "If there are any conditions not mentioned above, put the URL link of the license document here.")]
        public string otherPermissionUrl;
        #endregion

        #region Distribution License
        [JsonSchema(Description = "License type", EnumValues = new object[]
        {
            "Redistribution_Prohibited",
            "CC0",
            "CC_BY",
            "CC_BY_NC",
            "CC_BY_SA",
            "CC_BY_NC_SA",
            "CC_BY_ND",
            "CC_BY_NC_ND",
            "Other"
        }, EnumSerializationType = EnumSerializationType.AsString)]
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

        [JsonSchema(Description = "If “Other” is selected, put the URL link of the license document here.")]
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
