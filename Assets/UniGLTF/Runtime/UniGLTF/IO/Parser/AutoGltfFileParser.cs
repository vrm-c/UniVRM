using System.IO;

namespace UniGLTF
{
    /// <summary>
    /// Auto detection file parser.
    /// Determine parsing method from the file extension.
    /// Detects `gltf`` zip`, others as` glb`
    /// </summary>
    public sealed class AutoGltfFileParser
    {
        private readonly string _path;
        
        public AutoGltfFileParser(string path)
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