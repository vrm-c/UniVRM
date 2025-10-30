using System;
using System.Linq;
using System.Text;


namespace UniJSON
{
    public static class JsonString
    {
        #region Quote
        public static void Escape(String s, IStore w)
        {
            if (String.IsNullOrEmpty(s))
            {
                return;
            }

            var it = s.ToCharArray().Cast<char>().GetEnumerator();
            while (it.MoveNext())
            {
                switch (it.Current)
                {
                    case '"':
                    case '\\':
                    case '/':
                        // \\ prefix
                        w.Write('\\');
                        w.Write(it.Current);
                        break;

                    case '\b':
                        w.Write('\\');
                        w.Write('b');
                        break;
                    case '\f':
                        w.Write('\\');
                        w.Write('f');
                        break;
                    case '\n':
                        w.Write('\\');
                        w.Write('n');
                        break;
                    case '\r':
                        w.Write('\\');
                        w.Write('r');
                        break;
                    case '\t':
                        w.Write('\\');
                        w.Write('t');
                        break;

                    default:
                        w.Write(it.Current);
                        break;
                }
            }
        }

        public static void Escape(Utf8String s, IStore w)
        {
            if (s.IsEmpty)
            {
                return;
            }

            var it = s.GetIterator();
            while (it.MoveNext())
            {
                var l = it.CurrentByteLength;
                if (l == 1)
                {
                    var b = it.Current;
                    switch (b)
                    {
                        case (Byte)'"':
                        case (Byte)'\\':
                        case (Byte)'/':
                            // \\ prefix
                            w.Write((Byte)'\\');
                            w.Write(b);
                            break;

                        case (Byte)'\b':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'b');
                            break;
                        case (Byte)'\f':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'f');
                            break;
                        case (Byte)'\n':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'n');
                            break;
                        case (Byte)'\r':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'r');
                            break;
                        case (Byte)'\t':
                            w.Write((Byte)'\\');
                            w.Write((Byte)'t');
                            break;

