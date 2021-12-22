using System;
using System.Runtime.InteropServices;
using System.Text;


namespace UniGLTF
{
    public class BytesReader
    {
        Byte[] m_bytes;
        int m_pos;

        public BytesReader(Byte[] bytes, int pos = 0)
        {
            m_bytes = bytes;
            m_pos = pos;
        }

        public string ReadString(int count, Encoding encoding)
        {
            var s = encoding.GetString(m_bytes, m_pos, count);
            m_pos += count;
            return s;
        }

        public float ReadSingle()
        {
            var n = BitConverter.ToSingle(m_bytes, m_pos);
            m_pos += 4;
            return n;
        }

        public byte ReadUInt8()
        {
            return m_bytes[m_pos++];
        }

        public UInt16 ReadUInt16()
        {
            var n = BitConverter.ToUInt16(m_bytes, m_pos);
            m_pos += 2;
            return n;
        }

        public sbyte ReadInt8()
        {
            return (sbyte)m_bytes[m_pos++];
        }

        public Int16 ReadInt16()
        {
            var n = BitConverter.ToInt16(m_bytes, m_pos);
            m_pos += 2;
            return n;
        }

        public int ReadInt32()
        {
            var n = BitConverter.ToInt32(m_bytes, m_pos);
            m_pos += 4;
            return n;
        }

        public void ReadToArray<T>(T[] dst) where T : struct
        {
            var bytes = new ArraySegment<Byte>(m_bytes, m_pos, m_bytes.Length - m_pos);
            SafeMarshalCopy.CopyBytesToArray(bytes, dst);
            m_pos += bytes.Count;
        }

        public T ReadStruct<T>() where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            using (var pin = Pin.Create(new ArraySegment<Byte>(m_bytes, m_pos, m_bytes.Length - m_pos)))
            {
                var s = (T)Marshal.PtrToStructure(pin.Ptr, typeof(T));
                m_pos += size;
                return s;
            }
        }
    }
}
