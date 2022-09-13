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
    public delegate Task<(byte[] binary, string mimeType)?> GetTextureBytesAsync();

    /// <summary>
    /// 入力 glTF ファイルを Import した結果生成される、UnityEngine.Texture のアセット 1 つを確定させる Import 情報。
    /// </summary>
    public readonly struct TextureDescriptor
    {
        public readonly string UnityObjectName;

        public readonly Vector2 Offset;
        public readonly Vector2 Scale;
        public readonly SamplerParam Sampler;

        public readonly TextureImportTypes TextureType;

        // Parameters for Type:StandardMap
        public readonly float MetallicFactor;
        public readonly float RoughnessFactor;

        public readonly GetTextureBytesAsync Index0;
        public readonly GetTextureBytesAsync Index1;
        public readonly GetTextureBytesAsync Index2;
        public readonly GetTextureBytesAsync Index3;
        public readonly GetTextureBytesAsync Index4;
        public readonly GetTextureBytesAsync Index5;

        /// <summary>
        /// この Texture が Unity に実アセットとして存在する際の一意な Key
        /// </summary>
        public SubAssetKey SubAssetKey => new SubAssetKey(SubAssetKey.TextureType, UnityObjectName);

        public TextureDescriptor(string name, Vector2 offset, Vector2 scale, SamplerParam sampler, TextureImportTypes textureType, float metallicFactor, float roughnessFactor,
            GetTextureBytesAsync i0,
            GetTextureBytesAsync i1,
            GetTextureBytesAsync i2,
            GetTextureBytesAsync i3,
            GetTextureBytesAsync i4,
            GetTextureBytesAsync i5)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            UnityObjectName = name;
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
