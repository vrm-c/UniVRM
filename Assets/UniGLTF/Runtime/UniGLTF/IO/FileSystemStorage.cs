using System;
using System.IO;

namespace UniGLTF
{
    /// <summary>
    /// Implement url that represnet relative path
    /// </summary>
    public class FileSystemStorage : IStorage
    {
        string m_root;

        public FileSystemStorage(string root)
        {
            m_root = Path.GetFullPath(root);
        }

        public ArraySegment<byte> Get(string url)
        {
            var bytes = File.ReadAllBytes(Path.Combine(m_root, url));
            return new ArraySegment<byte>(bytes);
        }
    }
}
