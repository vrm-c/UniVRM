using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Byte3 : IEquatable<Byte3>
    {
        public readonly byte x;
        public readonly byte y;
        public readonly byte z;
        public Byte3(byte _x, byte _y, byte _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public bool Equals(Byte3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }
}
