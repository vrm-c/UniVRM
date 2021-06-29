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
        
        public GltfFileWithResourceFilesParser(string gltfFilePath)
        {
            if (!File.Exists(gltfFilePath))
            {
                throw new ArgumentException($"no file: {gltfFilePath}");
            }
            
            _gltfFilePath = gltfFilePath;
            _gltfRootPath = Path.GetDirectoryName(gltfFilePath);
        }

        public GltfData Parse()
        {
            var binary = File.ReadAllBytes(_gltfFilePath);
            
            return GlbLowLevelParser.ParseGltf(
                _gltfFilePath,
                Encoding.UTF8.GetString(binary),
                new List<GlbChunk>(), // .gltf file has no chunks.
                new FileSystemStorage(_gltfRootPath), // .gltf file has resource path at file system.
                new MigrationFlags()
            );
        }
    }
}