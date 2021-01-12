using System;
using System.Collections.Generic;
using System.Numerics;
using VrmLib;


namespace UniVRM10
{
    public static class ArrayExtensions
    {
        public static Vector2 ToVector2(this float[] src, Vector2 defaultValue = default)
        {
            if (src.Length != 2) return defaultValue;

            var v = new Vector2();
            v.X = src[0];
            v.Y = src[1];
            return v;
        }

        public static Vector3 ToVector3(this float[] src, Vector3 defaultValue = default)
        {
            if (src.Length != 3) return defaultValue;

            var v = new Vector3();
            v.X = src[0];
            v.Y = src[1];
            v.Z = src[2];
            return v;
        }

        public static Quaternion ToQuaternion(this float[] src)
        {
            if (src.Length != 4) return Quaternion.Identity;

            var v = new Quaternion(src[0], src[1], src[2], src[3]);
            return v;
        }

        public static TextureInfo GetTexture(this int? nullable, List<Texture> textures)
        {
            if (!nullable.TryGetValidIndex(textures.Count, out int index))
            {
                return null;
            }
            var texture = textures[index];
            return new TextureInfo(texture);
        }

        public static TextureInfo GetTexture(this int index, List<Texture> textures)
        {
            if (index < 0 || index >= textures.Count)
            {
                return null;
            }
            var texture = textures[index];
            return new TextureInfo(texture);
        }

        public static int? ToIndex(this TextureInfo texture, List<Texture> textures)
        {
            if (texture == null)
            {
                return default;
            }
            return textures.IndexOfThrow(texture.Texture);
        }

        public static Vector4 ToVector4(this float[] src, Vector4 defaultValue = default)
        {
            switch (src.Length)
            {
                case 4:
                    return new Vector4(src[0], src[1], src[2], src[3]);
                case 3:
                    return new Vector4(src[0], src[1], src[2], 1.0f);
                case 0:
                    return defaultValue;
                default:
                    throw new Exception();
            }
        }

        public static LinearColor ToLinearColor(this float[] src, Vector4 defaultValue)
        {
            switch (src.Length)
            {
                case 4:
                    return LinearColor.FromLiner(src[0], src[1], src[2], src[3]);
                case 3:
                    return LinearColor.FromLiner(src[0], src[1], src[2], 1.0f);
                case 0:
                    return LinearColor.FromLiner(defaultValue);
                default:
                    throw new Exception();
            }
        }

        public static float[] ToFloat2(this System.Numerics.Vector2 value)
        {
            return new float[] { value.X, value.Y };
        }

        public static float[] ToFloat4(this System.Numerics.Vector4 value)
        {
            return new float[] { value.X, value.Y, value.Z, value.W };
        }
    }
}
