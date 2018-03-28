using System;
using UniGLTF;


namespace VRM
{
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

    public static class LicenseTypeExtensions
    {
        public static LicenseType ToLicenseType(this string src)
        {
            try
            {
                return (LicenseType)Enum.Parse(typeof(LicenseType), src, true);
            }
            catch (Exception)
            {
                return default(LicenseType);
            }
        }
    }

    [Serializable]
    public class glTF_VRM_Meta : JsonSerializableBase
    {
        public string author;
        public string contactInformation;
        public string licenseName;
        public LicenseType licenseType
        {
            get
            {
                return licenseName.ToLicenseType();
            }
            set
            {
                licenseName = value.ToString();
            }
        }
        public string otherLicenseUrl;
        public string reference;

        public string title;
        public int texture = -1;

        protected override void SerializeMembers(JsonFormatter f)
        {
            f.KeyValue(() => author);
            f.KeyValue(() => contactInformation);
            f.KeyValue(() => licenseName);
            f.KeyValue(() => otherLicenseUrl);
            f.KeyValue(() => reference);
            f.KeyValue(() => title);
            f.KeyValue(() => texture);
        }
    }
}
