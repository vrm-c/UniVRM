using System.IO;

namespace UniGLTF
{
    /// <summary>
    /// .GLB file parser.
    /// </summary>
    public sealed class GlbFileParser
    {
        private readonly string _path;

        public GlbFileParser(string glbFilePath)
        {
            _path = glbFilePath;
        }

        public GltfData Parse()
        {
            var data = File.ReadAllBytes(_path);
            return new GlbLowLevelParser(_path, data).Parse();
        }
    }
}