using System;


namespace UniJSON
{
    public class ByteBuffer
    {
        Byte[] m_buffer;
        public ArraySegment<Byte> Bytes
        {
            get { return new ArraySegment<byte>(m_buffer, 0, Count); }
        }

        public ByteBuffer()
        {

        }

        public ByteBuffer(Byte[] buffer)
        {
            m_buffer = buffer;
        }

        int m_used;
        public int Count
        {
            get { return m_used; }
        }

        public int Remain
        {
            get {
                if (m_buffer == null) return 0;
                return m_buffer.Length - m_used;
            }
        }

        void Ensure(int size)
        {
            if (m_buffer != null && size < m_buffer.Length - m_used)
            {
                return;
            }
            var buffer = new Byte[m_used + size];
            if (m_buffer != null && m_used > 0)
            {
                Buffer.BlockCopy(m_buffer, 0, buffer, 0, m_used);
            }
            m_buffer = buffer;
        }

        public void Push(Byte b)
        {
            Ensure(1);
            m_buffer[m_used++] = b;
        }

        public void Push(Byte[] buffer)
        {
            Push(new ArraySegment<Byte>(buffer));
        }

        public void Push(ArraySegment<Byte> buffer)
        {
            Ensure(buffer.Count);
            Buffer.BlockCopy(buffer.Array, buffer.Offset, m_buffer, m_used, buffer.Count);
            m_used += buffer.Count;
        }

        public void Unshift(int size)
        {
            if (size > m_used)
            {
                throw new ArgumentException();
            }

            if (m_used - size < size)
            {
                Buffer.BlockCopy(m_buffer, m_used, m_buffer, 0, m_used - size);
                m_used = m_used - size;
            }
            else
            {
                var buffer = new Byte[m_used];
                Buffer.BlockCopy(m_buffer, size, buffer, 0, m_used - size);
                m_buffer = buffer;
            }
        }
    }
}
