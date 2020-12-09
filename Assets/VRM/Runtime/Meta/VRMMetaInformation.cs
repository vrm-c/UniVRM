using System;
using System.IO;
using UnityEngine;
using UniGLTF;


namespace VRM
{
    [Obsolete("reimport, use VRMMeta. Please reimport")]
    [Serializable]
    [DisallowMultipleComponent]
    public class VRMMetaInformation : MonoBehaviour, IEquatable<VRMMetaInformation>
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

        public bool Equals(VRMMetaInformation other)
        {
            return
            Author == other.Author
            && Title == other.Title
            && UniGLTF.MonoBehaviourComparator.AssetAreEquals(Thumbnail, other.Thumbnail)
            ;
        }

        private void Reset()
        {
            Title = name;
        }

#if UNITY_EDITOR
        [ContextMenu("CreateThumbnail")]
        void CreateThumbnailMenu()
        {
            var lookAt = GetComponent<VRMLookAt>();
            if (lookAt != null)
            {
                var texture = lookAt.CreateThumbnail();

#if false
                var assetPath = string.Format("Assets/{0}.thumbnail.asset", name);
                assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
                UnityEditor.AssetDatabase.CreateAsset(texture, assetPath);
#else
                var assetPath = string.Format("Assets/{0}.thumbnail.jpg", name);
                assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
                File.WriteAllBytes(assetPath.AssetPathToFullPath(), texture.EncodeToJPG());

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(texture);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                UnityEditor.AssetDatabase.ImportAsset(assetPath);
                Thumbnail = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
#endif
            }
        }
#endif

                public void CopyTo(GameObject _dst)
        {
            var dst = _dst.AddComponent<VRMMetaInformation>();
            dst.Title = Title;
            dst.Author = Author;
            dst.Thumbnail = Thumbnail;
        }

        public void OnValidate()
        {
            if (Thumbnail != null)
            {
                if (Thumbnail.width != 2048 || Thumbnail.height != 2048)
                {
                    Thumbnail = null;
                    Debug.LogError("Thumbnail must 2048 x 2048");
                }
            }
        }
    }
}
