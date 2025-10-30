using System;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public static class TextureConverter
    {
        public static readonly TextureFormat WithAlphaFormat = TextureFormat.ARGB32;
        public static readonly TextureFormat WithoutAlphaFormat = TextureFormat.RGB24;

        public static Texture2D CreateEmptyTextureWithSettings(Texture src, ColorSpace dstColorSpace, bool dstNeedsAlpha)
        {
            var texFormat = dstNeedsAlpha ? WithAlphaFormat : WithoutAlphaFormat;
            var dst = new Texture2D(src.width, src.height, texFormat, src.HasMipMap(), dstColorSpace == ColorSpace.Linear);
            dst.name = src.name;
            dst.anisoLevel = src.anisoLevel;
            dst.filterMode = src.filterMode;
            dst.mipMapBias = src.mipMapBias;
            dst.wrapMode = src.wrapMode;
            dst.wrapModeU = src.wrapModeU;
            dst.wrapModeV = src.wrapModeV;
            dst.wrapModeW = src.wrapModeW;

            return dst;
        }

        public static Texture2D CopyTexture(Texture src, ColorSpace dstColorSpace, bool dstNeedsAlpha, Material material)
        {
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

            var dst = CreateEmptyTextureWithSettings(src, dstColorSpace, dstNeedsAlpha);
            dst.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
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
