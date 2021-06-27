using System;
using System.Collections.Generic;

namespace UniGLTF
{
    /// <summary>
    /// For unit tests.
    /// JSON string with storage parser.
    /// </summary>
    public sealed class JsonWithStorageParser
    {
        private readonly string _json;
        private readonly IStorage _storage;
        
        public JsonWithStorageParser(string json, IStorage storage = null)
        {
            _json = json;
            _storage = storage ?? new SimpleStorage(new ArraySegment<byte>());
        }

        public GltfData Parse()
        {
            return GlbLowLevelParser.ParseGltf(
                string.Empty,
                _json,
                new List<GlbChunk>(),
                _storage,
                new MigrationFlags()
            );
        }
    }
}