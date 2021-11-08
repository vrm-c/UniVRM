using System;
using System.Collections.Generic;
using System.Linq;

namespace UniGLTF
{
    /// <summary>
    /// * JSON is parsed but not validated as glTF
    /// * For glb, bin chunks are already available
    /// </summary>
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
        public ArraySegment<byte> Bin
        {
            get
            {
                if (Chunks == null)
                {
                    return default;
                }
                if (Chunks.Count < 2)
                {
                    return default;
                }
                return Chunks[1].Bytes;
            }
        }

        /// <summary>
        /// Migration Flags used by ImporterContext
        /// </summary>
        public MigrationFlags MigrationFlags { get; }

        /// <summary>
        /// URI access
        /// </summary>
        IStorage _storage;

        /// <summary>
        /// uri = data: base64デコード
        /// uri = 相対パス。File.ReadAllBytes
        /// </summary>
        /// <returns></returns>
        Dictionary<string, ArraySegment<byte>> _UriCache = new Dictionary<string, ArraySegment<byte>>();

        public GltfData(string targetPath, string json, glTF gltf, IReadOnlyList<GlbChunk> chunks, IStorage storage, MigrationFlags migrationFlags)
        {
            TargetPath = targetPath;
            Json = json;
            GLTF = gltf;
            Chunks = chunks;
            _storage = storage;
            MigrationFlags = migrationFlags;
        }

        public static GltfData CreateFromExportForTest(ExportingGltfData data)
        {
            return CreateFromGltfDataForTest(data.GLTF, data.BinBytes);
        }

        public static GltfData CreateFromGltfDataForTest(glTF gltf, ArraySegment<byte> bytes)
        {
            return new GltfData(
                string.Empty,
                string.Empty,
                gltf,
                new List<GlbChunk>
                {
                    new GlbChunk(), // json
                    GlbChunk.CreateBin(bytes),
                },
                default,
                new MigrationFlags()
            );
        }

