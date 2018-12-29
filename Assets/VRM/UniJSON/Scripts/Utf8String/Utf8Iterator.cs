using System;
using System.Collections;
using System.Collections.Generic;


namespace UniJSON
{
    public struct Utf8Iterator : IEnumerator<Byte>
    {
        Byte[] m_bytes;
        int m_offset;
        int m_start;
        int m_position;
        int m_end;

        public Utf8Iterator(ArraySegment<Byte> range, int start = 0)
        {
            m_bytes = range.Array;
            m_offset = range.Offset;
            m_start = m_offset + start;
            m_position = -1;
            m_end = range.Offset + range.Count;
        }

        public int BytePosition
        {
            get { return m_position - m_offset; }
        }

        public int CurrentByteLength
        {
            get
            {
                var firstByte = Current;
                if (firstByte <= 0x7F)
                {
                    return 1;
                }
                else if (firstByte <= 0xDF)
                {
                    return 2;
                }
                else if (firstByte <= 0xEF)
                {
                    return 3;
                }
                else if (firstByte <= 0xF7)
                {
                    return 4;
                }
                else
                {
                    throw new Exception("invalid utf8");
                }
            }
        }

        public byte Current
        {
            get { return m_bytes[m_position]; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public byte Second
        {
            get { return m_bytes[m_position + 1]; }
        }

        public byte Third
        {
            get { return m_bytes[m_position + 2]; }
        }

        public byte Fourth
        {
            get { return m_bytes[m_position + 3]; }
        }

        public const uint Mask1 = 0x01;
        public const uint Mask2 = 0x03;
        public const uint Mask3 = 0x07;
        public const uint Mask4 = 0x0F;
        public const uint Mask5 = 0x1F;
        public const uint Mask6 = 0x3F;
        public const uint Mask7 = 0x7F;
        public const uint Mask11 = 0x07FF;

        public const uint Head1 = 0x80;
        public const uint Head2 = 0xC0;
        public const uint Head3 = 0xE0;
        public const uint Head4 = 0xF0;

        public static int ByteLengthFromChar(char c)
        {
            if (c <= Mask7)
            {
                return 1;
            }
            else if (c <= Mask11)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        public uint Unicode
        {
            get
            {
                var l = CurrentByteLength;
                if (l == 1)
                {
                    // 7bit
                    return Current;
                }
                else if (l == 2)
                {
                    // 11bit
                    return (Mask5 & Current) << 6 | (Mask6 & Second);
                }
                else if (l == 3)
                {
                    // 16bit
                    return (Mask4 & Current) << 12 | (Mask6 & Second) << 6 | (Mask6 & Third);
                }
                else if (l == 4)
                {
                    // 21bit
                    return (Mask3 & Current) << 18 | (Mask6 & Second) << 12 | (Mask6 & Third) << 6 | (Mask6 & Fourth);
                }
                else
                {
                    throw new Exception("invalid utf8");
                }
            }
        }

        public char Char
        {
            get
            {
                var l = CurrentByteLength;
                if (l == 1)
                {
                    // 7bit
                    return (char)Current;
                }
                else if (l == 2)
                {
                    // 11bit
                    return (char)((Mask5 & Current) << 6 | (Mask6 & Second));
                }
                else if (l == 3)
                {
                    // 16bit
                    return (char)((Mask4 & Current) << 12 | (Mask6 & Second) << 6 | (Mask6 & Third));
                }
                else if (l == 4)
                {
                    // 21bit
                    throw new NotImplementedException();
                }
                else
                {
                    throw new Exception("invalid utf8");
                }
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (m_position == -1)
            {
                m_position = m_start;
            }
            else
            {
                m_position += CurrentByteLength;
            }
            return m_position < m_end;
        }

        public void Reset()
        {
            m_position = -1;
        }
    }
}
