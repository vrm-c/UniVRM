using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct SByte3 : IEquatable<SByte3>
    {
        public readonly sbyte x;
        public readonly sbyte y;
        public readonly sbyte z;

        public SByte3(sbyte _x, sbyte _y, sbyte _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public bool Equals(SByte3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }
}
