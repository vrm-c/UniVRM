using System;
using System.Collections.Generic;
using System.IO;
using UniJSON;
using Unity.Collections;

namespace UniGLTF
{
    public class ExportingGltfData
    {
        public glTF Gltf { get; } = new glTF();

        protected ArrayByteBuffer _buffer;
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
            Gltf.buffers.Add(new glTFBuffer());
            _buffer = new ArrayByteBuffer(new byte[reserved]);
        }

        #region Buffer management for export
        public glTFBufferView ExtendBufferAndGetView<T>(ArraySegment<T> segment, glBufferTarget target) where T : struct
        {
            var view = _buffer.Extend(segment, target);
            Gltf.buffers[0].byteLength = _buffer.Bytes.Count;
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
            var viewIndex = Gltf.bufferViews.Count;
            Gltf.bufferViews.Add(view);
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
            Gltf.bufferViews[viewIndex].byteStride = 0;

            var accessorIndex = Gltf.accessors.Count;
            Gltf.accessors.Add(new glTFAccessor
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
            var accessorIndex = Gltf.accessors.Count;
            Gltf.accessors.Add(new glTFAccessor
            {
                byteOffset = default,
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

        public void Reserve(int bytesLength)
        {
            _buffer.ExtendCapacity(bytesLength);
        }

        public int AppendToBuffer(NativeArray<byte> segment)
        {
            var gltfBufferView = _buffer.Extend(segment);
            var viewIndex = Gltf.bufferViews.Count;
            Gltf.bufferViews.Add(gltfBufferView);
            return viewIndex;
        }

        #endregion

        #region ToGltf & ToGlb
        /// <summary>
        /// GLBバイト列
        /// </summary>
        /// <returns></returns>
        public byte[] ToGlbBytes()
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize(f, Gltf);

            var json = f.ToString().ParseAsJson().ToString("  ");

            json = GltfJsonUtil.FindUsedExtensionsAndUpdateJson(json);

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
            if (Gltf.buffers.Count == 1)
            {
                var withoutExt = Path.GetFileNameWithoutExtension(gltfPath);
                Gltf.buffers[0].uri = $"{withoutExt}.bin";
            }
            else
            {
                throw new NotImplementedException();
            }

            var f = new JsonFormatter();
            GltfSerializer.Serialize(f, Gltf);
            var json = f.ToString().ParseAsJson().ToString("  ");

            json = GltfJsonUtil.FindUsedExtensionsAndUpdateJson(json);

            return (json, Gltf.buffers[0]);
        }
        #endregion
    }
}
