using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Short3 : IEquatable<Short3>
    {
        public readonly short x;
        public readonly short y;
        public readonly short z;

        public Short3(short _x, short _y, short _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public bool Equals(Short3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }
}
