using System;
using UnityEngine;

namespace UniVRM10
{
    public static class UnityTextureUtil
    {
        struct ColorSpaceScope : IDisposable
        {
            bool m_sRGBWrite;

            public ColorSpaceScope(RenderTextureReadWrite dstColorSpace)
            {
                m_sRGBWrite = GL.sRGBWrite;
                switch (dstColorSpace)
                {
                    case RenderTextureReadWrite.Linear:
                        GL.sRGBWrite = false;
                        break;

                    case RenderTextureReadWrite.sRGB:
                    default:
                        GL.sRGBWrite = true;
                        break;
                }
            }
            public ColorSpaceScope(bool sRGBWrite)
            {
                m_sRGBWrite = GL.sRGBWrite;
                GL.sRGBWrite = sRGBWrite;
            }

            public void Dispose()
            {
                GL.sRGBWrite = m_sRGBWrite;
            }
        }

        /// <summary>
        /// Copy texture for export.
        /// Use when source texture is not Texture2D or isReadable==false.
        /// </summary>
        public static Texture2D CopyTexture(Texture src, RenderTextureReadWrite dstColorSpace, Material material = null)
        {
            Texture2D dst = null;

            var renderTexture = new RenderTexture(src.width, src.height, 0, RenderTextureFormat.ARGB32, dstColorSpace);

            using (var scope = new ColorSpaceScope(dstColorSpace))
            {
                if (material != null)
                {
                    Graphics.Blit(src, renderTexture, material);
                }
                else
                {
                    Graphics.Blit(src, renderTexture);
                }
            }

            dst = new Texture2D(src.width, src.height, TextureFormat.ARGB32, false, dstColorSpace == RenderTextureReadWrite.Linear);
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