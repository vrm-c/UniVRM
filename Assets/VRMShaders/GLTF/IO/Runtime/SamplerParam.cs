using UnityEngine;


namespace VRMShaders
{
    public struct SamplerParam
    {
        public TextureWrapMode WrapModesU;

        public TextureWrapMode WrapModesV;

        public FilterMode FilterMode;

        public bool EnableMipMap;

        public static SamplerParam Default => new SamplerParam
        {
            FilterMode = FilterMode.Bilinear,
            WrapModesU = TextureWrapMode.Repeat,
            WrapModesV = TextureWrapMode.Repeat,
            EnableMipMap = true,
        };
    }

    public static class SamplerParamExtensions
    {
        public static void SetSampler(this Texture2D texture, in SamplerParam param)
        {
            if (texture == null)
            {
                return;
            }
            texture.wrapModeU = param.WrapModesU;
            texture.wrapModeV = param.WrapModesV;
            texture.filterMode = param.FilterMode;
        }
    }
}
