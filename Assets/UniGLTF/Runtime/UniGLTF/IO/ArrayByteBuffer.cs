using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UniGLTF
{
    /// <summary>
    /// for exporter
    /// </summary>
    public class ArrayByteBuffer
    {
        public string Uri
        {
            get;
            private set;
        }

        Byte[] m_bytes;
        int m_used;

        public ArrayByteBuffer(Byte[] bytes = null)
        {
            Uri = "";
            m_bytes = bytes;
        }

        public glTFBufferView Extend<T>(NativeArray<T> array, glBufferTarget target = default) where T : struct
        {
            return Extend(new ArraySegment<T>(array.ToArray()), target);
        }

        public glTFBufferView Extend<T>(ArraySegment<T> array, glBufferTarget target = default) where T : struct
        {
            using (var pin = Pin.Create(array))
            {
                var elementSize = Marshal.SizeOf(typeof(T));
                var view = Extend(pin.Ptr, array.Count * elementSize, elementSize, target);
                return view;
            }
        }

        public glTFBufferView Extend(IntPtr p, int bytesLength, int stride, glBufferTarget target)
        {
            var tmp = m_bytes;
            // alignment
            var padding = m_used % stride == 0 ? 0 : stride - m_used % stride;

            if (m_bytes == null || m_used + padding + bytesLength > m_bytes.Length)
            {
                // recreate buffer
                m_bytes = new Byte[m_used + padding + bytesLength];
                if (m_used > 0)
                {
                    Buffer.BlockCopy(tmp, 0, m_bytes, 0, m_used);
                }
            }
            if (m_used + padding + bytesLength > m_bytes.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            Marshal.Copy(p, m_bytes, m_used + padding, bytesLength);
            var result = new glTFBufferView
            {
                buffer = 0,
                byteLength = bytesLength,
                byteOffset = m_used + padding,
                byteStride = stride,
                target = target,
            };
            m_used = m_used + padding + bytesLength;
            return result;
        }

        public void ExtendCapacity(int capacity)
        {
            if (m_bytes != null && capacity < m_bytes.Length)
            {
                return;
            }

            var newBuffer = new byte[capacity];
            if (m_bytes != null && m_used > 0)
            {
                Buffer.BlockCopy(m_bytes, 0, newBuffer, 0, m_used);
            }
            m_bytes = newBuffer;
        }

        public ArraySegment<byte> Bytes
        {
            get
            {
                if (m_bytes == null)
                {
                    return new ArraySegment<byte>();
                }

                return new ArraySegment<byte>(m_bytes, 0, m_used);
            }
        }
    }
}
