using System;
using System.IO;
using System.Runtime.InteropServices;


namespace UniGLTF
{
    public interface IBytesBuffer
    {
        string Uri { get; }
        ArraySegment<Byte> GetBytes();
        glTFBufferView Extend<T>(ArraySegment<T> array, glBufferTarget target) where T : struct;
    }

    public static class IBytesBufferExtensions
    {
        public static glTFBufferView Extend<T>(this IBytesBuffer buffer, T[] array, glBufferTarget target) where T : struct
        {
            return buffer.Extend(new ArraySegment<T>(array), target);
        }
    }

    /// <summary>
    /// for buffer with uri read
    /// </summary>
    public class UriByteBuffer : IBytesBuffer
    {
        public string Uri
        {
            get;
            private set;
        }

        Byte[] m_bytes;
        public ArraySegment<byte> GetBytes()
        {
            return new ArraySegment<byte>(m_bytes);
        }

        public UriByteBuffer(string baseDir, string uri)
        {
            Uri = uri;
            m_bytes = ReadFromUri(baseDir, uri);
        }

        const string DataPrefix = "data:application/octet-stream;base64,";

        const string DataPrefix2 = "data:application/gltf-buffer;base64,";

        const string DataPrefix3 = "data:image/jpeg;base64,";

        public static Byte[] ReadEmbeded(string uri)
        {
            var pos = uri.IndexOf(";base64,");
            if (pos < 0)
            {
                throw new NotImplementedException();
            }
            else
            {
                return Convert.FromBase64String(uri.Substring(pos + 8));
            }
        }

        Byte[] ReadFromUri(string baseDir, string uri)
        {
            var bytes = ReadEmbeded(uri);
            if (bytes != null)
            {
                return bytes;
            }
            else
            {
                // as local file path
                return File.ReadAllBytes(Path.Combine(baseDir, uri));
            }
        }

        public glTFBufferView Extend<T>(ArraySegment<T> array, glBufferTarget target) where T : struct
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// for glb chunk buffer read
    /// </summary>
    public class ArraySegmentByteBuffer : IBytesBuffer
    {
        ArraySegment<Byte> m_bytes;

        public ArraySegmentByteBuffer(ArraySegment<Byte> bytes)
        {
            m_bytes = bytes;
        }

        public string Uri
        {
            get;
            private set;
        }

        public glTFBufferView Extend<T>(ArraySegment<T> array, glBufferTarget target) where T : struct
        {
            throw new NotImplementedException();
        }

        public ArraySegment<byte> GetBytes()
        {
            return m_bytes;
        }
    }

    /// <summary>
    /// for exporter
    /// </summary>
    public class ArrayByteBuffer : IBytesBuffer
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

        public glTFBufferView Extend<T>(ArraySegment<T> array, glBufferTarget target) where T : struct
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
            var result=new glTFBufferView
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

        public ArraySegment<byte> GetBytes()
        {
            if (m_bytes == null)
            {
                return new ArraySegment<byte>();
            }

            return new ArraySegment<byte>(m_bytes, 0, m_used);
        }
    }
}
