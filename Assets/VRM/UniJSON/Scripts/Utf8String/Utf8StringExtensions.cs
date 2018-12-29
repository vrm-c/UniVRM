using System;
using System.Collections.Generic;
using System.IO;


namespace UniJSON
{
    public static class Utf8StringExtensions
    {
        public static void WriteTo(this Utf8String src, Stream dst)
        {
            dst.Write(src.Bytes.Array, src.Bytes.Offset, src.Bytes.Count);
        }

        public static Utf8Iterator GetFirst(this Utf8String src)
        {
            var it = src.GetIterator();
            it.MoveNext();
            return it;
        }

        public static bool TrySearchByte(this Utf8String src, Func<byte, bool> pred, out int pos)
        {
            pos = 0;
            for (; pos < src.ByteLength; ++pos)
            {
                if (pred(src[pos]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool TrySearchAscii(this Utf8String src, Byte target, int start, out int pos)
        {
            var p = new Utf8Iterator(src.Bytes, start);
            while (p.MoveNext())
            {
                var b = p.Current;
                if (b <= 0x7F)
                {
                    // ascii
                    if (b == target/*'\"'*/)
                    {
                        // closed
                        pos = p.BytePosition;
                        return true;
                    }
                    else if (b == '\\')
                    {
                        // escaped
                        switch ((char)p.Second)
                        {
                            case '"': // fall through
                            case '\\': // fall through
                            case '/': // fall through
                            case 'b': // fall through
                            case 'f': // fall through
                            case 'n': // fall through
                            case 'r': // fall through
                            case 't': // fall through
                                      // skip next
                                p.MoveNext();
                                break;

                            case 'u': // unicode
                                      // skip next 4
                                p.MoveNext();
                                p.MoveNext();
                                p.MoveNext();
                                p.MoveNext();
                                break;

                            default:
                                // unkonw escape
                                throw new ParserException("unknown escape: " + p.Second);
                        }
                    }
                }
            }

            pos = -1;
            return false;
        }

        public static IEnumerable<Utf8String> Split(this Utf8String src, byte delemeter)
        {
            var start = 0;
            var p = new Utf8Iterator(src.Bytes);
            while (p.MoveNext())
            {
                if (p.Current == delemeter)
                {
                    if (p.BytePosition - start == 0)
                    {
                        yield return default(Utf8String);
                    }
                    else
                    {
                        yield return src.Subbytes(start, p.BytePosition - start);
                    }
                    start = p.BytePosition + 1;
                }
            }

            if (start < p.BytePosition)
            {
                yield return src.Subbytes(start, p.BytePosition - start);
            }
        }

        #region atoi
        public static SByte ToSByte(this Utf8String src)
        {
            SByte value = 0;
            var p = new Utf8Iterator(src.Bytes);
            while (p.MoveNext())
            {
                var b = p.Current;
                switch (b)
                {
                    case 0x30: value = (SByte)(value * 10); break;
                    case 0x31: value = (SByte)(value * 10 + 1); break;
                    case 0x32: value = (SByte)(value * 10 + 2); break;
                    case 0x33: value = (SByte)(value * 10 + 3); break;
                    case 0x34: value = (SByte)(value * 10 + 4); break;
                    case 0x35: value = (SByte)(value * 10 + 5); break;
                    case 0x36: value = (SByte)(value * 10 + 6); break;
                    case 0x37: value = (SByte)(value * 10 + 7); break;
                    case 0x38: value = (SByte)(value * 10 + 8); break;
                    case 0x39: value = (SByte)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public static Int16 ToInt16(this Utf8String src)
        {
            Int16 value = 0;
            var p = new Utf8Iterator(src.Bytes);
            while (p.MoveNext())
            {
                var b = p.Current;
                switch (b)
                {
                    case 0x30: value = (Int16)(value * 10); break;
                    case 0x31: value = (Int16)(value * 10 + 1); break;
                    case 0x32: value = (Int16)(value * 10 + 2); break;
                    case 0x33: value = (Int16)(value * 10 + 3); break;
                    case 0x34: value = (Int16)(value * 10 + 4); break;
                    case 0x35: value = (Int16)(value * 10 + 5); break;
                    case 0x36: value = (Int16)(value * 10 + 6); break;
                    case 0x37: value = (Int16)(value * 10 + 7); break;
                    case 0x38: value = (Int16)(value * 10 + 8); break;
                    case 0x39: value = (Int16)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public static Int32 ToInt32(this Utf8String src)
        {
            Int32 value = 0;
            Int32 sign = 1;
            var p = new Utf8Iterator(src.Bytes);
            bool isFirst = true;
            while (p.MoveNext())
            {
                var b = p.Current;

                if (isFirst)
                {
                    isFirst = false;
                    if (b == '-')
                    {
                        sign = -1;
                        continue;
                    }
                }

                switch (b)
                {
                    case 0x30: value = value * 10; break;
                    case 0x31: value = value * 10 + 1; break;
                    case 0x32: value = value * 10 + 2; break;
                    case 0x33: value = value * 10 + 3; break;
                    case 0x34: value = value * 10 + 4; break;
                    case 0x35: value = value * 10 + 5; break;
                    case 0x36: value = value * 10 + 6; break;
                    case 0x37: value = value * 10 + 7; break;
                    case 0x38: value = value * 10 + 8; break;
                    case 0x39: value = value * 10 + 9; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value * sign;
        }
        public static Int64 ToInt64(this Utf8String src)
        {
            Int64 value = 0;
            var p = new Utf8Iterator(src.Bytes);
            while (p.MoveNext())
            {
                var b = p.Current;
                switch (b)
                {
                    case 0x30: value = (Int64)(value * 10); break;
                    case 0x31: value = (Int64)(value * 10 + 1); break;
                    case 0x32: value = (Int64)(value * 10 + 2); break;
                    case 0x33: value = (Int64)(value * 10 + 3); break;
                    case 0x34: value = (Int64)(value * 10 + 4); break;
                    case 0x35: value = (Int64)(value * 10 + 5); break;
                    case 0x36: value = (Int64)(value * 10 + 6); break;
                    case 0x37: value = (Int64)(value * 10 + 7); break;
                    case 0x38: value = (Int64)(value * 10 + 8); break;
                    case 0x39: value = (Int64)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public static Byte ToByte(this Utf8String src)
        {
            Byte value = 0;
            var p = new Utf8Iterator(src.Bytes);
            while (p.MoveNext())
            {
                var b = p.Current;
                switch (b)
                {
                    case 0x30: value = (Byte)(value * 10); break;
                    case 0x31: value = (Byte)(value * 10 + 1); break;
                    case 0x32: value = (Byte)(value * 10 + 2); break;
                    case 0x33: value = (Byte)(value * 10 + 3); break;
                    case 0x34: value = (Byte)(value * 10 + 4); break;
                    case 0x35: value = (Byte)(value * 10 + 5); break;
                    case 0x36: value = (Byte)(value * 10 + 6); break;
                    case 0x37: value = (Byte)(value * 10 + 7); break;
                    case 0x38: value = (Byte)(value * 10 + 8); break;
                    case 0x39: value = (Byte)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public static UInt16 ToUInt16(this Utf8String src)
        {
            UInt16 value = 0;
            var p = new Utf8Iterator(src.Bytes);
            while (p.MoveNext())
            {
                var b = p.Current;
                switch (b)
                {
                    case 0x30: value = (UInt16)(value * 10); break;
                    case 0x31: value = (UInt16)(value * 10 + 1); break;
                    case 0x32: value = (UInt16)(value * 10 + 2); break;
                    case 0x33: value = (UInt16)(value * 10 + 3); break;
                    case 0x34: value = (UInt16)(value * 10 + 4); break;
                    case 0x35: value = (UInt16)(value * 10 + 5); break;
                    case 0x36: value = (UInt16)(value * 10 + 6); break;
                    case 0x37: value = (UInt16)(value * 10 + 7); break;
                    case 0x38: value = (UInt16)(value * 10 + 8); break;
                    case 0x39: value = (UInt16)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public static UInt32 ToUInt32(this Utf8String src)
        {
            UInt32 value = 0;
            var p = new Utf8Iterator(src.Bytes);
            while (p.MoveNext())
            {
                var b = p.Current;
                switch (b)
                {
                    case 0x30: value = (UInt32)(value * 10); break;
                    case 0x31: value = (UInt32)(value * 10 + 1); break;
                    case 0x32: value = (UInt32)(value * 10 + 2); break;
                    case 0x33: value = (UInt32)(value * 10 + 3); break;
                    case 0x34: value = (UInt32)(value * 10 + 4); break;
                    case 0x35: value = (UInt32)(value * 10 + 5); break;
                    case 0x36: value = (UInt32)(value * 10 + 6); break;
                    case 0x37: value = (UInt32)(value * 10 + 7); break;
                    case 0x38: value = (UInt32)(value * 10 + 8); break;
                    case 0x39: value = (UInt32)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        public static UInt64 ToUInt64(this Utf8String src)
        {
            UInt64 value = 0;
            var p = new Utf8Iterator(src.Bytes);
            while (p.MoveNext())
            {
                var b = p.Current;
                switch (b)
                {
                    case 0x30: value = (UInt64)(value * 10); break;
                    case 0x31: value = (UInt64)(value * 10 + 1); break;
                    case 0x32: value = (UInt64)(value * 10 + 2); break;
                    case 0x33: value = (UInt64)(value * 10 + 3); break;
                    case 0x34: value = (UInt64)(value * 10 + 4); break;
                    case 0x35: value = (UInt64)(value * 10 + 5); break;
                    case 0x36: value = (UInt64)(value * 10 + 6); break;
                    case 0x37: value = (UInt64)(value * 10 + 7); break;
                    case 0x38: value = (UInt64)(value * 10 + 8); break;
                    case 0x39: value = (UInt64)(value * 10 + 9); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            return value;
        }
        #endregion

        public static float ToSingle(this Utf8String src)
        {
            return Single.Parse(src.ToAscii(), System.Globalization.CultureInfo.InvariantCulture);
        }
        public static double ToDouble(this Utf8String src)
        {
            return Double.Parse(src.ToAscii(), System.Globalization.CultureInfo.InvariantCulture);
        }

        public static Utf8String GetLine(this Utf8String src)
        {
            int pos;
            if (!src.TrySearchAscii((byte)'\n', 0, out pos))
            {
                return src;
            }

            return src.Subbytes(0, pos + 1);
        }
    }
}
