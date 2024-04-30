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

        public NativeArrayManager NativeArrayManager { get; } = new NativeArrayManager();

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
        /// Extension Supporting flags.
        ///
        /// User can set flags to enable or disable some extensions.
        /// </summary>
        public ExtensionSupportFlags ExtensionSupportFlags { get; } = new ExtensionSupportFlags();

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
                    Bin = NativeArrayManager.CreateNativeArray(Chunks[1].Bytes);
                }
            }
        }

        public void Dispose()
        {
            NativeArrayManager.Dispose();
            _UriCache.Clear();
        }

        public static GltfData CreateFromExportForTest(ExportingGltfData data)
        {
            return CreateFromGltfDataForTest(data.Gltf, data.BinBytes);
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
                data = NativeArrayManager.CreateNativeArray(UriByteBuffer.ReadEmbedded(uri));
            }
            else
            {
                data = NativeArrayManager.CreateNativeArray(_storage.Get(uri));
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

        NativeArray<byte> GetBytesFromBufferView(glTFBufferView view)
        {
            var segment = GetBytesFromBuffer(view.buffer);
            return segment.GetSubArray(view.byteOffset, view.byteLength);
        }

        NativeArray<T> GetTypedFromAccessor<T>(glTFAccessor accessor, glTFBufferView view) where T : struct
        {
            var bytes = GetBytesFromBufferView(view);
            if (view.byteStride == 0 || view.byteStride == accessor.GetStride())
            {
                // planar layout
                return bytes.GetSubArray(
                    accessor.byteOffset.GetValueOrDefault(),
                    accessor.CalcByteSize()).Reinterpret<T>(1);
            }
            else
            {
                // interleaved layout
                // copy interleaved vertex to planar array
                var src = GetBytesFromBufferView(view);
                var dst = NativeArrayManager.CreateNativeArray<T>(accessor.count);
                var offset = accessor.byteOffset.GetValueOrDefault();
                var size = Marshal.SizeOf<T>();
                for (int i = 0; i < accessor.count; ++i, offset += view.byteStride)
                {
                    var values = src.GetSubArray(offset, size).Reinterpret<T>(1);
                    dst[i] = values[0];
                }
                return dst;
            }
        }

        /// <summary>
        /// for sparse
        /// </summary>
        /// <param name="view"></param>
        /// <param name="count"></param>
        /// <param name="byteOffset"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
        BufferAccessor GetIntIndicesFromView(glTFBufferView view, int count, int byteOffset, glComponentType componentType)
        {
            var bytes = GetBytesFromBufferView(view);
            switch (componentType)
            {
                case glComponentType.UNSIGNED_BYTE:
                    {
                        return new BufferAccessor(NativeArrayManager, bytes.GetSubArray(byteOffset, bytes.Length - byteOffset),
                            AccessorValueType.UNSIGNED_BYTE, AccessorVectorType.SCALAR, count);
                    }

                case glComponentType.UNSIGNED_SHORT:
                    {
                        return new BufferAccessor(NativeArrayManager, bytes.GetSubArray(byteOffset, bytes.Length - byteOffset),
                            AccessorValueType.UNSIGNED_SHORT, AccessorVectorType.SCALAR, count);
                    }

                case glComponentType.UNSIGNED_INT:
                    {
                        return new BufferAccessor(NativeArrayManager, bytes.GetSubArray(byteOffset, bytes.Length - byteOffset),
                        AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR, count);
                    }

                default:
                    throw new NotImplementedException("GetIndices: unknown componenttype: " + componentType);
            }
        }

        public BufferAccessor GetIndicesFromAccessorIndex(int accessorIndex)
        {
            var accessor = GLTF.accessors[accessorIndex];
            var view = GLTF.bufferViews[accessor.bufferView.Value];
            return GetIntIndicesFromView(view, accessor.count, accessor.byteOffset.GetValueOrDefault(), accessor.componentType);
        }

        public NativeArray<T> GetArrayFromAccessor<T>(int accessorIndex) where T : struct
        {
            var vertexAccessor = GLTF.accessors[accessorIndex];

            if (vertexAccessor.count <= 0) return NativeArrayManager.CreateNativeArray<T>(0);

            var result = (vertexAccessor.bufferView.HasValidIndex())
                ? GetTypedFromAccessor<T>(vertexAccessor, GLTF.bufferViews[vertexAccessor.bufferView.Value])
                : NativeArrayManager.CreateNativeArray<T>(vertexAccessor.count)
                ;

            var sparse = vertexAccessor.sparse;
            if (sparse != null && sparse.count > 0)
            {
                // override sparse values
                var _indices = GetIntIndicesFromView(GLTF.bufferViews[sparse.indices.bufferView], sparse.count, sparse.indices.byteOffset, sparse.indices.componentType);
                var bytes = GetBytesFromBufferView(GLTF.bufferViews[sparse.values.bufferView]);
                var values = bytes.GetSubArray(sparse.values.byteOffset, bytes.Length - sparse.values.byteOffset).Reinterpret<T>(1).GetSubArray(0, sparse.count);

                switch (_indices.ComponentType)
                {
                    case AccessorValueType.UNSIGNED_BYTE:
                        {
                            var indices = _indices.Bytes;
                            for (int i = 0; i < sparse.count; ++i)
                            {
                                result[indices[i]] = values[i];
                            }
                            break;
                        }
                    case AccessorValueType.UNSIGNED_SHORT:
                        {
                            var indices = _indices.Bytes.Reinterpret<ushort>(1);
                            for (int i = 0; i < sparse.count; ++i)
                            {
                                result[indices[i]] = values[i];
                            }
                            break;
                        }
                    case AccessorValueType.UNSIGNED_INT:
                        {
                            var indices = _indices.Bytes.Reinterpret<int>(1);
                            for (int i = 0; i < sparse.count; ++i)
                            {
                                result[indices[i]] = values[i];
                            }
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }

        public NativeArray<float> FlatternFloatArrayFromAccessor(int accessorIndex)
        {
            var vertexAccessor = GLTF.accessors[accessorIndex];

            if (vertexAccessor.count <= 0) return NativeArrayManager.CreateNativeArray<float>(0);

            var bufferCount = vertexAccessor.count * vertexAccessor.TypeCount;

            NativeArray<float> result = default;
            if (vertexAccessor.bufferView.HasValidIndex())
            {
                var view = GLTF.bufferViews[vertexAccessor.bufferView.Value];
                var segment = GetBytesFromBuffer(view.buffer);
                result = segment.GetSubArray(view.byteOffset + vertexAccessor.byteOffset.GetValueOrDefault(), vertexAccessor.count * 4 * vertexAccessor.TypeCount).Reinterpret<float>(1);
            }
            else
            {
                result = NativeArrayManager.CreateNativeArray<float>(bufferCount);
            }

            var sparse = vertexAccessor.sparse;
            if (sparse != null && sparse.count > 0)
            {
                // override sparse values
                var _indices = GetIntIndicesFromView(GLTF.bufferViews[sparse.indices.bufferView], sparse.count, sparse.indices.byteOffset, sparse.indices.componentType);
                var bytes = GetBytesFromBufferView(GLTF.bufferViews[sparse.values.bufferView]);
                var values = bytes.GetSubArray(sparse.values.byteOffset, bytes.Length - sparse.values.byteOffset).Reinterpret<float>(1).GetSubArray(0, sparse.count * vertexAccessor.TypeCount);

                switch (_indices.ComponentType)
                {
                    case AccessorValueType.UNSIGNED_BYTE:
                        {
                            var indices = _indices.Bytes;
                            for (int i = 0; i < sparse.count; ++i)
                            {
                                result[indices[i]] = values[i];
                            }
                            break;
                        }
                    case AccessorValueType.UNSIGNED_SHORT:
                        {
                            var indices = _indices.Bytes.Reinterpret<ushort>(1);
                            for (int i = 0; i < sparse.count; ++i)
                            {
                                result[indices[i]] = values[i];
                            }
                            break;
                        }
                    case AccessorValueType.UNSIGNED_INT:
                        {
                            var indices = _indices.Bytes.Reinterpret<int>(1);
                            for (int i = 0; i < sparse.count; ++i)
                            {
                                result[indices[i]] = values[i];
                            }
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }

        static string GuessMimeFromUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return null;
            }
            var ext = System.IO.Path.GetExtension(uri).ToLowerInvariant();
            switch (ext)
            {
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                default:
                    return null;
            }
        }

        public (NativeArray<byte> binary, string mimeType)? GetBytesFromImage(int imageIndex)
        {
            if (imageIndex < 0 || imageIndex >= GLTF.images.Count) return default;

            var image = GLTF.images[imageIndex];
            if (string.IsNullOrEmpty(image.uri))
            {
                // use bufferView(glb)
                return (GetBytesFromBufferView(image.bufferView), image.mimeType);
            }
            else
            {
                // use uri(gltf)
                return (GetBytesFromUri(image.uri), image.mimeType ?? GuessMimeFromUri(image.uri));
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
    }
}
