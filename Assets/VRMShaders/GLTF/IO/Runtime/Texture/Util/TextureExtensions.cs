using UnityEngine;

namespace VRMShaders
{
    public static class TextureExtensions
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
    }
}
