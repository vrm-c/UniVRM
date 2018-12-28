using System;
using System.IO;
using System.Text;

namespace UniJSON
{
    public class StreamStore: IStore
    {
        Stream m_s;
        BinaryWriter m_w;

        public StreamStore(Stream s)
        {
            m_s = s;
            m_w = new BinaryWriter(m_s);
        }

        public ArraySegment<byte> Bytes
        {
            get
            {
#if NETFX_CORE
                throw new NotImplementedException();
#else
                var ms = m_s as MemoryStream;
                if (ms == null)
                {
                    throw new NotImplementedException();
                }
                return new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Position);
#endif
            }
        }

        public void Clear()
        {
            m_s.SetLength(0);
        }

        public void Write(sbyte value)
        {
            m_w.Write(value);
        }

        public void Write(byte value)
        {
            m_w.Write(value);
        }

        public void Write(char c)
        {
            m_w.Write(c);
        }

        public void Write(string src)
        {
            m_w.Write(Encoding.UTF8.GetBytes(src));
        }

        public void Write(ArraySegment<byte> bytes)
        {
            m_w.Write(bytes.Array, bytes.Offset, bytes.Count);
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

        public void WriteBigEndian(short value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(ulong value)
        {
            throw new NotImplementedException();
        }

        public void WriteBigEndian(ushort value)
        {
            throw new NotImplementedException();
        }
#endregion

#region LittleEndian
        public void WriteLittleEndian(long value)
        {
            m_w.Write(value);
        }

        public void WriteLittleEndian(uint value)
        {
            m_w.Write(value);
        }

        public void WriteLittleEndian(short value)
        {
            m_w.Write(value);
        }

        public void WriteLittleEndian(ulong value)
        {
            m_w.Write(value);
        }

        public void WriteLittleEndian(double value)
        {
            m_w.Write(value);
        }

        public void WriteLittleEndian(float value)
        {
            m_w.Write(value);
        }

        public void WriteLittleEndian(int value)
        {
            m_w.Write(value);
        }

        public void WriteLittleEndian(ushort value)
        {
            m_w.Write(value);
        }
#endregion
    }
}
