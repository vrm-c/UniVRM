using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;

namespace UniGLTF
{
    /// <summary>
    /// * JSON is parsed but not validated as glTF
    /// * For glb, bin chunks are already available
    /// </summary>
    public sealed class GltfData : IDisposable
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
        public NativeArray<byte> Bin { get; }

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
        Dictionary<string, NativeArray<byte>> _UriCache = new Dictionary<string, NativeArray<byte>>();

        List<IDisposable> m_disposables = new List<IDisposable>();

        public GltfData(string targetPath, string json, glTF gltf, IReadOnlyList<GlbChunk> chunks, IStorage storage, MigrationFlags migrationFlags)
        {
            TargetPath = targetPath;
            Json = json;
            GLTF = gltf;
            Chunks = chunks;
            _storage = storage;
            MigrationFlags = migrationFlags;

            // init
            if (Chunks != null)
            {
                if (Chunks.Count >= 2)
                {
                    Bin = CreateNativeArray(Chunks[1].Bytes);
                }
            }
        }

        public void Dispose()
        {
            foreach (var disposable in m_disposables)
            {
                disposable.Dispose();
            }
            m_disposables.Clear();
            _UriCache.Clear();
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

        NativeArray<Byte> GetBytesFromUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException();
            }

            if (_UriCache.TryGetValue(uri, out NativeArray<byte> data))
            {
                // return cache
                return data;
            }

            if (uri.StartsWith("data:", StringComparison.Ordinal))
            {
                data = CreateNativeArray(UriByteBuffer.ReadEmbedded(uri));
            }
            else
            {
                data = CreateNativeArray(_storage.Get(uri));
            }
            _UriCache.Add(uri, data);
            return data;
        }

        public NativeArray<Byte> GetBytesFromBuffer(int bufferIndex)
        {
            var buffer = GLTF.buffers[bufferIndex];
            if (bufferIndex == 0 && Bin.IsCreated)
            {
                // https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#glb-stored-buffer
                return Bin;
            }
            else
            {
                return GetBytesFromUri(buffer.uri);
            }
        }

        public NativeArray<Byte> GetBytesFromBufferView(int bufferView)
        {
            var view = GLTF.bufferViews[bufferView];
            var segment = GetBytesFromBuffer(view.buffer);
            return segment.GetSubArray(view.byteOffset, view.byteLength);
        }

        NativeArray<T> GetTypedFromBufferView<T>(int count, int byteOffset, glTFBufferView view) where T : struct
        {
            var segment = GetBytesFromBuffer(view.buffer);
            return segment.GetSubArray(view.byteOffset + byteOffset, count * Marshal.SizeOf<T>()).Reinterpret<T>(1);
        }

        NativeArray<T> GetTypedFromAccessor<T>(glTFAccessor accessor, glTFBufferView view) where T : struct
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

        public NativeArray<T> GetArrayFromAccessor<T>(int accessorIndex) where T : struct
        {
            var vertexAccessor = GLTF.accessors[accessorIndex];

            if (vertexAccessor.count <= 0) return CreateNativeArray<T>(0);

            var result = (vertexAccessor.bufferView != -1)
                ? GetTypedFromAccessor<T>(vertexAccessor, GLTF.bufferViews[vertexAccessor.bufferView])
                : CreateNativeArray<T>(vertexAccessor.count)
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

        public NativeArray<float> FlatternFloatArrayFromAccessor(int accessorIndex)
        {
            var vertexAccessor = GLTF.accessors[accessorIndex];

            if (vertexAccessor.count <= 0) return CreateNativeArray<float>(0);

            var bufferCount = vertexAccessor.count * vertexAccessor.TypeCount;

            NativeArray<float> result = default;
            if (vertexAccessor.bufferView != -1)
            {
                var view = GLTF.bufferViews[vertexAccessor.bufferView];
                var segment = GetBytesFromBuffer(view.buffer);
                result = segment.GetSubArray(view.byteOffset + vertexAccessor.byteOffset, vertexAccessor.count * 4 * vertexAccessor.TypeCount).Reinterpret<float>(1);
            }
            else
            {
                result = CreateNativeArray<float>(bufferCount);
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

        public (NativeArray<byte> binary, string mimeType)? GetBytesFromImage(int imageIndex)
        {
            if (imageIndex < 0 || imageIndex >= GLTF.images.Count) return default;

            var image = GLTF.images[imageIndex];
            if (string.IsNullOrEmpty(image.uri))
            {
                return (GetBytesFromBufferView(image.bufferView), image.mimeType);
            }
            else
            {
                return (GetBytesFromUri(image.uri), image.mimeType);
            }
        }

        // not black(0, 0, 0, 1)
        static readonly UnityEngine.Color ZERO = new UnityEngine.Color(0, 0, 0, 0);

        public bool HasVertexColor(glTFAttributes attributes)
        {
            if (attributes.COLOR_0 == -1)
            {
                return false;
            }

            var colors = GetArrayFromAccessor<UnityEngine.Color>(attributes.COLOR_0);
            foreach (var color in colors)
            {
                if (color != ZERO)
                {
                    return true;
                }
            }
            // すべて (0, 0, 0, 0) だった。使っていないと見做す。
            return false;
        }

        public bool MaterialHasVertexColor(int materialIndex)
        {
            if (materialIndex < 0 || materialIndex >= GLTF.materials.Count)
            {
                // index out of range. material not exists
                return false;
            }

            foreach (var mesh in GLTF.meshes)
            {
                foreach (var prim in mesh.primitives)
                {
                    if (prim.material == materialIndex && HasVertexColor(prim.attributes))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// NativeArrayを新規作成し、Dispose管理する。
        /// 個別にDisposeする必要が無い。
        /// </summary>
        /// <param name="size"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public NativeArray<T> CreateNativeArray<T>(int size) where T : struct
        {
            var array = new NativeArray<T>(size, Allocator.Persistent);
            m_disposables.Add(array);
            return array;
        }

        NativeArray<T> CreateNativeArray<T>(ArraySegment<T> data) where T : struct
        {
            var array = CreateNativeArray<T>(data.Count);
            // TODO: remove ToArray
            array.CopyFrom(data.ToArray());
            return array;
        }

        NativeArray<T> CreateNativeArray<T>(T[] data) where T : struct
        {
            var array = CreateNativeArray<T>(data.Length);
            array.CopyFrom(data);
            return array;
        }
    }
}
