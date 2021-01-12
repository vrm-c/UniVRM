using System;
using System.Numerics;

namespace VrmLib
{
    public struct LinearColor : IEquatable<LinearColor>
    {
        public Vector4 RGBA;

        public float[] ToFloat4()
        {
            return new float[]{
                RGBA.X,
                RGBA.Y,
                RGBA.Z,
                RGBA.W,
            };
        }

        public float[] ToFloat3()
        {
            return new float[]{
                RGBA.X,
                RGBA.Y,
                RGBA.Z,
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is LinearColor color)
            {
                return Equals(color);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return RGBA.GetHashCode();
        }

        public static bool operator ==(LinearColor lhs, LinearColor rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(LinearColor lhs, LinearColor rhs)
        {
            return !(lhs == rhs);
        }

        public static LinearColor FromLiner(Vector4 color)
        {
            return new LinearColor
            {
                RGBA = color
            };
        }

        public static LinearColor FromLiner(float r, float g, float b, float a)
        {
            return new LinearColor
            {
                RGBA = new Vector4(r, g, b, a)
            };
        }

        public static LinearColor FromLiner(float[] color)
        {
            return new LinearColor
            {
                RGBA = new Vector4(color[0], color[1], color[2], color[3])
            };
        }

        public static LinearColor White => new LinearColor
        {
            RGBA = Vector4.One,
        };

        public static LinearColor Black => new LinearColor
        {
            RGBA = new Vector4(0, 0, 0, 1),
        };

        public bool Equals(LinearColor other)
        {
            return RGBA == other.RGBA;
        }
    }
}
