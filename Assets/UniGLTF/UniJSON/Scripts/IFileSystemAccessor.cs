using System.IO;
using System.Text;


namespace UniJSON
{
    public interface IFileSystemAccessor
    {
        string ReadAllText();
        string ReadAllText(string relativePath);
        IFileSystemAccessor Get(string relativePath);
    }

    public class FileSystemAccessor : IFileSystemAccessor
    {
        string m_path;
        string m_baseDir;
        public FileSystemAccessor(string path)
        {
            m_path = path;
            if (Directory.Exists(path))
            {
                m_baseDir = path;
            }
            else
            {
                m_baseDir = Path.GetDirectoryName(path);
            }
        }

        public override string ToString()
        {
            return "<" + Path.GetFileName(m_path) + ">";
        }

        public string ReadAllText()
        {
            return File.ReadAllText(m_path, Encoding.UTF8);
        }

        public string ReadAllText(string relativePath)
        {
            var path = Path.Combine(m_baseDir, relativePath);
            return File.ReadAllText(path, Encoding.UTF8);
        }

        public IFileSystemAccessor Get(string relativePath)
        {
            var path = Path.Combine(m_baseDir, relativePath);
            return new FileSystemAccessor(path);
        }
    }
}
