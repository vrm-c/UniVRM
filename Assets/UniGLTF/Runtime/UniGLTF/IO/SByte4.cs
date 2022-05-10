using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct SByte4 : IEquatable<SByte4>
    {
        public readonly sbyte x;
        public readonly sbyte y;
        public readonly sbyte z;
        public readonly sbyte w;
        public SByte4(sbyte _x, sbyte _y, sbyte _z, sbyte _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public bool Equals(SByte4 other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }
    }
}
