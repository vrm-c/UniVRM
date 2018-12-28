using System;
using System.IO;

namespace UniJSON
{
    public static class ByteExtensions
    {
        public static Byte GetHexDigit(this UInt16 n, int index)
        {
            return (Byte)(n >> 8 * index & 0xff);
        }
        public static Byte GetHexDigit(this UInt32 n, int index)
        {
            return (Byte)(n >> 8 * index & 0xff);
        }
        public static Byte GetHexDigit(this UInt64 n, int index)
        {
            return (Byte)(n >> 8 * index & 0xff);
        }
        public static Byte GetHexDigit(this Int16 n, int index)
        {
            return (Byte)(n >> 8 * index & 0xff);
        }
        public static Byte GetHexDigit(this Int32 n, int index)
        {
            return (Byte)(n >> 8 * index & 0xff);
        }
        public static Byte GetHexDigit(this Int64 n, int index)
        {
            return (Byte)(n >> 8 * index & 0xff);
        }

        public static UInt32 ToUint32(this Single n, Byte[] buffer)
        {
            if (buffer.Length < 4)
            {
                throw new ArgumentException();
            }
            using (var ms = new MemoryStream(buffer))
            using (var w = new BinaryWriter(ms))
            {
                w.Write(n);
            }
            return BitConverter.ToUInt32(buffer, 0);
        }
        public static UInt64 ToUint64(this Double n, Byte[] buffer)
        {
            if (buffer.Length < 8)
            {
                throw new ArgumentException();
            }
            using (var ms = new MemoryStream(buffer))
            using (var w = new BinaryWriter(ms))
            {
                w.Write(n);
            }
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}
