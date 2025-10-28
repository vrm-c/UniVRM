using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct UShort3 : IEquatable<UShort3>
    {
        public readonly ushort x;
        public readonly ushort y;
        public readonly ushort z;

        public UShort3(ushort _x, ushort _y, ushort _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public bool Equals(UShort3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }
}
