using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

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
        /// JSON source
        /// </summary>
        public string Json { get; }

        /// <summary>
        /// GLTF parsed from JSON
        /// </summary>
        public glTF GLTF { get; }

        /// <summary>
        /// Chunk Data.
        /// Maybe empty if source file was not glb format.
        /// </summary>
        public IReadOnlyList<GlbChunk> Chunks { get; }

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
