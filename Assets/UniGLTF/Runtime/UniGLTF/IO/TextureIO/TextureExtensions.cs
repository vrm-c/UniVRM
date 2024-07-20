using UnityEngine;

namespace VRMShaders
{
    internal static class TextureExtensions
    {
        public static bool HasMipMap(this Texture texture)
        {
            if (texture is Texture2D t2)
            {
                return t2.mipmapCount > 1;
            }
            else if (texture is RenderTexture rt)
            {
                return rt.useMipMap;
            }
            else
            {
                return false;
            }
        }

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
