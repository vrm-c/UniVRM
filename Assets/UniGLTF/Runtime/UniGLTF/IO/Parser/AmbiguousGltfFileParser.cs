using System.IO;

namespace UniGLTF
{
    /// <summary>
    /// Ambiguous file parser.
    /// Determine parsing method from the file extension.
    /// </summary>
    public sealed class AmbiguousGltfFileParser
    {
        private readonly string _path;
        
        public AmbiguousGltfFileParser(string path)
        {
            _path = path;
        }
        
        public GltfData Parse()
        {
            var ext = Path.GetExtension(_path).ToLower();
            switch (ext)
            {
                case ".gltf":
                    return new GltfFileWithResourceFilesParser(_path).Parse();
                
                case ".zip":
                    return new ZipArchivedGltfFileParser(_path).Parse();

                default:
                    return new GlbFileParser(_path).Parse();
            }
        }
    }
}