using System;
using System.Net;

namespace UniJSON
{
    public static class EndianConverter
    {
#if false
        /*
        #region Converter
        /// <summary>
        /// Read Uint16 From NetworkBytesOrder to HostBytesOrder
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static UInt16 N2H_UInt16(this ArraySegment<Byte> self)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (UInt16)(self.Get(0) << 8 | self.Get(1));
            }
            else
            {
                return BitConverter.ToUInt16(self.Array, self.Offset);
            }
        }

        public static UInt32 N2H_UInt32(this ArraySegment<Byte> self)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (UInt32)(self.Get(0) << 24 | self.Get(1) << 16 | self.Get(2) << 8 | self.Get(3));
            }
            else
            {
                return BitConverter.ToUInt32(self.Array, self.Offset);
            }
        }

        public static UInt64 N2H_UInt64(this ArraySegment<Byte> self)
        {
            var uvalue = BitConverter.ToUInt64(self.Array, self.Offset);
            if (BitConverter.IsLittleEndian)
            {
                ulong swapped =
                     ((0x00000000000000FF) & (uvalue >> 56)
                     | (0x000000000000FF00) & (uvalue >> 40)
                     | (0x0000000000FF0000) & (uvalue >> 24)
                     | (0x00000000FF000000) & (uvalue >> 8)
                     | (0x000000FF00000000) & (uvalue << 8)
                     | (0x0000FF0000000000) & (uvalue << 24)
                     | (0x00FF000000000000) & (uvalue << 40)
                     | (0xFF00000000000000) & (uvalue << 56));
                return swapped;
            }
            else
            {
                return uvalue;
            }
        }

        public static Int16 N2H_Int16(this ArraySegment<Byte> self)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (Int16)(self.Get(0) << 8 | self.Get(1));
            }
            else
            {
                return BitConverter.ToInt16(self.Array, self.Offset);
            }
        }

        public static Int32 N2H_Int32(this ArraySegment<Byte> self)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (Int32)(self.Get(0) << 24 | self.Get(1) << 16 | self.Get(2) << 8 | self.Get(3));
            }
            else
            {
                return BitConverter.ToInt32(self.Array, self.Offset);
            }
        }

        public static Int64 N2H_Int64(this ArraySegment<Byte> self)
        {
            var value = BitConverter.ToUInt64(self.Array, self.Offset);
            if (BitConverter.IsLittleEndian)
            {
                ulong swapped =
                     ((0x00000000000000FF) & (value >> 56)
                     | (0x000000000000FF00) & (value >> 40)
                     | (0x0000000000FF0000) & (value >> 24)
                     | (0x00000000FF000000) & (value >> 8)
                     | (0x000000FF00000000) & (value << 8)
                     | (0x0000FF0000000000) & (value << 24)
                     | (0x00FF000000000000) & (value << 40)
                     | (0xFF00000000000000) & (value << 56));
                return (long)swapped;
            }
            else
            {
                return (long)value;
            }
        }

        public static Single N2H_Single(this ArraySegment<Byte> self)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToSingle(self.TakeReversedArray(4), 0);
            }
            else
            {
                return BitConverter.ToSingle(self.Array, self.Offset);
            }
        }

        public static Double N2H_Double(this ArraySegment<Byte> self)
        {
            return BitConverter.Int64BitsToDouble(self.N2H_Int64());
        }

        public static void N2H_CopyTo(this ArraySegment<Byte> self, Byte[] buffer, int elementSize)
        {
            if (buffer.Length < self.Count) throw new ArgumentException();

            for (int i = 0; i < self.Count; i += elementSize)
            {
                for (int j = 0; j < elementSize; ++j)
                {
                    buffer[i + j] = self.Get(i + elementSize - 1 - j);
                }
            }
        }

        public static void N2H_CopyTo(this ArraySegment<Byte> self, Array result, Byte[] buffer)
        {
            if (BitConverter.IsLittleEndian)
            {
                if (buffer.Length < self.Count)
                {
                    throw new ArgumentException();
                }
                self.N2H_CopyTo(buffer, Marshal.SizeOf(result.GetType().GetElementType()));

                Buffer.BlockCopy(buffer, 0, result, 0, self.Count);
            }
            else
            {
                Buffer.BlockCopy(self.Array, self.Offset, result, 0, self.Count);
            }
        }
        #endregion
        */

#else
        #region Signed
        public static Int16 NetworkByteWordToSignedNativeByteOrder(ArraySegment<Byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                // Network to Little
                var value = new ByteUnion.WordValue
                {
                    Byte0 = bytes.Get(1),
                    Byte1 = bytes.Get(0),
                };
                return value.Signed;
            }
            else
            {
                // Network to Big
                var value = new ByteUnion.WordValue
                {
                    Byte0 = bytes.Get(0),
                    Byte1 = bytes.Get(1),
                };
                return value.Signed;
            }
        }
        public static Int32 NetworkByteDWordToSignedNativeByteOrder(ArraySegment<Byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                // Network to Little
                var value = new ByteUnion.DWordValue
                {
                    Byte0 = bytes.Get(3),
                    Byte1 = bytes.Get(2),
                    Byte2 = bytes.Get(1),
                    Byte3 = bytes.Get(0),
                };
                return value.Signed;
            }
            else
            {
                // Network to Big
                var value = new ByteUnion.DWordValue
                {
                    Byte0 = bytes.Get(0),
                    Byte1 = bytes.Get(1),
                    Byte2 = bytes.Get(2),
                    Byte3 = bytes.Get(3),
                };
                return value.Signed;
            }
        }
        public static Int64 NetworkByteQWordToSignedNativeByteOrder(ArraySegment<Byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                // Network to Little
                var value = new ByteUnion.QWordValue
                {
                    Byte0 = bytes.Get(7),
                    Byte1 = bytes.Get(6),
                    Byte2 = bytes.Get(5),
                    Byte3 = bytes.Get(4),
                    Byte4 = bytes.Get(3),
                    Byte5 = bytes.Get(2),
                    Byte6 = bytes.Get(1),
                    Byte7 = bytes.Get(0),
                };
                return value.Signed;
            }
            else
            {
                // Network to Big
                var value = new ByteUnion.QWordValue
                {
                    Byte0 = bytes.Get(0),
                    Byte1 = bytes.Get(1),
                    Byte2 = bytes.Get(2),
                    Byte3 = bytes.Get(3),
                    Byte4 = bytes.Get(4),
                    Byte5 = bytes.Get(5),
                    Byte6 = bytes.Get(6),
                    Byte7 = bytes.Get(7),
                };
                return value.Signed;
            }
        }
        #endregion
        #region Unsigned
        public static UInt16 NetworkByteWordToUnsignedNativeByteOrder(ArraySegment<Byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                // Network to Little
                var value = new ByteUnion.WordValue
                {
                    Byte0 = bytes.Get(1),
                    Byte1 = bytes.Get(0),
                };
                return value.Unsigned;
            }
            else
            {
                // Network to Big
                var value = new ByteUnion.WordValue
                {
                    Byte0 = bytes.Get(0),
                    Byte1 = bytes.Get(1),
                };
                return value.Unsigned;
            }
        }
        public static UInt32 NetworkByteDWordToUnsignedNativeByteOrder(ArraySegment<Byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                // Network to Little
                var value = new ByteUnion.DWordValue
                {
                    Byte0 = bytes.Get(3),
                    Byte1 = bytes.Get(2),
                    Byte2 = bytes.Get(1),
                    Byte3 = bytes.Get(0),
                };
                return value.Unsigned;
            }
            else
            {
                // Network to Big
                var value = new ByteUnion.DWordValue
                {
                    Byte0 = bytes.Get(0),
                    Byte1 = bytes.Get(1),
                    Byte2 = bytes.Get(2),
                    Byte3 = bytes.Get(3),
                };
                return value.Unsigned;
            }
        }
        public static UInt64 NetworkByteQWordToUnsignedNativeByteOrder(ArraySegment<Byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                // Network to Little
                var value = new ByteUnion.QWordValue
                {
                    Byte0 = bytes.Get(7),
                    Byte1 = bytes.Get(6),
                    Byte2 = bytes.Get(5),
                    Byte3 = bytes.Get(4),
                    Byte4 = bytes.Get(3),
                    Byte5 = bytes.Get(2),
                    Byte6 = bytes.Get(1),
                    Byte7 = bytes.Get(0),
                };
                return value.Unsigned;
            }
            else
            {
                // Network to Big
                var value = new ByteUnion.QWordValue
                {
                    Byte0 = bytes.Get(0),
                    Byte1 = bytes.Get(1),
                    Byte2 = bytes.Get(2),
                    Byte3 = bytes.Get(3),
                    Byte4 = bytes.Get(4),
                    Byte5 = bytes.Get(5),
                    Byte6 = bytes.Get(6),
                    Byte7 = bytes.Get(7),
                };
                return value.Unsigned;
            }
        }
        #endregion
        #region Floating
        public static Single NetworkByteDWordToFloatNativeByteOrder(ArraySegment<Byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                // Network to Little
                var value = new ByteUnion.DWordValue
                {
                    Byte0 = bytes.Get(3),
                    Byte1 = bytes.Get(2),
                    Byte2 = bytes.Get(1),
                    Byte3 = bytes.Get(0),
                };
                return value.Float;
            }
            else
            {
                // Network to Big
                var value = new ByteUnion.DWordValue
                {
                    Byte0 = bytes.Get(0),
                    Byte1 = bytes.Get(1),
                    Byte2 = bytes.Get(2),
                    Byte3 = bytes.Get(3),
                };
                return value.Float;
            }
        }
        public static Double NetworkByteQWordToFloatNativeByteOrder(ArraySegment<Byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                // Network to Little
                var value = new ByteUnion.QWordValue
                {
                    Byte0 = bytes.Get(7),
                    Byte1 = bytes.Get(6),
                    Byte2 = bytes.Get(5),
                    Byte3 = bytes.Get(4),
                    Byte4 = bytes.Get(3),
                    Byte5 = bytes.Get(2),
                    Byte6 = bytes.Get(1),
                    Byte7 = bytes.Get(0),
                };
                return value.Float;
            }
            else
            {
                // Network to Big
                var value = new ByteUnion.QWordValue
                {
                    Byte0 = bytes.Get(0),
                    Byte1 = bytes.Get(1),
                    Byte2 = bytes.Get(2),
                    Byte3 = bytes.Get(3),
                    Byte4 = bytes.Get(4),
                    Byte5 = bytes.Get(5),
                    Byte6 = bytes.Get(6),
                    Byte7 = bytes.Get(7),
                };
                return value.Float;
            }
        }
        #endregion
