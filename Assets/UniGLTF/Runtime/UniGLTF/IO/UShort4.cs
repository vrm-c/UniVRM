using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UShort4
    {
        public readonly ushort x;
        public readonly ushort y;
        public readonly ushort z;
        public readonly ushort w;

        public UShort4(ushort _x, ushort _y, ushort _z, ushort _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }
    }
}
