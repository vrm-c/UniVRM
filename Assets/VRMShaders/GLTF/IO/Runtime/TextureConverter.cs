using System;
using System.Linq;
using UnityEngine;
using ColorSpace = UniGLTF.ColorSpace;


namespace VRMShaders
{
    public static class TextureConverter
    {
        public delegate Color32 ColorConversion(Color32 color);

        public static Texture2D Convert(Texture texture, ColorSpace dstColorSpace, ColorConversion colorConversion, Material convertMaterial)
        {
            var copyTexture = CopyTexture(texture, dstColorSpace, convertMaterial);
            if (colorConversion != null)
            {
                copyTexture.SetPixels32(copyTexture.GetPixels32().Select(x => colorConversion(x)).ToArray());
                copyTexture.Apply();
            }
            copyTexture.name = texture.name;
            return copyTexture;
        }

        public static Texture2D CopyTexture(Texture src, ColorSpace dstColorSpace, Material material)
        {
            Texture2D dst = null;
            RenderTextureReadWrite readWrite;
            switch (dstColorSpace)
            {
                case ColorSpace.sRGB:
                    readWrite = RenderTextureReadWrite.sRGB;
                    break;
                case ColorSpace.Linear:
                    readWrite = RenderTextureReadWrite.Linear;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dstColorSpace), dstColorSpace, null);
            }

            var renderTexture = new RenderTexture(src.width, src.height, 0, RenderTextureFormat.ARGB32, readWrite);

            if (material != null)
            {
                Graphics.Blit(src, renderTexture, material);
            }
            else
            {
                Graphics.Blit(src, renderTexture);
            }

            dst = new Texture2D(src.width, src.height, TextureFormat.ARGB32, src.HasMipMap(), readWrite == RenderTextureReadWrite.Linear);
            dst.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            dst.name = src.name;
            dst.anisoLevel = src.anisoLevel;
            dst.filterMode = src.filterMode;
            dst.mipMapBias = src.mipMapBias;
            dst.wrapMode = src.wrapMode;
            dst.wrapModeU = src.wrapModeU;
            dst.wrapModeV = src.wrapModeV;
            dst.wrapModeW = src.wrapModeW;
            dst.Apply();

            RenderTexture.active = null;
            if (Application.isEditor)
            {
                GameObject.DestroyImmediate(renderTexture);
            }
            else
            {
                GameObject.Destroy(renderTexture);
            }
            return dst;
        }
    }
}
