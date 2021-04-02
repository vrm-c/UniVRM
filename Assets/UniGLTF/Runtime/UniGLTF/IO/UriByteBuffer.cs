using System;
using System.IO;

namespace UniGLTF
{
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
        public ArraySegment<byte> Bytes => new ArraySegment<byte>(m_bytes);

        public UriByteBuffer(string baseDir, string uri)
        {
            Uri = uri;
            m_bytes = ReadFromUri(baseDir, uri);
        }

        const string DataPrefix = "data:application/octet-stream;base64,";

        const string DataPrefix2 = "data:application/gltf-buffer;base64,";

        const string DataPrefix3 = "data:image/jpeg;base64,";

        [Obsolete("Use ReadEmbedded(uri)")]
        public static Byte[] ReadEmbeded(string uri)
        {
            return ReadEmbedded(uri);
        }

        public static Byte[] ReadEmbedded(string uri)
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
            var bytes = ReadEmbedded(uri);
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

        public void ExtendCapacity(int capacity)
        {
            throw new NotImplementedException();
        }
    }
}
