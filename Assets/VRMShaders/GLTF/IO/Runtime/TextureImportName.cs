using System;
using System.IO;

namespace VRMShaders
{
    public static class TextureImportName
    {
        public static string GetUnityObjectName(TextureImportTypes type, string gltfName, string uri)
        {
            if (type == TextureImportTypes.StandardMap)
            {
                // metallic, smooth, occlusion
                return $"{gltfName}{STANDARD_SUFFIX}";
            }
            else
            {
                if (!string.IsNullOrEmpty(uri) && !uri.StartsWith("data:", StringComparison.Ordinal))
                {
                    // external image
                    return Path.GetFileNameWithoutExtension(uri);
                }
                else
                {
                    // texture name
                    return gltfName;
                }
            }
        }

        public const string NORMAL_SUFFIX = ".normal";
        public const string STANDARD_SUFFIX = ".standard";

        public static string RemoveSuffix(string src)
        {
            if (src.EndsWith(NORMAL_SUFFIX))
            {
                return src.Substring(0, src.Length - NORMAL_SUFFIX.Length);
            }
            else if (src.EndsWith(STANDARD_SUFFIX))
            {
                return src.Substring(0, src.Length - STANDARD_SUFFIX.Length);
            }
            else
            {
                return src;
            }
        }
    }
}
