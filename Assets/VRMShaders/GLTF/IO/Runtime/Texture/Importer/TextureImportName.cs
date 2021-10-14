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
            else if (type == TextureImportTypes.NormalMap)
            {
                return $"{gltfName}{NORMAL_SUFFIX}";

            }
            else if (type == TextureImportTypes.Linear)
            {
                return $"{gltfName}{LINEAR_SUFFIX}";
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

        private const string NORMAL_SUFFIX = ".normal";
        private const string STANDARD_SUFFIX = ".standard";
        private const string LINEAR_SUFFIX = ".linear";

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
            else if (src.EndsWith(LINEAR_SUFFIX))
            {
                return src.Substring(0, src.Length - LINEAR_SUFFIX.Length);
            }
            else
            {
                return src;
            }
        }
    }
}
