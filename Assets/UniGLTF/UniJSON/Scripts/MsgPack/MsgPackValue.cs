using System;
using System.Text;

namespace UniJSON
{
    public struct MsgPackValue: IListTreeItem, IValue<MsgPackValue>
    {
        public int ParentIndex
        {
            get;
            private set;
        }

        public ArraySegment<Byte> Bytes
        {
            get;
            private set;
        }
        public void SetBytesCount(int count)
        {
            Bytes = new ArraySegment<byte>(Bytes.Array, Bytes.Offset, count);
        }

        public MsgPackType Format
        {
            get
            {
                return (MsgPackType)Bytes.Get(0);
            }
        }

        public ValueNodeType ValueType
        {
            get
            {
                switch (Format)
                {
                    case MsgPackType.NIL:
                        return ValueNodeType.Null;

                    case MsgPackType.TRUE:
                    case MsgPackType.FALSE:
                        return ValueNodeType.Boolean;

                    default:
                        if (Format.IsArray())
                        {
                            return ValueNodeType.Array;
                        }
                        else if (Format.IsMap())
                        {
                            return ValueNodeType.Object;
                        }
                        else if (Format.IsInteger())
                        {
                            return ValueNodeType.Integer;
                        }
                        else if (Format.IsFloat())
                        {
                            return ValueNodeType.Number;
                        }
                        else if (Format.IsString())
                        {
                            return ValueNodeType.String;
                        }
                        else if (Format.IsBinary())
                        {
                            return ValueNodeType.Binary;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                }
            }
        }

        int _childCount;
        public int ChildCount
        {
            get { return _childCount; }
        }
        public void SetChildCount(int count)
        {
            _childCount = count;
        }

        public MsgPackValue(ArraySegment<Byte> segment, int parentIndex) : this()
        {
            Bytes = segment;
            ParentIndex = parentIndex;
        }

        public MsgPackValue New(ArraySegment<byte> bytes, ValueNodeType valueType, int parentIndex)
        {
            return new MsgPackValue(bytes, parentIndex);
        }

        public MsgPackValue Key(Utf8String key, int parentIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ArrayとMap以外のタイプのペイロードを得る
        /// </summary>
        /// <returns></returns>
        public ArraySegment<Byte> GetBody()
        {
            var bytes = Bytes;
            var formatType = Format;
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

        public SByte GetExtType()
        {
            var formatType = Format;
            switch (formatType)
            {
                case MsgPackType.FIX_EXT_4:
                    return (SByte)Bytes.Get(1);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// ArrayとMap以外のタイプの値を得る
        /// </summary>
        /// <returns></returns>
        public T GetValue<T>()
        {
            var formatType = Format;
            switch (formatType)
            {
                case MsgPackType.NIL: return GenericCast<object, T>.Null();
                case MsgPackType.TRUE: return GenericCast<bool, T>.Const(true)();
                case MsgPackType.FALSE: return GenericCast<bool, T>.Const(false)();
                case MsgPackType.POSITIVE_FIXNUM: return GenericCast<int, T>.Const(0)();
                case MsgPackType.POSITIVE_FIXNUM_0x01: return GenericCast<int, T>.Const(1)();
                case MsgPackType.POSITIVE_FIXNUM_0x02: return GenericCast<int, T>.Const(2)();
                case MsgPackType.POSITIVE_FIXNUM_0x03: return GenericCast<int, T>.Const(3)();
                case MsgPackType.POSITIVE_FIXNUM_0x04: return GenericCast<int, T>.Const(4)();
                case MsgPackType.POSITIVE_FIXNUM_0x05: return GenericCast<int, T>.Const(5)();
                case MsgPackType.POSITIVE_FIXNUM_0x06: return GenericCast<int, T>.Const(6)();
                case MsgPackType.POSITIVE_FIXNUM_0x07: return GenericCast<int, T>.Const(7)();
                case MsgPackType.POSITIVE_FIXNUM_0x08: return GenericCast<int, T>.Const(8)();
                case MsgPackType.POSITIVE_FIXNUM_0x09: return GenericCast<int, T>.Const(9)();
                case MsgPackType.POSITIVE_FIXNUM_0x0A: return GenericCast<int, T>.Const(10)();
                case MsgPackType.POSITIVE_FIXNUM_0x0B: return GenericCast<int, T>.Const(11)();
                case MsgPackType.POSITIVE_FIXNUM_0x0C: return GenericCast<int, T>.Const(12)();
                case MsgPackType.POSITIVE_FIXNUM_0x0D: return GenericCast<int, T>.Const(13)();
                case MsgPackType.POSITIVE_FIXNUM_0x0E: return GenericCast<int, T>.Const(14)();
                case MsgPackType.POSITIVE_FIXNUM_0x0F: return GenericCast<int, T>.Const(15)();

                case MsgPackType.POSITIVE_FIXNUM_0x10: return GenericCast<int, T>.Const(16)();
                case MsgPackType.POSITIVE_FIXNUM_0x11: return GenericCast<int, T>.Const(17)();
                case MsgPackType.POSITIVE_FIXNUM_0x12: return GenericCast<int, T>.Const(18)();
                case MsgPackType.POSITIVE_FIXNUM_0x13: return GenericCast<int, T>.Const(19)();
                case MsgPackType.POSITIVE_FIXNUM_0x14: return GenericCast<int, T>.Const(20)();
                case MsgPackType.POSITIVE_FIXNUM_0x15: return GenericCast<int, T>.Const(21)();
                case MsgPackType.POSITIVE_FIXNUM_0x16: return GenericCast<int, T>.Const(22)();
                case MsgPackType.POSITIVE_FIXNUM_0x17: return GenericCast<int, T>.Const(23)();
                case MsgPackType.POSITIVE_FIXNUM_0x18: return GenericCast<int, T>.Const(24)();
                case MsgPackType.POSITIVE_FIXNUM_0x19: return GenericCast<int, T>.Const(25)();
                case MsgPackType.POSITIVE_FIXNUM_0x1A: return GenericCast<int, T>.Const(26)();
                case MsgPackType.POSITIVE_FIXNUM_0x1B: return GenericCast<int, T>.Const(27)();
                case MsgPackType.POSITIVE_FIXNUM_0x1C: return GenericCast<int, T>.Const(28)();
                case MsgPackType.POSITIVE_FIXNUM_0x1D: return GenericCast<int, T>.Const(29)();
                case MsgPackType.POSITIVE_FIXNUM_0x1E: return GenericCast<int, T>.Const(30)();
                case MsgPackType.POSITIVE_FIXNUM_0x1F: return GenericCast<int, T>.Const(31)();

                case MsgPackType.POSITIVE_FIXNUM_0x20: return GenericCast<int, T>.Const(32)();
                case MsgPackType.POSITIVE_FIXNUM_0x21: return GenericCast<int, T>.Const(33)();
                case MsgPackType.POSITIVE_FIXNUM_0x22: return GenericCast<int, T>.Const(34)();
                case MsgPackType.POSITIVE_FIXNUM_0x23: return GenericCast<int, T>.Const(35)();
                case MsgPackType.POSITIVE_FIXNUM_0x24: return GenericCast<int, T>.Const(36)();
                case MsgPackType.POSITIVE_FIXNUM_0x25: return GenericCast<int, T>.Const(37)();
                case MsgPackType.POSITIVE_FIXNUM_0x26: return GenericCast<int, T>.Const(38)();
                case MsgPackType.POSITIVE_FIXNUM_0x27: return GenericCast<int, T>.Const(39)();
                case MsgPackType.POSITIVE_FIXNUM_0x28: return GenericCast<int, T>.Const(40)();
                case MsgPackType.POSITIVE_FIXNUM_0x29: return GenericCast<int, T>.Const(41)();
                case MsgPackType.POSITIVE_FIXNUM_0x2A: return GenericCast<int, T>.Const(42)();
                case MsgPackType.POSITIVE_FIXNUM_0x2B: return GenericCast<int, T>.Const(43)();
                case MsgPackType.POSITIVE_FIXNUM_0x2C: return GenericCast<int, T>.Const(44)();
                case MsgPackType.POSITIVE_FIXNUM_0x2D: return GenericCast<int, T>.Const(45)();
                case MsgPackType.POSITIVE_FIXNUM_0x2E: return GenericCast<int, T>.Const(46)();
                case MsgPackType.POSITIVE_FIXNUM_0x2F: return GenericCast<int, T>.Const(47)();

                case MsgPackType.POSITIVE_FIXNUM_0x30: return GenericCast<int, T>.Const(48)();
                case MsgPackType.POSITIVE_FIXNUM_0x31: return GenericCast<int, T>.Const(49)();
                case MsgPackType.POSITIVE_FIXNUM_0x32: return GenericCast<int, T>.Const(50)();
                case MsgPackType.POSITIVE_FIXNUM_0x33: return GenericCast<int, T>.Const(51)();
                case MsgPackType.POSITIVE_FIXNUM_0x34: return GenericCast<int, T>.Const(52)();
                case MsgPackType.POSITIVE_FIXNUM_0x35: return GenericCast<int, T>.Const(53)();
                case MsgPackType.POSITIVE_FIXNUM_0x36: return GenericCast<int, T>.Const(54)();
                case MsgPackType.POSITIVE_FIXNUM_0x37: return GenericCast<int, T>.Const(55)();
                case MsgPackType.POSITIVE_FIXNUM_0x38: return GenericCast<int, T>.Const(56)();
                case MsgPackType.POSITIVE_FIXNUM_0x39: return GenericCast<int, T>.Const(57)();
                case MsgPackType.POSITIVE_FIXNUM_0x3A: return GenericCast<int, T>.Const(58)();
                case MsgPackType.POSITIVE_FIXNUM_0x3B: return GenericCast<int, T>.Const(59)();
                case MsgPackType.POSITIVE_FIXNUM_0x3C: return GenericCast<int, T>.Const(60)();
                case MsgPackType.POSITIVE_FIXNUM_0x3D: return GenericCast<int, T>.Const(61)();
                case MsgPackType.POSITIVE_FIXNUM_0x3E: return GenericCast<int, T>.Const(62)();
                case MsgPackType.POSITIVE_FIXNUM_0x3F: return GenericCast<int, T>.Const(63)();

                case MsgPackType.POSITIVE_FIXNUM_0x40: return GenericCast<int, T>.Const(64)();
                case MsgPackType.POSITIVE_FIXNUM_0x41: return GenericCast<int, T>.Const(65)();
                case MsgPackType.POSITIVE_FIXNUM_0x42: return GenericCast<int, T>.Const(66)();
                case MsgPackType.POSITIVE_FIXNUM_0x43: return GenericCast<int, T>.Const(67)();
                case MsgPackType.POSITIVE_FIXNUM_0x44: return GenericCast<int, T>.Const(68)();
                case MsgPackType.POSITIVE_FIXNUM_0x45: return GenericCast<int, T>.Const(69)();
                case MsgPackType.POSITIVE_FIXNUM_0x46: return GenericCast<int, T>.Const(70)();
                case MsgPackType.POSITIVE_FIXNUM_0x47: return GenericCast<int, T>.Const(71)();
                case MsgPackType.POSITIVE_FIXNUM_0x48: return GenericCast<int, T>.Const(72)();
                case MsgPackType.POSITIVE_FIXNUM_0x49: return GenericCast<int, T>.Const(73)();
                case MsgPackType.POSITIVE_FIXNUM_0x4A: return GenericCast<int, T>.Const(74)();
                case MsgPackType.POSITIVE_FIXNUM_0x4B: return GenericCast<int, T>.Const(75)();
                case MsgPackType.POSITIVE_FIXNUM_0x4C: return GenericCast<int, T>.Const(76)();
                case MsgPackType.POSITIVE_FIXNUM_0x4D: return GenericCast<int, T>.Const(77)();
                case MsgPackType.POSITIVE_FIXNUM_0x4E: return GenericCast<int, T>.Const(78)();
                case MsgPackType.POSITIVE_FIXNUM_0x4F: return GenericCast<int, T>.Const(79)();

                case MsgPackType.POSITIVE_FIXNUM_0x50: return GenericCast<int, T>.Const(80)();
                case MsgPackType.POSITIVE_FIXNUM_0x51: return GenericCast<int, T>.Const(81)();
                case MsgPackType.POSITIVE_FIXNUM_0x52: return GenericCast<int, T>.Const(82)();
                case MsgPackType.POSITIVE_FIXNUM_0x53: return GenericCast<int, T>.Const(83)();
                case MsgPackType.POSITIVE_FIXNUM_0x54: return GenericCast<int, T>.Const(84)();
                case MsgPackType.POSITIVE_FIXNUM_0x55: return GenericCast<int, T>.Const(85)();
                case MsgPackType.POSITIVE_FIXNUM_0x56: return GenericCast<int, T>.Const(86)();
                case MsgPackType.POSITIVE_FIXNUM_0x57: return GenericCast<int, T>.Const(87)();
                case MsgPackType.POSITIVE_FIXNUM_0x58: return GenericCast<int, T>.Const(88)();
                case MsgPackType.POSITIVE_FIXNUM_0x59: return GenericCast<int, T>.Const(89)();
                case MsgPackType.POSITIVE_FIXNUM_0x5A: return GenericCast<int, T>.Const(90)();
                case MsgPackType.POSITIVE_FIXNUM_0x5B: return GenericCast<int, T>.Const(91)();
                case MsgPackType.POSITIVE_FIXNUM_0x5C: return GenericCast<int, T>.Const(92)();
                case MsgPackType.POSITIVE_FIXNUM_0x5D: return GenericCast<int, T>.Const(93)();
                case MsgPackType.POSITIVE_FIXNUM_0x5E: return GenericCast<int, T>.Const(94)();
                case MsgPackType.POSITIVE_FIXNUM_0x5F: return GenericCast<int, T>.Const(95)();

                case MsgPackType.POSITIVE_FIXNUM_0x60: return GenericCast<int, T>.Const(96)();
                case MsgPackType.POSITIVE_FIXNUM_0x61: return GenericCast<int, T>.Const(97)();
                case MsgPackType.POSITIVE_FIXNUM_0x62: return GenericCast<int, T>.Const(98)();
                case MsgPackType.POSITIVE_FIXNUM_0x63: return GenericCast<int, T>.Const(99)();
                case MsgPackType.POSITIVE_FIXNUM_0x64: return GenericCast<int, T>.Const(100)();
                case MsgPackType.POSITIVE_FIXNUM_0x65: return GenericCast<int, T>.Const(101)();
                case MsgPackType.POSITIVE_FIXNUM_0x66: return GenericCast<int, T>.Const(102)();
                case MsgPackType.POSITIVE_FIXNUM_0x67: return GenericCast<int, T>.Const(103)();
                case MsgPackType.POSITIVE_FIXNUM_0x68: return GenericCast<int, T>.Const(104)();
                case MsgPackType.POSITIVE_FIXNUM_0x69: return GenericCast<int, T>.Const(105)();
                case MsgPackType.POSITIVE_FIXNUM_0x6A: return GenericCast<int, T>.Const(106)();
                case MsgPackType.POSITIVE_FIXNUM_0x6B: return GenericCast<int, T>.Const(107)();
                case MsgPackType.POSITIVE_FIXNUM_0x6C: return GenericCast<int, T>.Const(108)();
                case MsgPackType.POSITIVE_FIXNUM_0x6D: return GenericCast<int, T>.Const(109)();
                case MsgPackType.POSITIVE_FIXNUM_0x6E: return GenericCast<int, T>.Const(110)();
                case MsgPackType.POSITIVE_FIXNUM_0x6F: return GenericCast<int, T>.Const(111)();

                case MsgPackType.POSITIVE_FIXNUM_0x70: return GenericCast<int, T>.Const(112)();
                case MsgPackType.POSITIVE_FIXNUM_0x71: return GenericCast<int, T>.Const(113)();
                case MsgPackType.POSITIVE_FIXNUM_0x72: return GenericCast<int, T>.Const(114)();
                case MsgPackType.POSITIVE_FIXNUM_0x73: return GenericCast<int, T>.Const(115)();
                case MsgPackType.POSITIVE_FIXNUM_0x74: return GenericCast<int, T>.Const(116)();
                case MsgPackType.POSITIVE_FIXNUM_0x75: return GenericCast<int, T>.Const(117)();
                case MsgPackType.POSITIVE_FIXNUM_0x76: return GenericCast<int, T>.Const(118)();
                case MsgPackType.POSITIVE_FIXNUM_0x77: return GenericCast<int, T>.Const(119)();
                case MsgPackType.POSITIVE_FIXNUM_0x78: return GenericCast<int, T>.Const(120)();
                case MsgPackType.POSITIVE_FIXNUM_0x79: return GenericCast<int, T>.Const(121)();
                case MsgPackType.POSITIVE_FIXNUM_0x7A: return GenericCast<int, T>.Const(122)();
                case MsgPackType.POSITIVE_FIXNUM_0x7B: return GenericCast<int, T>.Const(123)();
                case MsgPackType.POSITIVE_FIXNUM_0x7C: return GenericCast<int, T>.Const(124)();
                case MsgPackType.POSITIVE_FIXNUM_0x7D: return GenericCast<int, T>.Const(125)();
                case MsgPackType.POSITIVE_FIXNUM_0x7E: return GenericCast<int, T>.Const(126)();
                case MsgPackType.POSITIVE_FIXNUM_0x7F: return GenericCast<int, T>.Const(127)();

                case MsgPackType.NEGATIVE_FIXNUM: return GenericCast<int, T>.Const(-32)();
                case MsgPackType.NEGATIVE_FIXNUM_0x01: return GenericCast<int, T>.Const(-1)();
                case MsgPackType.NEGATIVE_FIXNUM_0x02: return GenericCast<int, T>.Const(-2)();
                case MsgPackType.NEGATIVE_FIXNUM_0x03: return GenericCast<int, T>.Const(-3)();
                case MsgPackType.NEGATIVE_FIXNUM_0x04: return GenericCast<int, T>.Const(-4)();
                case MsgPackType.NEGATIVE_FIXNUM_0x05: return GenericCast<int, T>.Const(-5)();
                case MsgPackType.NEGATIVE_FIXNUM_0x06: return GenericCast<int, T>.Const(-6)();
                case MsgPackType.NEGATIVE_FIXNUM_0x07: return GenericCast<int, T>.Const(-7)();
                case MsgPackType.NEGATIVE_FIXNUM_0x08: return GenericCast<int, T>.Const(-8)();
                case MsgPackType.NEGATIVE_FIXNUM_0x09: return GenericCast<int, T>.Const(-9)();
                case MsgPackType.NEGATIVE_FIXNUM_0x0A: return GenericCast<int, T>.Const(-10)();
                case MsgPackType.NEGATIVE_FIXNUM_0x0B: return GenericCast<int, T>.Const(-11)();
                case MsgPackType.NEGATIVE_FIXNUM_0x0C: return GenericCast<int, T>.Const(-12)();
                case MsgPackType.NEGATIVE_FIXNUM_0x0D: return GenericCast<int, T>.Const(-13)();
                case MsgPackType.NEGATIVE_FIXNUM_0x0E: return GenericCast<int, T>.Const(-14)();
                case MsgPackType.NEGATIVE_FIXNUM_0x0F: return GenericCast<int, T>.Const(-15)();
                case MsgPackType.NEGATIVE_FIXNUM_0x10: return GenericCast<int, T>.Const(-16)();
                case MsgPackType.NEGATIVE_FIXNUM_0x11: return GenericCast<int, T>.Const(-17)();
                case MsgPackType.NEGATIVE_FIXNUM_0x12: return GenericCast<int, T>.Const(-18)();
                case MsgPackType.NEGATIVE_FIXNUM_0x13: return GenericCast<int, T>.Const(-19)();
                case MsgPackType.NEGATIVE_FIXNUM_0x14: return GenericCast<int, T>.Const(-20)();
                case MsgPackType.NEGATIVE_FIXNUM_0x15: return GenericCast<int, T>.Const(-21)();
                case MsgPackType.NEGATIVE_FIXNUM_0x16: return GenericCast<int, T>.Const(-22)();
                case MsgPackType.NEGATIVE_FIXNUM_0x17: return GenericCast<int, T>.Const(-23)();
                case MsgPackType.NEGATIVE_FIXNUM_0x18: return GenericCast<int, T>.Const(-24)();
                case MsgPackType.NEGATIVE_FIXNUM_0x19: return GenericCast<int, T>.Const(-25)();
                case MsgPackType.NEGATIVE_FIXNUM_0x1A: return GenericCast<int, T>.Const(-26)();
                case MsgPackType.NEGATIVE_FIXNUM_0x1B: return GenericCast<int, T>.Const(-27)();
                case MsgPackType.NEGATIVE_FIXNUM_0x1C: return GenericCast<int, T>.Const(-28)();
                case MsgPackType.NEGATIVE_FIXNUM_0x1D: return GenericCast<int, T>.Const(-29)();
                case MsgPackType.NEGATIVE_FIXNUM_0x1E: return GenericCast<int, T>.Const(-30)();
                case MsgPackType.NEGATIVE_FIXNUM_0x1F: return GenericCast<int, T>.Const(-31)();

                case MsgPackType.INT8: return GenericCast<SByte, T>.Cast((SByte)GetBody().Get(0));
                case MsgPackType.INT16: return GenericCast<short, T>.Cast(EndianConverter.NetworkByteWordToSignedNativeByteOrder(GetBody()));
                case MsgPackType.INT32: return GenericCast<int, T>.Cast(EndianConverter.NetworkByteDWordToSignedNativeByteOrder(GetBody()));
                case MsgPackType.INT64: return GenericCast<long, T>.Cast(EndianConverter.NetworkByteQWordToSignedNativeByteOrder(GetBody()));
                case MsgPackType.UINT8: return GenericCast<Byte, T>.Cast(GetBody().Get(0));
                case MsgPackType.UINT16: return GenericCast<ushort, T>.Cast(EndianConverter.NetworkByteWordToUnsignedNativeByteOrder(GetBody()));
                case MsgPackType.UINT32: return GenericCast<uint, T>.Cast(EndianConverter.NetworkByteDWordToUnsignedNativeByteOrder(GetBody()));
                case MsgPackType.UINT64: return GenericCast<ulong, T>.Cast(EndianConverter.NetworkByteQWordToUnsignedNativeByteOrder(GetBody()));
                case MsgPackType.FLOAT: return GenericCast<float, T>.Cast(EndianConverter.NetworkByteDWordToFloatNativeByteOrder(GetBody()));
                case MsgPackType.DOUBLE: return GenericCast<double, T>.Cast(EndianConverter.NetworkByteQWordToFloatNativeByteOrder(GetBody()));

                case MsgPackType.FIX_STR: return GenericCast<string, T>.Const("")();
                case MsgPackType.FIX_STR_0x01:
                case MsgPackType.FIX_STR_0x02:
                case MsgPackType.FIX_STR_0x03:
                case MsgPackType.FIX_STR_0x04:
                case MsgPackType.FIX_STR_0x05:
                case MsgPackType.FIX_STR_0x06:
                case MsgPackType.FIX_STR_0x07:
                case MsgPackType.FIX_STR_0x08:
                case MsgPackType.FIX_STR_0x09:
                case MsgPackType.FIX_STR_0x0A:
                case MsgPackType.FIX_STR_0x0B:
                case MsgPackType.FIX_STR_0x0C:
                case MsgPackType.FIX_STR_0x0D:
                case MsgPackType.FIX_STR_0x0E:
                case MsgPackType.FIX_STR_0x0F:
                case MsgPackType.FIX_STR_0x10:
                case MsgPackType.FIX_STR_0x11:
                case MsgPackType.FIX_STR_0x12:
                case MsgPackType.FIX_STR_0x13:
                case MsgPackType.FIX_STR_0x14:
                case MsgPackType.FIX_STR_0x15:
                case MsgPackType.FIX_STR_0x16:
                case MsgPackType.FIX_STR_0x17:
                case MsgPackType.FIX_STR_0x18:
                case MsgPackType.FIX_STR_0x19:
                case MsgPackType.FIX_STR_0x1A:
                case MsgPackType.FIX_STR_0x1B:
                case MsgPackType.FIX_STR_0x1C:
                case MsgPackType.FIX_STR_0x1D:
                case MsgPackType.FIX_STR_0x1E:
                case MsgPackType.FIX_STR_0x1F:
                case MsgPackType.STR8:
                case MsgPackType.STR16:
                case MsgPackType.STR32:
                    {
                        var body = GetBody();
                        var str = Encoding.UTF8.GetString(body.Array, body.Offset, body.Count);
                        return GenericCast<string, T>.Cast(str);
                    }

                case MsgPackType.BIN8:
                case MsgPackType.BIN16:
                case MsgPackType.BIN32:
                    {
                        var body = GetBody();
                        return GenericCast<ArraySegment<Byte>, T>.Cast(body);
                    }

                case MsgPackType.FIX_EXT_4:
                    {
                        if (GetExtType() == -1)
                        {
                            var unixtime = EndianConverter.NetworkByteDWordToUnsignedNativeByteOrder(GetBody());
                            var dt = new DateTimeOffset(unixtime * DateTimeOffsetExtensions.TicksPerSecond + DateTimeOffsetExtensions.EpochTime.Ticks, TimeSpan.Zero);
                            return GenericCast<DateTimeOffset, T>.Cast(dt);
                        }
                        break;
                    }
            }

            throw new ArgumentException("GetValue to array or map: " + formatType);
        }

        public bool GetBoolean()
        {
            switch (Format)
            {
                case MsgPackType.TRUE: return true;
                case MsgPackType.FALSE: return false;
                default: throw new MsgPackTypeException("Not boolean");
            }
        }

        public ArraySegment<Byte> GetBytes()
        {
            if (!Format.IsBinary())
            {
                throw new MsgPackTypeException("Not bin");
            }
            return GetBody();
        }

        public string GetString()
        {
            if (!Format.IsString())
            {
                throw new MsgPackTypeException("Not str");
            }
            var bytes = GetBody();
            return Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
        }

        public Utf8String GetUtf8String()
        {
            if (!Format.IsString())
            {
                throw new MsgPackTypeException("Not str");
            }
            var bytes = GetBody();
            return new Utf8String(bytes);
        }

        public SByte GetSByte()
        {
            return GetValue<SByte>();
        }

        public Int16 GetInt16()
        {
            return GetValue<Int16>();
        }

        public Int32 GetInt32()
        {
            return GetValue<Int32>();
        }

        public Int64 GetInt64()
        {
            return GetValue<Int64>();
        }

        public Byte GetByte()
        {
            return GetValue<Byte>();
        }

        public UInt16 GetUInt16()
        {
            return GetValue<UInt16>();
        }

        public UInt32 GetUInt32()
        {
            return GetValue<UInt32>();
        }

        public UInt64 GetUInt64()
        {
            return GetValue<UInt64>();
        }

        public float GetSingle()
        {
            return GetValue<Single>();
        }

        public double GetDouble()
        {
            return GetValue<Double>();
        }

        public void SetValue<T>(Utf8String jsonPointer, T value)
        {
            throw new NotImplementedException();
        }

        public void RemoveValue(Utf8String jsonPointer)
        {
            throw new NotImplementedException();
        }

        public void AddKey(Utf8String key)
        {
            throw new NotImplementedException();
        }

        public void AddValue(ArraySegment<byte> bytes, ValueNodeType valueType)
        {
            throw new NotImplementedException();
        }
    }
}
