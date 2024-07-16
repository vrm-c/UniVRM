using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 
    /// </summary>
    public static class PackageResource
    {
        /// <summary>
        /// Local時のAssetPath
        /// </summary>
        public const string LocalBase = "Assets/VRM10";

        /// <summary>
        /// UPM参照時のAssetPath
        /// </summary>
        public const string PackageBase = "Packages/com.vrmc.univrm";

        /// <summary>
        /// Try local then try package.
        /// </summary>
        /// <param name="relpath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ResourceLocalOrUPM<T>(string relpath) where T : UnityEngine.Object
        {
            var path = $"{LocalBase}/{relpath}";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset is null)
            {
                path = $"{PackageResource.PackageBase}/{relpath}";
                asset = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return asset;
        }
    }
}
