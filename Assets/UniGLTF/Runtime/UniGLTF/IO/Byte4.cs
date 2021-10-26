using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Byte4
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
    }
}
