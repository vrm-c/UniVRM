using System.IO;

namespace VRMShaders
{
    public struct TextureImportName
    {
        public readonly string GltfName;
        public readonly string ConvertedName;

        public readonly string Ext;
        public readonly string Uri;
        public readonly string ExtractKey;

        public static string GetExtractKey(TextureImportTypes type, string gltfName, string convertedName, string uri)
        {
            if (type == TextureImportTypes.StandardMap)
            {
                // metallic, smooth, occlusion
                return convertedName;
            }
            else
            {
                if (!string.IsNullOrEmpty(uri))
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

        public TextureImportName(TextureImportTypes textureType, string gltfName, string ext, string uri)
        {
            GltfName = gltfName;
            ConvertedName = TextureImportName.Convert(gltfName, textureType);
            Ext = ext;
            Uri = uri;
            ExtractKey = GetExtractKey(textureType, gltfName, ConvertedName, uri);
        }

        public string GltfFileName => $"{GltfName}{Ext}";

        public string ConvertedFileName => $"{ConvertedName}.png";

        public const string NORMAL_SUFFIX = ".normal";
        public const string STANDARD_SUFFIX = ".standard";
        public static string Convert(string name, TextureImportTypes textureType)
        {
            switch (textureType)
            {
                case TextureImportTypes.StandardMap: return $"{name}{STANDARD_SUFFIX}";
                case TextureImportTypes.NormalMap: return $"{name}{NORMAL_SUFFIX}";
                default: return name;
            }
        }
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
