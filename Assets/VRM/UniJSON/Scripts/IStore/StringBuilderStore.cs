using System;
using System.Collections.Generic;
using System.Text;


namespace UniJSON
{
    public class StringBuilderStore: IStore
    {
        StringBuilder m_sb;

        public StringBuilderStore(StringBuilder sb)
        {
            m_sb = sb;
        }

        public ArraySegment<Byte> Bytes
        {
            get
            {
                return new ArraySegment<Byte>(
                    Encoding.UTF8.GetBytes(Buffer())
                    );
            }
        }

        public string Buffer()
        {
            return m_sb.ToString();
        }

        public void Clear()
        {
            m_sb.Length = 0;
        }

        public void Write(ArraySegment<byte> bytes)
        {
            var text = Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
            Write(text);
        }

        public void Write(byte value)
        {
            throw new NotImplementedException();
        }

        public void Write(sbyte value)
        {
            throw new NotImplementedException();
        }

        public void Write(IEnumerable<char> src)
        {
            foreach(var c in src)
            {
                m_sb.Append(c);
            }
        }
        public void Write(Char c)
        {
            m_sb.Append(c);
        }
        public void Write(string src)
        {
            m_sb.Append(src);
        }

        #region BigEndian
        public void WriteBigEndian(int value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(float value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(double value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(long value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(ulong value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(short value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(ushort value)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region LittleEndian
        public void WriteLittleEndian(double value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(short value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(int value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(float value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(long value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(ulong value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteLittleEndian(ushort value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
