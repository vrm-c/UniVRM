using UnityEngine;


namespace UniGLTF
{
    public readonly struct SamplerParam
    {
        public TextureWrapMode WrapModesU { get; }

        public TextureWrapMode WrapModesV { get; }

        public FilterMode FilterMode { get; }

        public bool EnableMipMap { get; }

        public SamplerParam(TextureWrapMode wrapModesU, TextureWrapMode wrapModesV, FilterMode filterMode, bool enableMipMap)
        {
            WrapModesU = wrapModesU;
            WrapModesV = wrapModesV;
            FilterMode = filterMode;
            EnableMipMap = enableMipMap;
        }

        public static SamplerParam Default => new SamplerParam(
            TextureWrapMode.Repeat,
            TextureWrapMode.Repeat,
            FilterMode.Bilinear,
            true
        );
    }
}
