using System;
using System.Collections.Generic;

namespace UniJSON
{
    public static class MsgPackParser
    {
        public static ListTreeNode<MsgPackValue> Parse(Byte[] bytes)
        {
            return Parse(new ArraySegment<byte>(bytes));
        }

        static MsgPackType GetFormat(ArraySegment<Byte> bytes)
        {
            return (MsgPackType)bytes.Get(0);
        }

        /// <summary>
        /// Array又はMapの子要素の数を得る
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        static ArraySegment<Byte> GetItemCount(ArraySegment<Byte> bytes, MsgPackType formatType, out UInt32 count)
        {
            switch (formatType)
            {
                case MsgPackType.FIX_ARRAY: count = 0; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x1: count = 1; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x2: count = 2; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x3: count = 3; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x4: count = 4; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x5: count = 5; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x6: count = 6; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x7: count = 7; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x8: count = 8; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0x9: count = 9; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0xA: count = 10; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0xB: count = 11; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0xC: count = 12; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0xD: count = 13; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0xE: count = 14; return bytes.Advance(1);
                case MsgPackType.FIX_ARRAY_0xF: count = 15; return bytes.Advance(1);

                case MsgPackType.FIX_MAP: count = 0; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x1: count = 1; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x2: count = 2; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x3: count = 3; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x4: count = 4; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x5: count = 5; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x6: count = 6; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x7: count = 7; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x8: count = 8; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0x9: count = 9; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0xA: count = 10; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0xB: count = 11; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0xC: count = 12; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0xD: count = 13; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0xE: count = 14; return bytes.Advance(1);
                case MsgPackType.FIX_MAP_0xF: count = 15; return bytes.Advance(1);

                case MsgPackType.ARRAY16:
                case MsgPackType.MAP16:
                    count = EndianConverter.NetworkByteWordToUnsignedNativeByteOrder(bytes.Advance(1));
                    return bytes.Advance(1 + 2);

                case MsgPackType.ARRAY32:
                case MsgPackType.MAP32:
                    count = EndianConverter.NetworkByteDWordToUnsignedNativeByteOrder(bytes.Advance(1));
                    return bytes.Advance(1 + 4);

                default:
                    throw new ArgumentException("is not collection: " + formatType);
            }
        }

