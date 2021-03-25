using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// get bytes for
    /// 
    /// runtime:
    ///   Texture2D.LoadImage
    /// extact:
    ///   File.WriteAllBytes
    /// </summary>
    /// <returns></returns>
    public delegate Task<byte[]> GetTextureBytesAsync();

    public struct TextureImportParam
    {
        public const string NORMAL_PROP = "_BumpMap";

        public const string METALLIC_GLOSS_PROP = "_MetallicGlossMap";
        public const string OCCLUSION_PROP = "_OcclusionMap";

       
        public readonly TextureImportName Name;
        public string GltfName => Name.GltfName;
        public string GltfFileName => Name.GltfFileName; 
        public string ConvertedName => Name.ConvertedName;
        public string ConvertedFileName => Name.ConvertedFileName;
        public string Uri => Name.Uri;

        public string ExtractKey => Name.ExtractKey;

        
        public Vector2 Offset;
        public Vector2 Scale;

        public SamplerParam Sampler;

        public readonly TextureImportTypes TextureType;
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
        public bool ExtractConverted => TextureType == TextureImportTypes.StandardMap;

        public TextureImportParam(TextureImportName name, Vector2 offset, Vector2 scale, SamplerParam sampler, TextureImportTypes textureType, float metallicFactor, float roughnessFactor,
            GetTextureBytesAsync i0,
            GetTextureBytesAsync i1,
            GetTextureBytesAsync i2,
            GetTextureBytesAsync i3,
            GetTextureBytesAsync i4,
            GetTextureBytesAsync i5)
        {
            Name = name;
            Offset = offset;
            Scale = scale;
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
