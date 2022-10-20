using System;
using UniJSON;

namespace UniVRM10.Migration
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

    public enum UsageLicense
    {
        Disallow,
        Allow,
    }

    /// <summary>
    /// VRM0.x version meta. This class has meta before migrate.
    /// </summary>
    public class Vrm0Meta
    {
        public string title;
        public string version;
        public string author;
        public string contactInformation;
        public string reference;
        public int texture = -1;
        public AllowedUser allowedUser = AllowedUser.OnlyAuthor;
        public bool violentUsage = false;
        public bool sexualUsage = false;
        public bool commercialUsage = false;
        public string otherPermissionUrl;
        public LicenseType licenseType = LicenseType.Redistribution_Prohibited;
        public string otherLicenseUrl;

        public static Vrm0Meta FromJsonBytes(UniJSON.JsonNode glTF)
        {
            var oldMeta = new Vrm0Meta();
            var extensions = glTF["extensions"];
            var vrm = extensions["VRM"];
            var meta = vrm["meta"];
            foreach (var kv in meta.ObjectItems())
            {
                var key = kv.Key.GetString();
                switch (key)
                {
                    case "title":
                        oldMeta.title = kv.Value.GetString();
                        break;
                    case "version":
                        oldMeta.version = kv.Value.GetString();
                        break;
                    case "author":
                        oldMeta.author = kv.Value.GetString();
                        break;
                    case "contactInformation":
                        oldMeta.contactInformation = kv.Value.GetString();
                        break;
                    case "reference":
                        oldMeta.reference = kv.Value.GetString();
                        break;
                    case "texture":
                        oldMeta.texture = kv.Value.GetInt32();
                        break;
                    case "allowedUserName":
                        oldMeta.allowedUser = (AllowedUser)Enum.Parse(typeof(AllowedUser), kv.Value.GetString(), true);
                        break;
                    case "violentUssageName":
                        oldMeta.violentUsage = kv.Value.GetString() == "Allow";
                        break;
                    case "sexualUssageName":
                        oldMeta.sexualUsage = kv.Value.GetString() == "Allow";
                        break;
                    case "commercialUssageName":
                        oldMeta.commercialUsage = kv.Value.GetString() == "Allow";
                        break;
                    case "otherPermissionUrl":
                        oldMeta.otherPermissionUrl = kv.Value.GetString();
                        break;
                    case "licenseName":
                        oldMeta.licenseType = (LicenseType)Enum.Parse(typeof(LicenseType), kv.Value.GetString(), true);
                        break;
                    case "otherLicenseUrl":
                        oldMeta.otherLicenseUrl = kv.Value.GetString();
                        break;
                    default:
                        UnityEngine.Debug.Log($"{key}");
                        break;
                }
            }
            return oldMeta;
        }
    }
}
