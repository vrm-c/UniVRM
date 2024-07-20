using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// ITextureDeserializer 実装者にテクスチャロード処理を委譲するために必要な情報.
    /// </summary>
    public sealed class DeserializingTextureInfo
    {
        /// <summary>
        /// Texture のバイト列
        /// </summary>
        public byte[] ImageData { get; }

        /// <summary>
        /// Texture の mimeType
        /// </summary>
        public string DataMimeType { get; }

        /// <summary>
        /// Texture に求められる色空間
        /// </summary>
        public ColorSpace ColorSpace { get; }

        /// <summary>
        /// Texture に Mipmap が求められるか否か
        /// </summary>
        public bool UseMipmap { get; }

        /// <summary>
        /// Texture に求められる FilterMode
        /// </summary>
        public FilterMode FilterMode { get; }

        /// <summary>
        /// Texture に求められる U-Axis の WrapMode
        /// </summary>
        public TextureWrapMode WrapModeU { get; }

        /// <summary>
        /// Texture に求められる V-Axis の WrapMode
        /// </summary>
        public TextureWrapMode WrapModeV { get; }

        public TextureImportTypes ImportTypes { get; }

        public DeserializingTextureInfo(byte[] imageData, string dataMimeType, ColorSpace colorSpace, bool useMipmap, FilterMode filterMode, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV)
        {
            ImageData = imageData;
            DataMimeType = dataMimeType;
            ColorSpace = colorSpace;
            UseMipmap = useMipmap;
            FilterMode = filterMode;
            WrapModeU = wrapModeU;
            WrapModeV = wrapModeV;
        }

        public DeserializingTextureInfo(byte[] imageData, string dataMimeType, ColorSpace colorSpace, SamplerParam samplerParam, TextureImportTypes importTypes)
        {
            ImageData = imageData;
            DataMimeType = dataMimeType;
            ColorSpace = colorSpace;
            UseMipmap = samplerParam.EnableMipMap;
            FilterMode = samplerParam.FilterMode;
            WrapModeU = samplerParam.WrapModesU;
            WrapModeV = samplerParam.WrapModesV;
            ImportTypes = importTypes;
        }
    }
}