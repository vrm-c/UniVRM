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
            var path = $"{PackageResource.PackageBase}/{relpath}";
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}