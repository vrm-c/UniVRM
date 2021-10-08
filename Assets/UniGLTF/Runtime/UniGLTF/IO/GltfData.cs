using System;
using System.Collections.Generic;

namespace UniGLTF
{
    public sealed class GltfData
    {
        /// <summary>
        /// Source file path.
        /// Maybe empty if source file was on memory.
        /// </summary>
        public string TargetPath { get; }

        /// <summary>
        /// Chunk Data.
        /// Maybe empty if source file was not glb format.
        /// https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#chunks
        /// [0] must JSON
        /// [1] must BIN
        /// [2...] may exists.
        /// </summary>
        public IReadOnlyList<GlbChunk> Chunks { get; }

        /// <summary>
        /// JSON chunk ToString
        /// > This chunk MUST be the very first chunk of Binary glTF asset
        /// </summary>
        public string Json { get; }

        /// <summary>
        /// GLTF parsed from JSON chunk
        /// </summary>
        public glTF GLTF { get; }

        /// <summary>
        /// BIN chunk
        /// > This chunk MUST be the second chunk of the Binary glTF asset
        /// </summary>
        /// <returns></returns>
        public ArraySegment<byte> Bin => Chunks[1].Bytes;

        /// <summary>
        /// URI access
        /// </summary>
        public IStorage Storage { get; }

        /// <summary>
        /// Migration Flags used by ImporterContext
        /// </summary>
        public MigrationFlags MigrationFlags { get; }

        public GltfData(string targetPath, string json, glTF gltf, IReadOnlyList<GlbChunk> chunks, IStorage storage, MigrationFlags migrationFlags)
        {
            TargetPath = targetPath;
            Json = json;
            GLTF = gltf;
            Chunks = chunks;
            Storage = storage;
            MigrationFlags = migrationFlags;
        }

        public static GltfData CreateFromGltfDataForTest(glTF gltf)
        {
            return new GltfData(
                string.Empty,
                string.Empty,
                gltf,
                new List<GlbChunk>(),
                new SimpleStorage(new ArraySegment<byte>()),
                new MigrationFlags()
            );
        }
    }
}
