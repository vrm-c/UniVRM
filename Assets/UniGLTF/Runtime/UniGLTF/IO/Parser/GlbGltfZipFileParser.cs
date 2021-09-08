using System.IO;

namespace UniGLTF
{
    /// <summary>
    /// Parse file and detect file type by file extension.
    /// </summary>
    public sealed class GltfZipOrGlbFileParser
    {
        private readonly string _path;

        public GltfZipOrGlbFileParser(string glbFilePath)
        {
            _path = glbFilePath;
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
                    // or glb
                    return new GlbFileParser(_path).Parse();
            }
        }
    }
}