        ArraySegment<Byte> GetBytesFromUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException();
            }

            if (_UriCache.TryGetValue(uri, out ArraySegment<byte> data))
            {
                // return cache
                return data;
            }

            if (uri.StartsWith("data:", StringComparison.Ordinal))
            {
                data = new ArraySegment<byte>(UriByteBuffer.ReadEmbedded(uri));
            }
            else
            {
                data = _storage.Get(uri);
            }
            _UriCache.Add(uri, data);
            return data;
        }

        public ArraySegment<Byte> GetBytesFromBuffer(int bufferIndex)
        {
            var buffer = GLTF.buffers[bufferIndex];
            if (bufferIndex == 0 && Bin.Array != null)
            {
                // https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#glb-stored-buffer
                return Bin;
            }
            else
            {
                return GetBytesFromUri(buffer.uri);
            }
        }

        public ArraySegment<Byte> GetBytesFromBufferView(int bufferView)
        {
            var view = GLTF.bufferViews[bufferView];
            var segment = GetBytesFromBuffer(view.buffer);
            return new ArraySegment<byte>(segment.Array, segment.Offset + view.byteOffset, view.byteLength);
        }

        T[] GetTypedFromBufferView<T>(int count, int byteOffset, glTFBufferView view) where T : struct
        {
            var segment = GetBytesFromBuffer(view.buffer);
            var attrib = new T[count];
            var bytes = new ArraySegment<Byte>(segment.Array, segment.Offset + view.byteOffset + byteOffset, count * view.byteStride);
            bytes.MarshalCopyTo(attrib);
            return attrib;
        }

        T[] GetTypedFromAccessor<T>(glTFAccessor accessor, glTFBufferView view) where T : struct
        {
            return GetTypedFromBufferView<T>(accessor.count, accessor.byteOffset, view);
        }

        /// <summary>
        /// for sparse
        /// </summary>
        /// <param name="view"></param>
        /// <param name="count"></param>
        /// <param name="byteOffset"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
        IEnumerable<int> GetIntIndicesFromView(glTFBufferView view, int count, int byteOffset, glComponentType componentType)
        {
            switch (componentType)
            {
                case glComponentType.UNSIGNED_BYTE:
                    {
                        return GetTypedFromBufferView<Byte>(count, byteOffset, view).Select(x => (int)(x));
                    }

                case glComponentType.UNSIGNED_SHORT:
                    {
                        return GetTypedFromBufferView<UInt16>(count, byteOffset, view).Select(x => (int)(x));
                    }

                case glComponentType.UNSIGNED_INT:
                    {
                        return GetTypedFromBufferView<UInt32>(count, byteOffset, view).Select(x => (int)(x));
                    }
            }
            throw new NotImplementedException("GetIndices: unknown componenttype: " + componentType);
        }

        /// <summary>
        /// Get indices and cast to int
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<int> GetIntIndicesFromAccessor(glTFAccessor accessor, out int count)
        {
            count = accessor.count;
            var view = GLTF.bufferViews[accessor.bufferView];
            switch ((glComponentType)accessor.componentType)
            {
                case glComponentType.UNSIGNED_BYTE:
                    {
                        return GetTypedFromAccessor<Byte>(accessor, view).Select(x => (int)(x));
                    }

                case glComponentType.UNSIGNED_SHORT:
                    {
                        return GetTypedFromAccessor<UInt16>(accessor, view).Select(x => (int)(x));
                    }

                case glComponentType.UNSIGNED_INT:
                    {
                        return GetTypedFromAccessor<UInt32>(accessor, view).Select(x => (int)(x));
                    }
            }
            throw new NotImplementedException("GetIndices: unknown componenttype: " + accessor.componentType);
        }

        public int[] GetIndices(int accessorIndex)
        {
            int count;
            var result = GetIntIndicesFromAccessor(GLTF.accessors[accessorIndex], out count);
            var indices = new int[count];

            // flip triangles
            var it = result.GetEnumerator();
            {
                for (int i = 0; i < count; i += 3)
                {
                    it.MoveNext(); indices[i + 2] = it.Current;
                    it.MoveNext(); indices[i + 1] = it.Current;
                    it.MoveNext(); indices[i] = it.Current;
                }
            }

            return indices;
        }

        public T[] GetArrayFromAccessor<T>(int accessorIndex) where T : struct
        {
            var vertexAccessor = GLTF.accessors[accessorIndex];

            if (vertexAccessor.count <= 0) return new T[] { };

            var result = (vertexAccessor.bufferView != -1)
                ? GetTypedFromAccessor<T>(vertexAccessor, GLTF.bufferViews[vertexAccessor.bufferView])
                : new T[vertexAccessor.count]
                ;

            var sparse = vertexAccessor.sparse;
            if (sparse != null && sparse.count > 0)
            {
                // override sparse values
                var indices = GetIntIndicesFromView(GLTF.bufferViews[sparse.indices.bufferView], sparse.count, sparse.indices.byteOffset, sparse.indices.componentType);
                var values = GetTypedFromBufferView<T>(sparse.count, sparse.values.byteOffset, GLTF.bufferViews[sparse.values.bufferView]);

                var it = indices.GetEnumerator();
                for (int i = 0; i < sparse.count; ++i)
                {
                    it.MoveNext();
                    result[it.Current] = values[i];
                }
            }
            return result;
        }

        public float[] FlatternFloatArrayFromAccessor(int accessorIndex)
        {
            var vertexAccessor = GLTF.accessors[accessorIndex];

            if (vertexAccessor.count <= 0) return new float[] { };

            var bufferCount = vertexAccessor.count * vertexAccessor.TypeCount;

            float[] result = null;
            if (vertexAccessor.bufferView != -1)
            {
                var attrib = new float[vertexAccessor.count * vertexAccessor.TypeCount];
                var view = GLTF.bufferViews[vertexAccessor.bufferView];
                var segment = GetBytesFromBuffer(view.buffer);
                var bytes = new ArraySegment<Byte>(segment.Array, segment.Offset + view.byteOffset + vertexAccessor.byteOffset, vertexAccessor.count * view.byteStride);
                bytes.MarshalCopyTo(attrib);
                result = attrib;
            }
            else
            {
                result = new float[bufferCount];
            }

            var sparse = vertexAccessor.sparse;
            if (sparse != null && sparse.count > 0)
            {
                // override sparse values
                var indices = GetIntIndicesFromView(GLTF.bufferViews[sparse.indices.bufferView], sparse.count, sparse.indices.byteOffset, sparse.indices.componentType);
                var values = GetTypedFromBufferView<float>(sparse.count * vertexAccessor.TypeCount, sparse.values.byteOffset, GLTF.bufferViews[sparse.values.bufferView]);

                var it = indices.GetEnumerator();
                for (int i = 0; i < sparse.count; ++i)
                {
                    it.MoveNext();
                    result[it.Current] = values[i];
                }
            }
            return result;
        }

        public ArraySegment<Byte> GetBytesFromImage(int imageIndex)
        {
            var image = GLTF.images[imageIndex];
            if (string.IsNullOrEmpty(image.uri))
            {
                return GetBytesFromBufferView(image.bufferView);
            }
            else
            {
                return GetBytesFromUri(image.uri);
            }
        }

        public ArraySegment<Byte> GetImageBytesFromTextureIndex(int textureIndex)
        {
            var imageIndex = GLTF.textures[textureIndex].source;
            return GetBytesFromImage(imageIndex);
        }
    }
}