#endif

        public static Int16 ToNetworkByteOrder(this Int16 value)
        {
            return ByteUnion.WordValue.Create(value).HostToNetworkOrder().Signed;
        }
        public static UInt16 ToNetworkByteOrder(this UInt16 value)
        {
            return ByteUnion.WordValue.Create(value).HostToNetworkOrder().Unsigned;
        }

        public static Int32 ToNetworkByteOrder(this Int32 value)
        {
            return ByteUnion.DWordValue.Create(value).HostToNetworkOrder().Signed;
        }
        public static UInt32 ToNetworkByteOrder(this UInt32 value)
        {
            return ByteUnion.DWordValue.Create(value).HostToNetworkOrder().Unsigned;
        }
        public static Single ToNetworkByteOrder(this Single value)
        {
            return ByteUnion.DWordValue.Create(value).HostToNetworkOrder().Float;
        }

        public static Int64 ToNetworkByteOrder(this Int64 value)
        {
            return ByteUnion.QWordValue.Create(value).HostToNetworkOrder().Signed;
        }
        public static UInt64 ToNetworkByteOrder(this UInt64 value)
        {
            return ByteUnion.QWordValue.Create(value).HostToNetworkOrder().Unsigned;
        }
        public static Double ToNetworkByteOrder(this Double value)
        {
            return ByteUnion.QWordValue.Create(value).HostToNetworkOrder().Float;
        }
    }
}