        /// <summary>
        /// ArrayとMap以外のタイプのペイロードを得る
        /// </summary>
        /// <returns></returns>
        static ArraySegment<Byte> GetBody(ArraySegment<Byte> bytes, MsgPackType formatType)
        {
            switch (formatType)
            {
                case MsgPackType.FIX_STR: return bytes.Advance(1).Take(0);
                case MsgPackType.FIX_STR_0x01: return bytes.Advance(1).Take(1);
                case MsgPackType.FIX_STR_0x02: return bytes.Advance(1).Take(2);
                case MsgPackType.FIX_STR_0x03: return bytes.Advance(1).Take(3);
                case MsgPackType.FIX_STR_0x04: return bytes.Advance(1).Take(4);
                case MsgPackType.FIX_STR_0x05: return bytes.Advance(1).Take(5);
                case MsgPackType.FIX_STR_0x06: return bytes.Advance(1).Take(6);
                case MsgPackType.FIX_STR_0x07: return bytes.Advance(1).Take(7);
                case MsgPackType.FIX_STR_0x08: return bytes.Advance(1).Take(8);
                case MsgPackType.FIX_STR_0x09: return bytes.Advance(1).Take(9);
                case MsgPackType.FIX_STR_0x0A: return bytes.Advance(1).Take(10);
                case MsgPackType.FIX_STR_0x0B: return bytes.Advance(1).Take(11);
                case MsgPackType.FIX_STR_0x0C: return bytes.Advance(1).Take(12);
                case MsgPackType.FIX_STR_0x0D: return bytes.Advance(1).Take(13);
                case MsgPackType.FIX_STR_0x0E: return bytes.Advance(1).Take(14);
                case MsgPackType.FIX_STR_0x0F: return bytes.Advance(1).Take(15);

                case MsgPackType.FIX_STR_0x10: return bytes.Advance(1).Take(16);
                case MsgPackType.FIX_STR_0x11: return bytes.Advance(1).Take(17);
                case MsgPackType.FIX_STR_0x12: return bytes.Advance(1).Take(18);
                case MsgPackType.FIX_STR_0x13: return bytes.Advance(1).Take(19);
                case MsgPackType.FIX_STR_0x14: return bytes.Advance(1).Take(20);
                case MsgPackType.FIX_STR_0x15: return bytes.Advance(1).Take(21);
                case MsgPackType.FIX_STR_0x16: return bytes.Advance(1).Take(22);
                case MsgPackType.FIX_STR_0x17: return bytes.Advance(1).Take(23);
                case MsgPackType.FIX_STR_0x18: return bytes.Advance(1).Take(24);
                case MsgPackType.FIX_STR_0x19: return bytes.Advance(1).Take(25);
                case MsgPackType.FIX_STR_0x1A: return bytes.Advance(1).Take(26);
                case MsgPackType.FIX_STR_0x1B: return bytes.Advance(1).Take(27);
                case MsgPackType.FIX_STR_0x1C: return bytes.Advance(1).Take(28);
                case MsgPackType.FIX_STR_0x1D: return bytes.Advance(1).Take(29);
                case MsgPackType.FIX_STR_0x1E: return bytes.Advance(1).Take(30);
                case MsgPackType.FIX_STR_0x1F: return bytes.Advance(1).Take(31);

                case MsgPackType.STR8:
                case MsgPackType.BIN8:
                    {
                        var count = bytes.Get(1);
                        return bytes.Advance(1 + 1).Take(count);
                    }

                case MsgPackType.STR16:
                case MsgPackType.BIN16:
                    {
                        var count = EndianConverter.NetworkByteWordToUnsignedNativeByteOrder(bytes.Advance(1));
                        return bytes.Advance(1 + 2).Take(count);
                    }

                case MsgPackType.STR32:
                case MsgPackType.BIN32:
                    {
                        var count = EndianConverter.NetworkByteDWordToUnsignedNativeByteOrder(bytes.Advance(1));
                        return bytes.Advance(1 + 4).Take((int)count);
                    }

                case MsgPackType.NIL:
                case MsgPackType.TRUE:
                case MsgPackType.FALSE:
                case MsgPackType.POSITIVE_FIXNUM:
                case MsgPackType.POSITIVE_FIXNUM_0x01:
                case MsgPackType.POSITIVE_FIXNUM_0x02:
                case MsgPackType.POSITIVE_FIXNUM_0x03:
                case MsgPackType.POSITIVE_FIXNUM_0x04:
                case MsgPackType.POSITIVE_FIXNUM_0x05:
                case MsgPackType.POSITIVE_FIXNUM_0x06:
                case MsgPackType.POSITIVE_FIXNUM_0x07:
                case MsgPackType.POSITIVE_FIXNUM_0x08:
                case MsgPackType.POSITIVE_FIXNUM_0x09:
                case MsgPackType.POSITIVE_FIXNUM_0x0A:
                case MsgPackType.POSITIVE_FIXNUM_0x0B:
                case MsgPackType.POSITIVE_FIXNUM_0x0C:
                case MsgPackType.POSITIVE_FIXNUM_0x0D:
                case MsgPackType.POSITIVE_FIXNUM_0x0E:
                case MsgPackType.POSITIVE_FIXNUM_0x0F:

                case MsgPackType.POSITIVE_FIXNUM_0x10:
                case MsgPackType.POSITIVE_FIXNUM_0x11:
                case MsgPackType.POSITIVE_FIXNUM_0x12:
                case MsgPackType.POSITIVE_FIXNUM_0x13:
                case MsgPackType.POSITIVE_FIXNUM_0x14:
                case MsgPackType.POSITIVE_FIXNUM_0x15:
                case MsgPackType.POSITIVE_FIXNUM_0x16:
                case MsgPackType.POSITIVE_FIXNUM_0x17:
                case MsgPackType.POSITIVE_FIXNUM_0x18:
                case MsgPackType.POSITIVE_FIXNUM_0x19:
                case MsgPackType.POSITIVE_FIXNUM_0x1A:
                case MsgPackType.POSITIVE_FIXNUM_0x1B:
                case MsgPackType.POSITIVE_FIXNUM_0x1C:
                case MsgPackType.POSITIVE_FIXNUM_0x1D:
                case MsgPackType.POSITIVE_FIXNUM_0x1E:
                case MsgPackType.POSITIVE_FIXNUM_0x1F:

                case MsgPackType.POSITIVE_FIXNUM_0x20:
                case MsgPackType.POSITIVE_FIXNUM_0x21:
                case MsgPackType.POSITIVE_FIXNUM_0x22:
                case MsgPackType.POSITIVE_FIXNUM_0x23:
                case MsgPackType.POSITIVE_FIXNUM_0x24:
                case MsgPackType.POSITIVE_FIXNUM_0x25:
                case MsgPackType.POSITIVE_FIXNUM_0x26:
                case MsgPackType.POSITIVE_FIXNUM_0x27:
                case MsgPackType.POSITIVE_FIXNUM_0x28:
                case MsgPackType.POSITIVE_FIXNUM_0x29:
                case MsgPackType.POSITIVE_FIXNUM_0x2A:
                case MsgPackType.POSITIVE_FIXNUM_0x2B:
                case MsgPackType.POSITIVE_FIXNUM_0x2C:
                case MsgPackType.POSITIVE_FIXNUM_0x2D:
                case MsgPackType.POSITIVE_FIXNUM_0x2E:
                case MsgPackType.POSITIVE_FIXNUM_0x2F:

                case MsgPackType.POSITIVE_FIXNUM_0x30:
                case MsgPackType.POSITIVE_FIXNUM_0x31:
                case MsgPackType.POSITIVE_FIXNUM_0x32:
                case MsgPackType.POSITIVE_FIXNUM_0x33:
                case MsgPackType.POSITIVE_FIXNUM_0x34:
                case MsgPackType.POSITIVE_FIXNUM_0x35:
                case MsgPackType.POSITIVE_FIXNUM_0x36:
                case MsgPackType.POSITIVE_FIXNUM_0x37:
                case MsgPackType.POSITIVE_FIXNUM_0x38:
                case MsgPackType.POSITIVE_FIXNUM_0x39:
                case MsgPackType.POSITIVE_FIXNUM_0x3A:
                case MsgPackType.POSITIVE_FIXNUM_0x3B:
                case MsgPackType.POSITIVE_FIXNUM_0x3C:
                case MsgPackType.POSITIVE_FIXNUM_0x3D:
                case MsgPackType.POSITIVE_FIXNUM_0x3E:
                case MsgPackType.POSITIVE_FIXNUM_0x3F:

                case MsgPackType.POSITIVE_FIXNUM_0x40:
                case MsgPackType.POSITIVE_FIXNUM_0x41:
                case MsgPackType.POSITIVE_FIXNUM_0x42:
                case MsgPackType.POSITIVE_FIXNUM_0x43:
                case MsgPackType.POSITIVE_FIXNUM_0x44:
                case MsgPackType.POSITIVE_FIXNUM_0x45:
                case MsgPackType.POSITIVE_FIXNUM_0x46:
                case MsgPackType.POSITIVE_FIXNUM_0x47:
                case MsgPackType.POSITIVE_FIXNUM_0x48:
                case MsgPackType.POSITIVE_FIXNUM_0x49:
                case MsgPackType.POSITIVE_FIXNUM_0x4A:
                case MsgPackType.POSITIVE_FIXNUM_0x4B:
                case MsgPackType.POSITIVE_FIXNUM_0x4C:
                case MsgPackType.POSITIVE_FIXNUM_0x4D:
                case MsgPackType.POSITIVE_FIXNUM_0x4E:
                case MsgPackType.POSITIVE_FIXNUM_0x4F:

                case MsgPackType.POSITIVE_FIXNUM_0x50:
                case MsgPackType.POSITIVE_FIXNUM_0x51:
                case MsgPackType.POSITIVE_FIXNUM_0x52:
                case MsgPackType.POSITIVE_FIXNUM_0x53:
                case MsgPackType.POSITIVE_FIXNUM_0x54:
                case MsgPackType.POSITIVE_FIXNUM_0x55:
                case MsgPackType.POSITIVE_FIXNUM_0x56:
                case MsgPackType.POSITIVE_FIXNUM_0x57:
                case MsgPackType.POSITIVE_FIXNUM_0x58:
                case MsgPackType.POSITIVE_FIXNUM_0x59:
                case MsgPackType.POSITIVE_FIXNUM_0x5A:
                case MsgPackType.POSITIVE_FIXNUM_0x5B:
                case MsgPackType.POSITIVE_FIXNUM_0x5C:
                case MsgPackType.POSITIVE_FIXNUM_0x5D:
                case MsgPackType.POSITIVE_FIXNUM_0x5E:
                case MsgPackType.POSITIVE_FIXNUM_0x5F:

                case MsgPackType.POSITIVE_FIXNUM_0x60:
                case MsgPackType.POSITIVE_FIXNUM_0x61:
                case MsgPackType.POSITIVE_FIXNUM_0x62:
                case MsgPackType.POSITIVE_FIXNUM_0x63:
                case MsgPackType.POSITIVE_FIXNUM_0x64:
                case MsgPackType.POSITIVE_FIXNUM_0x65:
                case MsgPackType.POSITIVE_FIXNUM_0x66:
                case MsgPackType.POSITIVE_FIXNUM_0x67:
                case MsgPackType.POSITIVE_FIXNUM_0x68:
                case MsgPackType.POSITIVE_FIXNUM_0x69:
                case MsgPackType.POSITIVE_FIXNUM_0x6A:
                case MsgPackType.POSITIVE_FIXNUM_0x6B:
                case MsgPackType.POSITIVE_FIXNUM_0x6C:
                case MsgPackType.POSITIVE_FIXNUM_0x6D:
                case MsgPackType.POSITIVE_FIXNUM_0x6E:
                case MsgPackType.POSITIVE_FIXNUM_0x6F:

                case MsgPackType.POSITIVE_FIXNUM_0x70:
                case MsgPackType.POSITIVE_FIXNUM_0x71:
                case MsgPackType.POSITIVE_FIXNUM_0x72:
                case MsgPackType.POSITIVE_FIXNUM_0x73:
                case MsgPackType.POSITIVE_FIXNUM_0x74:
                case MsgPackType.POSITIVE_FIXNUM_0x75:
                case MsgPackType.POSITIVE_FIXNUM_0x76:
                case MsgPackType.POSITIVE_FIXNUM_0x77:
                case MsgPackType.POSITIVE_FIXNUM_0x78:
                case MsgPackType.POSITIVE_FIXNUM_0x79:
                case MsgPackType.POSITIVE_FIXNUM_0x7A:
                case MsgPackType.POSITIVE_FIXNUM_0x7B:
                case MsgPackType.POSITIVE_FIXNUM_0x7C:
                case MsgPackType.POSITIVE_FIXNUM_0x7D:
                case MsgPackType.POSITIVE_FIXNUM_0x7E:
                case MsgPackType.POSITIVE_FIXNUM_0x7F:

                case MsgPackType.NEGATIVE_FIXNUM:
                case MsgPackType.NEGATIVE_FIXNUM_0x01:
                case MsgPackType.NEGATIVE_FIXNUM_0x02:
                case MsgPackType.NEGATIVE_FIXNUM_0x03:
                case MsgPackType.NEGATIVE_FIXNUM_0x04:
                case MsgPackType.NEGATIVE_FIXNUM_0x05:
                case MsgPackType.NEGATIVE_FIXNUM_0x06:
                case MsgPackType.NEGATIVE_FIXNUM_0x07:
                case MsgPackType.NEGATIVE_FIXNUM_0x08:
                case MsgPackType.NEGATIVE_FIXNUM_0x09:
                case MsgPackType.NEGATIVE_FIXNUM_0x0A:
                case MsgPackType.NEGATIVE_FIXNUM_0x0B:
                case MsgPackType.NEGATIVE_FIXNUM_0x0C:
                case MsgPackType.NEGATIVE_FIXNUM_0x0D:
                case MsgPackType.NEGATIVE_FIXNUM_0x0E:
                case MsgPackType.NEGATIVE_FIXNUM_0x0F:
                case MsgPackType.NEGATIVE_FIXNUM_0x10:
                case MsgPackType.NEGATIVE_FIXNUM_0x11:
                case MsgPackType.NEGATIVE_FIXNUM_0x12:
                case MsgPackType.NEGATIVE_FIXNUM_0x13:
                case MsgPackType.NEGATIVE_FIXNUM_0x14:
                case MsgPackType.NEGATIVE_FIXNUM_0x15:
                case MsgPackType.NEGATIVE_FIXNUM_0x16:
                case MsgPackType.NEGATIVE_FIXNUM_0x17:
                case MsgPackType.NEGATIVE_FIXNUM_0x18:
                case MsgPackType.NEGATIVE_FIXNUM_0x19:
                case MsgPackType.NEGATIVE_FIXNUM_0x1A:
                case MsgPackType.NEGATIVE_FIXNUM_0x1B:
                case MsgPackType.NEGATIVE_FIXNUM_0x1C:
                case MsgPackType.NEGATIVE_FIXNUM_0x1D:
                case MsgPackType.NEGATIVE_FIXNUM_0x1E:
                case MsgPackType.NEGATIVE_FIXNUM_0x1F:
                    return bytes.Advance(1).Take(0);

                case MsgPackType.UINT8:
                case MsgPackType.INT8:
                    return bytes.Advance(1).Take(1);

                case MsgPackType.UINT16:
                case MsgPackType.INT16:
                    return bytes.Advance(1).Take(2);

                case MsgPackType.UINT32:
                case MsgPackType.INT32:
                case MsgPackType.FLOAT:
                    return bytes.Advance(1).Take(4);

                case MsgPackType.UINT64:
                case MsgPackType.INT64:
                case MsgPackType.DOUBLE:
                    return bytes.Advance(1).Take(8);

                case MsgPackType.FIX_EXT_1:
                    return bytes.Advance(2).Take(1);
                case MsgPackType.FIX_EXT_2:
                    return bytes.Advance(2).Take(2);
                case MsgPackType.FIX_EXT_4:
                    return bytes.Advance(2).Take(4);
                case MsgPackType.FIX_EXT_8:
                    return bytes.Advance(2).Take(8);
                case MsgPackType.FIX_EXT_16:
                    return bytes.Advance(2).Take(16);
                case MsgPackType.EXT8:
                    {
                        var count = bytes.Get(1);
                        return bytes.Advance(1 + 1 + 1).Take(count);
                    }
                case MsgPackType.EXT16:
                    {
                        var count = EndianConverter.NetworkByteWordToUnsignedNativeByteOrder(bytes.Advance(1));
                        return bytes.Advance(1 + 2 + 1).Take(count);
                    }
                case MsgPackType.EXT32:
                    {
                        var count = EndianConverter.NetworkByteDWordToUnsignedNativeByteOrder(bytes.Advance(1));
                        return bytes.Advance(1 + 4 + 1).Take((int)count);
                    }
                default:
                    throw new ArgumentException("unknown type: " + formatType);
            }
        }

