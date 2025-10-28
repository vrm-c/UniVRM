using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct UShort2 : IEquatable<UShort2>
    {
        public readonly ushort x;
        public readonly ushort y;

        public UShort2(ushort _x, ushort _y)
        {
            x = _x;
            y = _y;
        }

        public bool Equals(UShort2 other)
        {
            return x == other.x && y == other.y;
        }
    }
}
