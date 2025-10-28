using System;
using UnityEngine;

namespace UniGLTF
{
    public static class ColorConversionExtensions
    {
        public static float[] ToFloat4(this Color src, ColorSpace srcColorSpace, ColorSpace dstColorSpace)
        {
            var dst = src.ConvertColorSpace(srcColorSpace, dstColorSpace);
            return new float[] {dst.r, dst.g, dst.b, dst.a};
        }

        public static float[] ToFloat3(this Color src, ColorSpace srcColorSpace, ColorSpace dstColorSpace)
        {
            var dst = src.ConvertColorSpace(srcColorSpace, dstColorSpace);
            return new float[] {dst.r, dst.g, dst.b};
        }

        public static Color ToColor4(this float[] src, ColorSpace srcColorSpace, ColorSpace dstColorSpace)
        {
            if (src == null || src.Length < 4)
            {
                UniGLTFLogger.Warning("Invalid argument.");
                return Color.magenta;
            }

            return new Color(src[0], src[1], src[2], src[3]).ConvertColorSpace(srcColorSpace, dstColorSpace);
        }

        public static Color ToColor3(this float[] src, ColorSpace srcColorSpace, ColorSpace dstColorSpace)
        {
            if (src == null || src.Length < 3)
            {
                UniGLTFLogger.Warning("Invalid argument.");
                return Color.magenta;
            }

            return new Color(src[0], src[1], src[2], 1f).ConvertColorSpace(srcColorSpace, dstColorSpace);
        }

        public static Color ToColor3(this Vector3 src, ColorSpace srcColorSpace, ColorSpace dstColorSpace)
        {
            return new Color(src.x, src.y, src.z).ConvertColorSpace(srcColorSpace, dstColorSpace);
        }

        private static Color ConvertColorSpace(this Color srcColor, ColorSpace srcColorSpace, ColorSpace dstColorSpace)
        {
            // Need pattern matching :(
            if (srcColorSpace == ColorSpace.sRGB && dstColorSpace == ColorSpace.sRGB)
            {
                return srcColor;
            }
            else if (srcColorSpace == ColorSpace.sRGB && dstColorSpace == ColorSpace.Linear)
            {
                return srcColor.linear;
            }
            else if (srcColorSpace == ColorSpace.Linear && dstColorSpace == ColorSpace.sRGB)
            {
                return srcColor.gamma;
            }
            else if (srcColorSpace == ColorSpace.Linear && dstColorSpace == ColorSpace.Linear)
            {
                return srcColor;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
