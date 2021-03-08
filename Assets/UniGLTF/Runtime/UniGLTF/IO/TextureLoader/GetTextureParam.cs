using System;

namespace UniGLTF
{
    public struct GetTextureParam
    {
        public const string NORMAL_PROP = "_BumpMap";
        public const string NORMAL_SUFFIX = ".normal";
        public const string METALLIC_GLOSS_PROP = "_MetallicGlossMap";
        public const string METALLIC_GLOSS_SUFFIX = ".metallicRoughness";
        public const string OCCLUSION_PROP = "_OcclusionMap";
        public const string OCCLUSION_SUFFIX = ".occlusion";

        public static string RemoveSuffix(string src)
        {
            if (src.EndsWith(NORMAL_SUFFIX))
            {
                return src.Substring(0, src.Length - NORMAL_SUFFIX.Length);
            }
            else if (src.EndsWith(METALLIC_GLOSS_SUFFIX))
            {
                return src.Substring(0, src.Length - METALLIC_GLOSS_SUFFIX.Length);
            }
            else if (src.EndsWith(OCCLUSION_SUFFIX))
            {
                return src.Substring(0, src.Length - OCCLUSION_SUFFIX.Length);
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
                    case METALLIC_GLOSS_PROP: return $"{m_name}{METALLIC_GLOSS_SUFFIX}";
                    case OCCLUSION_PROP: return $"{m_name}{OCCLUSION_SUFFIX}";
                    case NORMAL_PROP: return $"{m_name}{NORMAL_SUFFIX}";
                    default: return m_name;
                }
            }
        }

        public readonly string TextureType;
        public readonly float MetallicFactor;
        public readonly ushort? Index0;
        public readonly ushort? Index1;
        public readonly ushort? Index2;
        public readonly ushort? Index3;
        public readonly ushort? Index4;
        public readonly ushort? Index5;

        /// <summary>
        /// この２種類は変換済みをExtract
        /// </summary>
        public bool ExtractConverted => TextureType == OCCLUSION_PROP || TextureType == METALLIC_GLOSS_PROP;

        public GetTextureParam(string name, string textureType, float metallicFactor, int i0, int i1, int i2, int i3, int i4, int i5)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException();
            }
            m_name = name;

            TextureType = textureType;
            MetallicFactor = metallicFactor;
            Index0 = (ushort)i0;
            Index1 = (ushort)i1;
            Index2 = (ushort)i2;
            Index3 = (ushort)i3;
            Index4 = (ushort)i4;
            Index5 = (ushort)i5;
        }

        public static GetTextureParam Create(glTF gltf, int textureIndex)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, default, default, textureIndex, default, default, default, default, default);
        }

        public static GetTextureParam Create(glTF gltf, int index, string prop)
        {
            switch (prop)
            {
                case NORMAL_PROP:
                    return CreateNormal(gltf, index);

                case OCCLUSION_PROP:
                    return CreateOcclusion(gltf, index);

                case METALLIC_GLOSS_PROP:
                    return CreateMetallic(gltf, index, 1);

                default:
                    return Create(gltf, index);
            }
        }

        public static GetTextureParam CreateNormal(glTF gltf, int textureIndex)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, NORMAL_PROP, default, textureIndex, default, default, default, default, default);
        }

        public static GetTextureParam CreateMetallic(glTF gltf, int textureIndex, float metallicFactor)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, METALLIC_GLOSS_PROP, metallicFactor, textureIndex, default, default, default, default, default);
        }

        public static GetTextureParam CreateOcclusion(glTF gltf, int textureIndex)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, OCCLUSION_PROP, default, textureIndex, default, default, default, default, default);
        }
    }
}
