using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// Low-level API.
    /// Parse from specified path & specified binary.
    /// </summary>
    public sealed class GlbLowLevelParser
    {
        private readonly string _path;
        private readonly byte[] _binary;

        public GlbLowLevelParser(string path, byte[] specifiedBinary)
        {
            _path = path;
            _binary = specifiedBinary;
        }

        public GltfData Parse()
        {
            try
            {
                var chunks = ParseGlbChunks(_binary);
                var jsonBytes = chunks[0].Bytes;
                return ParseGltf(
                    _path,
                    Encoding.UTF8.GetString(jsonBytes.Array, jsonBytes.Offset, jsonBytes.Count),
                    chunks,
                    default,
                    new MigrationFlags()
                );
            }
            catch (StackOverflowException ex)
            {
                throw new Exception("[UniVRM Import Error] json parsing failed, nesting is too deep.\n" + ex);
            }
            catch
            {
                throw;
            }
        }

        public static List<GlbChunk> ParseGlbChunks(byte[] data)
        {
            var chunks = glbImporter.ParseGlbChunks(data);

            if (chunks.Count < 2)
            {
                throw new Exception("unknown chunk count: " + chunks.Count);
            }

            if (chunks[0].ChunkType != GlbChunkType.JSON)
            {
                throw new Exception("chunk 0 is not JSON");
            }

            if (chunks[1].ChunkType != GlbChunkType.BIN)
            {
                throw new Exception("chunk 1 is not BIN");
            }

            return chunks;
        }

        public static GltfData ParseGltf(string path, string json, IReadOnlyList<GlbChunk> chunks, IStorage storage, MigrationFlags migrationFlags)
        {
            var GLTF = GltfDeserializer.Deserialize(json.ParseAsJson());
            if (GLTF.asset.version != "2.0")
            {
                throw new UniGLTFException("unknown gltf version {0}", GLTF.asset.version);
            }

            return new GltfData(path, json, GLTF, chunks, storage, migrationFlags);
        }


        public static void AppendImageExtension(glTFImage texture, string extension)
        {
            if (!texture.name.EndsWith(extension))
            {
                texture.name = texture.name + extension;
            }
        }
    }
}