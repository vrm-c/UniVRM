using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRM
{
    public class VRMMetaObject : ScriptableObject
    {
        #region Info
        [SerializeField, Header("Information")]
        public string Title;

        [SerializeField]
        public string Author;

        [SerializeField]
        public string ContactInformation;

        [SerializeField]
        public Texture2D Thumbnail;

        [SerializeField]
        public string Reference;
        #endregion

        #region License
        [SerializeField, Header("License")]
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
