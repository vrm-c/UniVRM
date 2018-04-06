using UnityEngine;


namespace VRM
{
    public class VRMMetaObject : ScriptableObject
    {
        #region Info
        [SerializeField, Header("Information")]
        public string Version;

        [SerializeField]
        public string Author;

        [SerializeField]
        public string ContactInformation;

        [SerializeField]
        public string Reference;

        [SerializeField]
        public string Title;

        [SerializeField]
        public Texture2D Thumbnail;
        #endregion

        #region Permission
        [SerializeField, Header("Permission")]
        public AllowedUser allowedUser;

        [SerializeField]
        public bool allowImmoralUssage;
        [SerializeField]
        public bool allowCcertainBeliefsUssage;
        [SerializeField]
        public bool allowPoliticalUssage;
        [SerializeField]
        public bool allowCommercialUssage;
        #endregion

        #region License
        [SerializeField, Header("Distribution License")]
        public LicenseType LicenseType;

        [SerializeField]
        public string OtherLicenseUrl;
        #endregion

        public bool Equals(VRMMetaObject other)
        {
            return
            Author == other.Author
            && Title == other.Title
            && UniGLTF.MonoBehaviourComparator.AssetAreEquals(Thumbnail, other.Thumbnail)
            ;
        }
    }
}
