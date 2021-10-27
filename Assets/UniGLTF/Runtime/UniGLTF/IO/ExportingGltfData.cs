using System;
using System.Collections.Generic;
using System.IO;
using UniJSON;

namespace UniGLTF
{
    public class ExportingGltfData
    {
        public glTF GLTF { get; } = new glTF();

        protected IBytesBuffer _buffer;
        /// <summary>
        /// bin chunk
        /// </summary>
        public ArraySegment<byte> BinBytes => _buffer.Bytes;

        public ExportingGltfData(int reserved = default)
        {
            if (reserved == 0)
            {
                reserved = 50 * 1024 * 1024;
            }

            // buffers[0] is export target
            GLTF.buffers.Add(new glTFBuffer());
            _buffer = new ArrayByteBuffer(new byte[reserved]);
        }

        #region Buffer management for export
        public glTFBufferView ExtendBufferAndGetView<T>(ArraySegment<T> segment, glBufferTarget target) where T : struct
        {
            var view = _buffer.Extend(segment, target);
            GLTF.buffers[0].byteLength = _buffer.Bytes.Count;
            return view;
        }

        public int ExtendBufferAndGetViewIndex<T>(
            ArraySegment<T> array,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            if (array.Count == 0)
            {
                return -1;
            }
            var view = ExtendBufferAndGetView(array, target);
            var viewIndex = GLTF.bufferViews.Count;
            GLTF.bufferViews.Add(view);
            return viewIndex;
        }

        public int ExtendBufferAndGetViewIndex<T>(
            T[] array,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            return ExtendBufferAndGetViewIndex(new ArraySegment<T>(array), target);
        }

        public int ExtendBufferAndGetAccessorIndex<T>(
            ArraySegment<T> array,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            if (array.Count == 0)
            {
                return -1;
            }
            var viewIndex = ExtendBufferAndGetViewIndex(array, target);

            // index buffer's byteStride is unnecessary
            GLTF.bufferViews[viewIndex].byteStride = 0;

            var accessorIndex = GLTF.accessors.Count;
            GLTF.accessors.Add(new glTFAccessor
            {
                bufferView = viewIndex,
                byteOffset = 0,
                componentType = glTFExtensions.GetComponentType<T>(),
                type = glTFExtensions.GetAccessorType<T>(),
                count = array.Count,
            });
            return accessorIndex;
        }

        public int ExtendBufferAndGetAccessorIndex<T>(T[] array,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            return ExtendBufferAndGetAccessorIndex(new ArraySegment<T>(array), target);
        }

        /// <summary>
        /// sparseValues は間引かれた配列
        /// </summary>
        public int ExtendSparseBufferAndGetAccessorIndex<T>(
            int accessorCount,
            T[] sparseValues, int[] sparseIndices, int sparseViewIndex,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            return ExtendSparseBufferAndGetAccessorIndex(
                accessorCount,
                new ArraySegment<T>(sparseValues), sparseIndices, sparseViewIndex,
                target);
        }

        public int ExtendSparseBufferAndGetAccessorIndex<T>(
            int accessorCount,
            ArraySegment<T> sparseValues, int[] sparseIndices, int sparseIndicesViewIndex,
            glBufferTarget target = glBufferTarget.NONE) where T : struct
        {
            if (sparseValues.Count == 0)
            {
                return -1;
            }
            var sparseValuesViewIndex = ExtendBufferAndGetViewIndex(sparseValues, target);
            var accessorIndex = GLTF.accessors.Count;
            GLTF.accessors.Add(new glTFAccessor
            {
                byteOffset = 0,
                componentType = glTFExtensions.GetComponentType<T>(),
                type = glTFExtensions.GetAccessorType<T>(),
                count = accessorCount,

                sparse = new glTFSparse
                {
                    count = sparseIndices.Length,
                    indices = new glTFSparseIndices
                    {
                        bufferView = sparseIndicesViewIndex,
                        componentType = glComponentType.UNSIGNED_INT
                    },
                    values = new glTFSparseValues
                    {
                        bufferView = sparseValuesViewIndex,
                    }
                }
            });
            return accessorIndex;
        }
        #endregion

        #region ToGltf & ToGlb
        static Utf8String s_extensions = Utf8String.From("extensions");

        static bool UsedExtension(glTF self, string key)
        {
            if (self.extensionsUsed.Contains(key))
            {
                return true;
            }

            return false;
        }

        static void Traverse(glTF self, JsonNode node, JsonFormatter f, Utf8String parentKey)
        {
            if (node.IsMap())
            {
                f.BeginMap();
                foreach (var kv in node.ObjectItems())
                {
                    if (parentKey == s_extensions)
                    {
                        if (!UsedExtension(self, kv.Key.GetString()))
                        {
                            // skip extension not in used
                            continue;
                        }
                    }
                    f.Key(kv.Key.GetUtf8String());
                    Traverse(self, kv.Value, f, kv.Key.GetUtf8String());
                }
                f.EndMap();
            }
            else if (node.IsArray())
            {
                f.BeginList();
                foreach (var x in node.ArrayItems())
                {
                    Traverse(self, x, f, default(Utf8String));
                }
                f.EndList();
            }
            else
            {
                f.Value(node);
            }
        }

        /// <summary>
        /// 出力前に不要な extension を削除する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        static string RemoveUnusedExtensions(glTF self, string json)
        {
            var f = new JsonFormatter();
            Traverse(self, JsonParser.Parse(json), f, default(Utf8String));
            return f.ToString();
        }

        /// <summary>
        /// GLBバイト列
        /// </summary>
        /// <returns></returns>
        public byte[] ToGlbBytes()
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize(f, GLTF);

            // remove unused extenions
            var json = f.ToString().ParseAsJson().ToString("  ");
            RemoveUnusedExtensions(GLTF, json);
            return Glb.Create(json, BinBytes).ToBytes();
        }

        /// <summary>
        /// glTF 形式で出力する？
        /// </summary>
        /// <param name="gltfPath"></param>
        /// <returns></returns>
        public (string, glTFBuffer) ToGltf(string gltfPath)
        {
            // fix buffer path
            if (GLTF.buffers.Count == 1)
            {
                var withoutExt = Path.GetFileNameWithoutExtension(gltfPath);
                GLTF.buffers[0].uri = $"{withoutExt}.bin";
            }
            else
            {
                throw new NotImplementedException();
            }

            var f = new JsonFormatter();
            GltfSerializer.Serialize(f, GLTF);
            var json = f.ToString().ParseAsJson().ToString("  ");
            RemoveUnusedExtensions(GLTF, json);
            return (json, GLTF.buffers[0]);
        }
        #endregion
    }
}
