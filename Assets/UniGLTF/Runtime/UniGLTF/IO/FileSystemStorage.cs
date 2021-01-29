using System;
using System.IO;

namespace UniGLTF
{
    public class SimpleStorage : IStorage
    {
        ArraySegment<Byte> m_bytes;

        public SimpleStorage() : this(new ArraySegment<byte>())
        {
        }

        public SimpleStorage(ArraySegment<Byte> bytes)
        {
            m_bytes = bytes;
        }

        public ArraySegment<byte> Get(string url)
        {
            return m_bytes;
        }

        public string GetPath(string url)
        {
            return null;
        }
    }

    public class FileSystemStorage : IStorage
    {
        string m_root;

        public FileSystemStorage(string root)
        {
            m_root = Path.GetFullPath(root);
        }

        public ArraySegment<byte> Get(string url)
        {
            var bytes =
                (url.FastStartsWith("data:"))
                ? UriByteBuffer.ReadEmbedded(url)
                : File.ReadAllBytes(Path.Combine(m_root, url))
                ;
            return new ArraySegment<byte>(bytes);
        }

        public string GetPath(string url)
        {
            if (url.FastStartsWith("data:"))
            {
                return null;
            }
            else
            {
                return Path.Combine(m_root, url).Replace("\\", "/");
            }
        }
    }
}
