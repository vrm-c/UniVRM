namespace UniJSON
{
    public static class MsgPackTypeExtensions
    {
        public static bool IsArray(this MsgPackType formatType)
        {
            switch (formatType)
            {
                case MsgPackType.FIX_ARRAY:
                case MsgPackType.FIX_ARRAY_0x1:
                case MsgPackType.FIX_ARRAY_0x2:
                case MsgPackType.FIX_ARRAY_0x3:
                case MsgPackType.FIX_ARRAY_0x4:
                case MsgPackType.FIX_ARRAY_0x5:
                case MsgPackType.FIX_ARRAY_0x6:
                case MsgPackType.FIX_ARRAY_0x7:
                case MsgPackType.FIX_ARRAY_0x8:
                case MsgPackType.FIX_ARRAY_0x9:
                case MsgPackType.FIX_ARRAY_0xA:
                case MsgPackType.FIX_ARRAY_0xB:
                case MsgPackType.FIX_ARRAY_0xC:
                case MsgPackType.FIX_ARRAY_0xD:
                case MsgPackType.FIX_ARRAY_0xE:
                case MsgPackType.FIX_ARRAY_0xF:
                case MsgPackType.ARRAY16:
                case MsgPackType.ARRAY32:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsMap(this MsgPackType formatType)
        {
            switch (formatType)
            {
                case MsgPackType.FIX_MAP:
                case MsgPackType.FIX_MAP_0x1:
                case MsgPackType.FIX_MAP_0x2:
                case MsgPackType.FIX_MAP_0x3:
                case MsgPackType.FIX_MAP_0x4:
                case MsgPackType.FIX_MAP_0x5:
                case MsgPackType.FIX_MAP_0x6:
                case MsgPackType.FIX_MAP_0x7:
                case MsgPackType.FIX_MAP_0x8:
                case MsgPackType.FIX_MAP_0x9:
                case MsgPackType.FIX_MAP_0xA:
                case MsgPackType.FIX_MAP_0xB:
                case MsgPackType.FIX_MAP_0xC:
                case MsgPackType.FIX_MAP_0xD:
                case MsgPackType.FIX_MAP_0xE:
                case MsgPackType.FIX_MAP_0xF:
                case MsgPackType.MAP16:
                case MsgPackType.MAP32:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsInteger(this MsgPackType formatType)
        {
            switch (formatType)
            {
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

                case MsgPackType.INT8:
                case MsgPackType.INT16:
                case MsgPackType.INT32:
                case MsgPackType.INT64:
                case MsgPackType.UINT8:
                case MsgPackType.UINT16:
                case MsgPackType.UINT32:
                case MsgPackType.UINT64:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsFloat(this MsgPackType formatType)
        {
            switch (formatType)
            {
                case MsgPackType.FLOAT:
                case MsgPackType.DOUBLE:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsString(this MsgPackType formatType)
        {
            switch (formatType)
            {
                case MsgPackType.FIX_STR:
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
                    return true;
            }
            return false;
        }

        public static bool IsExt(this MsgPackType formatType)
        {
            switch (formatType)
            {
                case MsgPackType.FIX_EXT_1:
                case MsgPackType.FIX_EXT_2:
                case MsgPackType.FIX_EXT_4:
                case MsgPackType.FIX_EXT_8:
                case MsgPackType.FIX_EXT_16:
                case MsgPackType.EXT8:
                case MsgPackType.EXT16:
                case MsgPackType.EXT32:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsBinary(this MsgPackType formatType)
        {
            switch (formatType)
            {
                case MsgPackType.BIN8:
                case MsgPackType.BIN16:
                case MsgPackType.BIN32:
                    return true;

                default:
                    return false;
            }
        }
    }
}
