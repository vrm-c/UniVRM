using System;
using System.Threading.Tasks;
using UnityEngine;


namespace UniGLTF
{
    public delegate Task<ArraySegment<byte>> GetTextureBytesAsync();

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
            Linear,
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
        
        public struct NameExt
        {
            public readonly string GltfName;
            public readonly string ConvertedName;

            public readonly string Ext;
            public string Uri;
            
            public NameExt(string gltfName, string convertedName, string ext, string uri)
            {
                GltfName = gltfName;
                ConvertedName = convertedName;
                Ext = ext;
                Uri = uri;
            }
            
            public string GltfFileName => $"{GltfName}{Ext}";
            
            public string ConvertedFileName => $"{ConvertedName}.png";
            
            public static string Convert(string name, TextureTypes textureType) 
            {
                switch (textureType)
                {
                    case TextureTypes.StandardMap: return $"{name}{STANDARD_SUFFIX}";
                    case TextureTypes.NormalMap: return $"{name}{NORMAL_SUFFIX}";
                    default: return name;
                }
            }
        }
        
        public readonly NameExt Name;
        public string GltfName => Name.GltfName;
        public string GltfFileName => Name.GltfFileName; 
        public string ConvertedName => Name.ConvertedName;
        public string ConvertedFileName => Name.ConvertedFileName;
        public string Uri => Name.Uri;

        public enum TextureWrapType
        {
            All,
            U,
            V,
            W,
        }

        public struct TextureSamplerParam
        {
            public (TextureWrapType, TextureWrapMode)[] WrapModes;
            public FilterMode FilterMode;
        }

        public TextureSamplerParam Sampler;

        public readonly TextureTypes TextureType;
        public readonly float MetallicFactor;
        public readonly float RoughnessFactor;
        
        public readonly GetTextureBytesAsync Index0;
        public readonly GetTextureBytesAsync Index1;
        public readonly GetTextureBytesAsync Index2;
        public readonly GetTextureBytesAsync Index3;
        public readonly GetTextureBytesAsync Index4;
        public readonly GetTextureBytesAsync Index5;

        /// <summary>
        /// この種類は RGB チャンネルの組み換えが必用
        /// </summary>
        public bool ExtractConverted => TextureType == TextureTypes.StandardMap;

        public GetTextureParam(NameExt name, TextureSamplerParam sampler, TextureTypes textureType, float metallicFactor, float roughnessFactor,
            GetTextureBytesAsync i0,
            GetTextureBytesAsync i1,
            GetTextureBytesAsync i2,
            GetTextureBytesAsync i3,
            GetTextureBytesAsync i4,
            GetTextureBytesAsync i5)
        {
            Name = name;
            Sampler = sampler;
            TextureType = textureType;
            MetallicFactor = metallicFactor;
            RoughnessFactor = roughnessFactor;
            Index0 = i0;
            Index1 = i1;
            Index2 = i2;
            Index3 = i3;
            Index4 = i4;
            Index5 = i5;
        }
    }
}