        static ListTreeNode<MsgPackValue> _Parse(ListTreeNode<MsgPackValue> tree, ArraySegment<Byte> bytes)
        {
            MsgPackType formatType = GetFormat(bytes);
            if (formatType.IsArray())
            {
                var array = tree.AddValue(bytes, ValueNodeType.Array);

                uint count;
                bytes = GetItemCount(bytes, formatType, out count);
                for (var i = 0; i < count; ++i)
                {
                    var child = _Parse(array, bytes);
                    bytes = bytes.Advance(child.Value.Bytes.Count);
                }

                array.SetValueBytesCount(bytes.Offset - array.Value.Bytes.Offset);

                return array;
            }
            else if (formatType.IsMap())
            {
                var obj = tree.AddValue(bytes, ValueNodeType.Object);

                uint count;
                bytes = GetItemCount(bytes, formatType, out count);
                for (var i = 0; i < count; ++i)
                {
                    // key
                    var key = _Parse(obj, bytes);
                    bytes = bytes.Advance(key.Value.Bytes.Count);

                    // value
                    var value = _Parse(obj, bytes);
                    bytes = bytes.Advance(value.Value.Bytes.Count);
                }

                obj.SetValueBytesCount(bytes.Offset - obj.Value.Bytes.Offset);

                return obj;
            }
            else
            {
                var body = GetBody(bytes, formatType);
                var headerSize = body.Offset - bytes.Offset;
                var size = headerSize + body.Count;

                var value = tree.AddValue(bytes.Take(size), ValueNodeType.Null);
                return value;
            }
        }

        public static ListTreeNode<MsgPackValue> Parse(ArraySegment<Byte> bytes)
        {
            return _Parse(default(ListTreeNode<MsgPackValue>), bytes);
        }
    }
}
