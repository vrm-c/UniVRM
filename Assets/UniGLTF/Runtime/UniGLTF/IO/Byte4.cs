using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Byte4 : IEquatable<Byte4>
    {
        public readonly byte x;
        public readonly byte y;
        public readonly byte z;
        public readonly byte w;
        public Byte4(byte _x, byte _y, byte _z, byte _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public bool Equals(Byte4 other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }
    }
}
