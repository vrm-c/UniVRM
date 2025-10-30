using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniGLTF
{
    /// <summary>
    /// .GLTF file with resources in same directory parser.
    /// </summary>
    public sealed class GltfFileWithResourceFilesParser
    {
        private readonly string _gltfFilePath;
        private readonly string _gltfRootPath;

        private readonly byte[] _bytes;

        public GltfFileWithResourceFilesParser(string gltfFilePath) : this(gltfFilePath, File.ReadAllBytes(gltfFilePath))
        {
        }

        public GltfFileWithResourceFilesParser(string gltfFilePath, byte[] bytes)
        {
            _gltfFilePath = gltfFilePath;
            _gltfRootPath = Path.GetDirectoryName(gltfFilePath);
            _bytes = bytes;
        }

        public GltfData Parse()
        {

            return GlbLowLevelParser.ParseGltf(
                _gltfFilePath,
                Encoding.UTF8.GetString(_bytes),
                new List<GlbChunk>(), // .gltf file has no chunks.
                new FileSystemStorage(_gltfRootPath), // .gltf file has resource path at file system.
                new MigrationFlags()
            );
        }
    }
}