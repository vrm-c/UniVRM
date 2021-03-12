using System;

namespace UniGLTF
{
    /// <summary>
    /// STANDARD(Pbr) texture = occlusion + metallic + smoothness
    /// </summary>
    public struct GetTextureParam
    {
        public const string NORMAL_PROP = "_BumpMap";
        public const string NORMAL_SUFFIX = ".normal";

        public const string METALLIC_GLOSS_PROP = "_MetallicGlossMap";
        public const string OCCLUSION_PROP = "_OcclusionMap";
        public const string STANDARD_SUFFIX = ".standard";

        public enum TextureTypes
        {
            sRGB,
            NormalMap,
            // Occlusion + Metallic + Smoothness
            StandardMap,
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

        readonly string m_name;

        public string GltflName => m_name;

        public string ConvertedName
        {
            get
            {
                switch (TextureType)
                {
                    case TextureTypes.StandardMap: return $"{m_name}{STANDARD_SUFFIX}";
                    case TextureTypes.NormalMap: return $"{m_name}{NORMAL_SUFFIX}";
                    default: return m_name;
                }
            }
        }

        public readonly TextureTypes TextureType;
        public readonly float MetallicFactor;
        public readonly float RoughnessFactor;
        public readonly ushort? Index0;
        public readonly ushort? Index1;
        public readonly ushort? Index2;
        public readonly ushort? Index3;
        public readonly ushort? Index4;
        public readonly ushort? Index5;

        /// <summary>
        /// この種類は RGB チャンネルの組み換えが必用
        /// </summary>
        public bool ExtractConverted => TextureType == TextureTypes.StandardMap;

        public GetTextureParam(string name, TextureTypes textureType, float metallicFactor, float roughnessFactor, int i0, int i1, int i2, int i3, int i4, int i5)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException();
            }
            m_name = name;

            TextureType = textureType;
            MetallicFactor = metallicFactor;
            RoughnessFactor = roughnessFactor;
            Index0 = (ushort)i0;
            Index1 = (ushort)i1;
            Index2 = (ushort)i2;
            Index3 = (ushort)i3;
            Index4 = (ushort)i4;
            Index5 = (ushort)i5;
        }

        public static GetTextureParam CreateSRGB(glTF gltf, int textureIndex)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, TextureTypes.sRGB, default, default, textureIndex, default, default, default, default, default);
        }

        public static GetTextureParam Create(glTF gltf, int index, string prop, float metallicFactor, float roughnessFactor)
        {
            switch (prop)
            {
                case NORMAL_PROP:
                    return CreateNormal(gltf, index);

                case OCCLUSION_PROP:
                case METALLIC_GLOSS_PROP:
                    return CreateStandard(gltf, index, metallicFactor, roughnessFactor);

                default:
                    return CreateSRGB(gltf, index);
            }
        }

        public static GetTextureParam CreateNormal(glTF gltf, int textureIndex)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, TextureTypes.NormalMap, default, default, textureIndex, default, default, default, default, default);
        }

        public static GetTextureParam CreateStandard(glTF gltf, int textureIndex, float metallicFactor, float roughnessFactor)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, TextureTypes.StandardMap, metallicFactor, roughnessFactor, textureIndex, default, default, default, default, default);
        }
    }
}
