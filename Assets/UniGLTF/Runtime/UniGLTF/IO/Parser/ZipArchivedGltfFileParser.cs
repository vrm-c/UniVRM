using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniGLTF
{
    /// <summary>
    /// Zip archived .GLTF file & resources parser.
    /// </summary>
    public sealed class ZipArchivedGltfFileParser
    {
        private readonly string _zippedFilePath;

        public ZipArchivedGltfFileParser(string zippedFilePath)
        {
            if (!File.Exists(zippedFilePath))
            {
                throw new ArgumentException($"no file: {zippedFilePath}");
            }
            
            _zippedFilePath = zippedFilePath;
        }
        
        public GltfData Parse()
        {
            var binary = File.ReadAllBytes(_zippedFilePath);
            
            var zipArchive = Zip.ZipArchiveStorage.Parse(binary);
            var gltf = zipArchive.Entries.FirstOrDefault(x => x.FileName.ToLower().EndsWith(".gltf"));
            if (gltf == null)
            {
                throw new Exception("no gltf in archive");
            }
            var jsonBytes = zipArchive.Extract(gltf);
            var json = Encoding.UTF8.GetString(jsonBytes);
            return GlbLowLevelParser.ParseGltf(
                _zippedFilePath,
                json,
                new List<GlbChunk>(),
                zipArchive,
                new MigrationFlags()
            );
        }
    }
}