                        default:
                            w.Write(b);
                            break;
                    }
                    // ascii
                }
                else if (l == 2)
                {
                    w.Write(it.Current);
                    w.Write(it.Second);
                }
                else if (l == 3)
                {
                    w.Write(it.Current);
                    w.Write(it.Second);
                    w.Write(it.Third);
                }
                else if (l == 4)
                {
                    w.Write(it.Current);
                    w.Write(it.Second);
                    w.Write(it.Third);
                    w.Write(it.Fourth);
                }
                else
                {
                    throw new ParserException("invalid utf8");
                }
            }
        }

        public static string Escape(String s)
        {
            var sb = new StringBuilder();
            Escape(s, new StringBuilderStore(sb));
            return sb.ToString();
        }

        public static void Quote(String s, IStore w)
        {
            w.Write('"');
            Escape(s, w);
            w.Write('"');
        }

        public static void Quote(Utf8String s, IStore w)
        {
            w.Write((Byte)'"');
            Escape(s, w);
            w.Write((Byte)'"');
        }

        /// <summary>
        /// Added " and Escape
        /// </summary>
        /// <param name="s"></param>
        /// <param name="w"></param>
        public static string Quote(string s)
        {
            var sb = new StringBuilder();
            Quote(s, new StringBuilderStore(sb));
            return sb.ToString();
        }

        public static Utf8String Quote(Utf8String s)
        {
            var sb = new BytesStore(s.ByteLength);
            Quote(s, sb);
            return new Utf8String(sb.Bytes);
        }
        #endregion

        #region Unquote
        static byte CheckHex(int b)
        {
            switch ((char)b)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A':
                case 'a':
                    return 10;
                case 'B':
                case 'b':
                    return 11;
                case 'C':
                case 'c':
                    return 12;
                case 'D':
                case 'd':
                    return 13;
                case 'E':
                case 'e':
                    return 14;
                case 'F':
                case 'f':
                    return 15;
            }

            throw new ArgumentOutOfRangeException();
        }

        public static int Unescape(string src, IStore w)
        {
            int writeCount = 0;
            Action<Char> Write = c =>
            {
                if (w != null)
                {
                    w.Write(c);
                }
                ++writeCount;
            };

            int i = 0;
            int length = src.Length - 1;
            while (i < length)
            {
                if (src[i] == '\\')
                {
                    var c = src[i + 1];
                    switch (c)
                    {
                        case '\\':
                        case '/':
                        case '"':
                            // remove prefix
                            Write(c);
                            i += 2;
                            continue;

                        case 'b':
                            Write('\b');
                            i += 2;
                            continue;
                        case 'f':
                            Write('\f');
                            i += 2;
                            continue;
                        case 'n':
                            Write('\n');
                            i += 2;
                            continue;
                        case 'r':
                            Write('\r');
                            i += 2;
                            continue;
                        case 't':
                            Write('\t');
                            i += 2;
                            continue;
                        case 'u':
                            {
                                var u0 = CheckHex(src[i + 2]);
                                var u1 = CheckHex(src[i + 3]);
                                var u2 = CheckHex(src[i + 4]);
                                var u3 = CheckHex(src[i + 5]);
                                var u = (u0 << 12) + (u1 << 8) + (u2 << 4) + u3;
                                Write((char)u);
                                i += 6;
                            }
                            continue;
                    }
                }

                Write(src[i]);
                i += 1;
            }
            while (i <= length)
            {
                Write(src[i++]);
            }

            return writeCount;
        }

        public static int Unescape(Utf8String s, IStore w)
        {
            int writeCount = 0;
            Action<Byte> Write = c =>
            {
                if (w != null)
                {
                    w.Write(c);
                }
                ++writeCount;
            };

            var it = s.GetIterator();
            while (it.MoveNext())
            {
                var l = it.CurrentByteLength;
                if (l == 1)
                {
                    if (it.Current == (Byte)'\\')
                    {
                        var c = it.Second;
                        switch (c)
                        {
                            case (Byte)'\\':
                            case (Byte)'/':
                            case (Byte)'"':
                                // remove prefix
                                Write(c);
                                it.MoveNext();
                                continue;

                            case (Byte)'b':
                                Write((Byte)'\b');
                                it.MoveNext();
                                continue;
                            case (Byte)'f':
                                Write((Byte)'\f');
                                it.MoveNext();
                                continue;
                            case (Byte)'n':
                                Write((Byte)'\n');
                                it.MoveNext();
                                continue;
                            case (Byte)'r':
                                Write((Byte)'\r');
                                it.MoveNext();
                                continue;
                            case (Byte)'t':
                                Write((Byte)'\t');
                                it.MoveNext();
                                continue;
                            case (Byte)'u':
                                {
                                    // skip back slash
                                    it.MoveNext();
                                    // skip u
                                    it.MoveNext();

                                    var u0 = CheckHex((char)it.Current);
                                    it.MoveNext();

                                    var u1 = CheckHex((char)it.Current);
                                    it.MoveNext();

                                    var u2 = CheckHex((char)it.Current);
                                    it.MoveNext();

                                    var u3 = CheckHex((char)it.Current);

                                    var u = (u0 << 12) + (u1 << 8) + (u2 << 4) + u3;
                                    var utf8 = Utf8String.From(new string(new char[] { (char)u }));
                                    // var utf8 = Utf8String.From((int)u);
                                    foreach (var x in utf8.Bytes)
                                    {
                                        Write(x);
                                    }
                                }
                                continue;
                        }
                    }

                    Write(it.Current);
                }
                else if (l == 2)
                {
                    Write(it.Current);
                    Write(it.Second);
                }
                else if (l == 3)
                {
                    Write(it.Current);
                    Write(it.Second);
                    Write(it.Third);
                }
                else if (l == 4)
                {
                    Write(it.Current);
                    Write(it.Second);
                    Write(it.Third);
                    Write(it.Fourth);
                }
                else
                {
                    throw new ParserException("invalid utf8");
                }
            }

            return writeCount;
        }

        public static string Unescape(string src)
        {
            var sb = new StringBuilder();
            Unescape(src, new StringBuilderStore(sb));
            return sb.ToString();
        }

        public static int Unquote(string src, IStore w)
        {
            return Unescape(src.Substring(1, src.Length - 2), w);
        }

        public static int Unquote(Utf8String src, IStore w)
        {
            return Unescape(src.Subbytes(1, src.ByteLength - 2), w);
        }

        public static string Unquote(string src)
        {
            var count = Unquote(src, null);
            if (count == src.Length - 2)
            {
                return src.Substring(1, src.Length - 2);
            }
            else
            {
                var sb = new StringBuilder(count);
                Unquote(src, new StringBuilderStore(sb));
                var str = sb.ToString();
                return str;
            }
        }

        public static Utf8String Unquote(Utf8String src)
        {
            var count = Unquote(src, null);
            if (count == src.ByteLength - 2)
            {
                return src.Subbytes(1, src.ByteLength - 2);
            }
            else
            {
                var sb = new BytesStore(count);
                Unquote(src, sb);
                return new Utf8String(sb.Bytes);
            }
        }
        #endregion
    }
}
