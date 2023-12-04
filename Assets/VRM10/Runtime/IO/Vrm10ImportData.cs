using System;
using System.Linq;
using System.Runtime.InteropServices;
using UniGLTF;
using VrmLib;
using System.Collections.Generic;
using UniGLTF.Utils;
using Unity.Collections;
using UnityEngine;

namespace UniVRM10
{
    public class Vrm10ImportData
    {
        UniGLTF.GltfData m_data;
        public UniGLTF.GltfData Data => m_data;

        public UniGLTF.glTF Gltf => m_data.GLTF;
        public string AssetVersion => Gltf.asset.version;

        public string AssetMinVersion => Gltf.asset.minVersion;

        public string AssetGenerator => Gltf.asset.generator;

        public string AssetCopyright => Gltf.asset.copyright;

        public int NodeCount => Gltf.nodes.Count;

        public int TextureCount => Gltf.textures.Count;

        public int SkinCount => Gltf.skins.Count;

        public int MeshCount => Gltf.meshes.Count;

        public UniGLTF.Extensions.VRMC_vrm.VRMC_vrm gltfVrm;

        public UniGLTF.Extensions.VRMC_springBone.VRMC_springBone gltfVrmSpringBone;

        /// <summary>
        /// for import
        /// </summary>
        /// <param name="json"></param>
        /// <param name="bin"></param>
        public Vrm10ImportData(UniGLTF.GltfData data)
        {
            m_data = data;

            if (UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(Gltf.extensions,
                out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                gltfVrm = vrm;
            }

            if (UniGLTF.Extensions.VRMC_springBone.GltfDeserializer.TryGet(Gltf.extensions,
                out UniGLTF.Extensions.VRMC_springBone.VRMC_springBone springBone))
            {
                gltfVrmSpringBone = springBone;
            }
        }

        public NativeArray<byte> GetBufferBytes(UniGLTF.glTFBuffer buffer)
        {
            int index = Gltf.buffers.IndexOf(buffer);
            if (index != 0)
            {
                throw new NotImplementedException();
            }
            return m_data.Bin;
        }

        public NativeArray<byte> GetBufferBytes(UniGLTF.glTFBufferView bufferView)
        {
            if (!bufferView.buffer.TryGetValidIndex(Gltf.buffers.Count, out int bufferViewBufferIndex))
            {
                throw new Exception();
            }
            return GetBufferBytes(Gltf.buffers[bufferViewBufferIndex]);
        }

        static NativeArray<byte> RestoreSparseAccessorUInt8<T>(INativeArrayManager arrayManager, NativeArray<byte> bytes, int accessorCount, NativeArray<byte> indices, NativeArray<byte> valuesBytes)
            where T : struct
        {
            var stride = Marshal.SizeOf(typeof(T));
            if (bytes.Length == 0)
            {
                bytes = arrayManager.CreateNativeArray<byte>(accessorCount * stride);
            }
            var dst = bytes.Reinterpret<T>(1);
            var values = valuesBytes.Reinterpret<T>(1);

            for (int i = 0; i < indices.Length; ++i)
            {
                var index = indices[i];
                var value = values[i];
                dst[index] = value;
            }

            return bytes;
        }

        static NativeArray<byte> RestoreSparseAccessorUInt16<T>(INativeArrayManager arrayManager, NativeArray<byte> bytes, int accessorCount, NativeArray<byte> indicesBytes, NativeArray<byte> valuesBytes)
            where T : struct
        {
            var stride = Marshal.SizeOf(typeof(T));
            if (bytes.Length == 0)
            {
                bytes = arrayManager.CreateNativeArray<byte>(accessorCount * stride);
            }
            var dst = bytes.Reinterpret<T>(1);

            var indices = indicesBytes.Reinterpret<UInt16>(1);
            var values = valuesBytes.Reinterpret<T>(1);

            for (int i = 0; i < indices.Length; ++i)
            {
                var index = indices[i];
                var value = values[i];
                dst[index] = value;
            }

            return bytes;
        }

        static NativeArray<byte> RestoreSparseAccessorUInt32<T>(INativeArrayManager arrayManager, NativeArray<byte> bytes, int accessorCount, NativeArray<byte> indicesBytes, NativeArray<byte> valuesBytes)
            where T : struct
        {
            var stride = Marshal.SizeOf(typeof(T));
            if (bytes.Length == 0)
            {
                bytes = arrayManager.CreateNativeArray<byte>(accessorCount * stride);
            }
            var dst = bytes.Reinterpret<T>(1);

            var indices = indicesBytes.Reinterpret<Int32>(1);
            var values = valuesBytes.Reinterpret<T>(1);

            for (int i = 0; i < indices.Length; ++i)
            {
                var index = indices[i];
                var value = values[i];
                dst[index] = value;
            }

            return bytes;
        }

        public NativeArray<byte> GetAccessorBytes(int accessorIndex)
        {
            var accessor = Gltf.accessors[accessorIndex];
            var sparse = accessor.sparse;

            NativeArray<byte> bytes = default;

            if (accessor.bufferView.TryGetValidIndex(Gltf.bufferViews.Count, out int bufferViewIndex))
            {
                var view = Gltf.bufferViews[bufferViewIndex];
                if (view.buffer.TryGetValidIndex(Gltf.buffers.Count, out int bufferIndex))
                {
                    var buffer = m_data.Bin;
                    var byteSize = accessor.CalcByteSize();
                    bytes = m_data.Bin.GetSubArray(view.byteOffset, view.byteLength).GetSubArray(accessor.byteOffset.GetValueOrDefault(), byteSize);
                }
            }

            if (sparse != null)
            {
                if (!sparse.indices.bufferView.TryGetValidIndex(Gltf.bufferViews.Count, out int sparseIndicesBufferViewIndex))
                {
                    throw new Exception();
                }
                var sparseIndexView = Gltf.bufferViews[sparseIndicesBufferViewIndex];
                var sparseIndexBin = GetBufferBytes(sparseIndexView);
                var sparseIndexBytes = sparseIndexBin
                    .GetSubArray(sparseIndexView.byteOffset, sparseIndexView.byteLength)
                    .GetSubArray(sparse.indices.byteOffset, ((AccessorValueType)sparse.indices.componentType).ByteSize() * sparse.count)
                    ;

                if (!sparse.values.bufferView.TryGetValidIndex(Gltf.bufferViews.Count, out int sparseValuesBufferViewIndex))
                {
                    throw new Exception();
                }
                var sparseValueView = Gltf.bufferViews[sparseValuesBufferViewIndex];
                var sparseValueBin = GetBufferBytes(sparseValueView);
                var sparseValueBytes = sparseValueBin
                    .GetSubArray(sparseValueView.byteOffset, sparseValueView.byteLength)
                    .GetSubArray(sparse.values.byteOffset, accessor.GetStride() * sparse.count);
                ;

                if (sparse.indices.componentType == (UniGLTF.glComponentType)AccessorValueType.UNSIGNED_BYTE
                    && accessor.componentType == (UniGLTF.glComponentType)AccessorValueType.FLOAT
                    && accessor.type == "VEC3")
                {
                    return RestoreSparseAccessorUInt8<Vector3>(m_data.NativeArrayManager, bytes, accessor.count, sparseIndexBytes, sparseValueBytes);
                }
                if (sparse.indices.componentType == (UniGLTF.glComponentType)AccessorValueType.UNSIGNED_SHORT
                    && accessor.componentType == (UniGLTF.glComponentType)AccessorValueType.FLOAT
                    && accessor.type == "VEC3")
                {
                    return RestoreSparseAccessorUInt16<Vector3>(m_data.NativeArrayManager, bytes, accessor.count, sparseIndexBytes, sparseValueBytes);
                }
                if (sparse.indices.componentType == (UniGLTF.glComponentType)AccessorValueType.UNSIGNED_INT
                    && accessor.componentType == (UniGLTF.glComponentType)AccessorValueType.FLOAT
                    && accessor.type == "VEC3")
                {
                    return RestoreSparseAccessorUInt32<Vector3>(m_data.NativeArrayManager, bytes, accessor.count, sparseIndexBytes, sparseValueBytes);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (bytes.Length == 0)
                {
                    // sparse and all value is zero
                    return m_data.NativeArrayManager.CreateNativeArray<byte>(accessor.GetStride() * accessor.count);
                }

                return bytes;
            }
        }

        public bool TryCreateAccessor(int accessorIndex, out BufferAccessor ba)
        {
            if (accessorIndex < 0 || accessorIndex >= Gltf.accessors.Count)
            {
                ba = default;
                return false;
            }
            var accessor = Gltf.accessors[accessorIndex];
            var bytes = GetAccessorBytes(accessorIndex);
            var vectorType = CachedEnum.Parse<AccessorVectorType>(accessor.type, ignoreCase: true);
            ba = new BufferAccessor(m_data.NativeArrayManager, bytes,
                (AccessorValueType)accessor.componentType, vectorType, accessor.count);
            return true;
        }

        public BufferAccessor CreateAccessor(int accessorIndex)
        {
            if (TryCreateAccessor(accessorIndex, out BufferAccessor ba))
            {
                return ba;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// submeshのindexが連続した領域に格納されているかを確認する
        /// </summary>
        bool AccessorsIsContinuous(int[] accessorIndices)
        {
            var firstAccessor = Gltf.accessors[accessorIndices[0]];
            var firstView = Gltf.bufferViews[firstAccessor.bufferView.Value];
            var start = firstView.byteOffset + firstAccessor.byteOffset;
            var pos = start;
            foreach (var i in accessorIndices)
            {
                var current = Gltf.accessors[i];
                if (current.type != "SCALAR")
                {
                    throw new ArgumentException($"accessor.type: {current.type}");
                }
                if (firstAccessor.componentType != current.componentType)
                {
                    return false;
                }

                var view = Gltf.bufferViews[current.bufferView.Value];
                if (pos != view.byteOffset + current.byteOffset)
                {
                    return false;
                }

                var begin = view.byteOffset + current.byteOffset;
                var byteLength = current.CalcByteSize();

                pos += byteLength;
            }

            return true;
        }

        /// <summary>
        /// Gltfの Primitive[] の indices をひとまとめにした
        /// IndexBuffer を返す。
        /// </summary>
        public BufferAccessor CreateAccessor(int[] accessorIndices)
        {
            var totalCount = accessorIndices.Sum(x => Gltf.accessors[x].count);
            if (AccessorsIsContinuous(accessorIndices))
            {
                // IndexBufferが連続して格納されている => Slice でいける
                var firstAccessor = Gltf.accessors[accessorIndices[0]];
                var firstView = Gltf.bufferViews[firstAccessor.bufferView.Value];
                var start = firstView.byteOffset + firstAccessor.byteOffset.GetValueOrDefault();
                if (!firstView.buffer.TryGetValidIndex(Gltf.buffers.Count, out int firstViewBufferIndex))
                {
                    throw new Exception();
                }
                var buffer = Gltf.buffers[firstViewBufferIndex];
                var bin = GetBufferBytes(buffer);
                var bytes = bin.GetSubArray(start, totalCount * firstAccessor.GetStride());
                return new BufferAccessor(m_data.NativeArrayManager, bytes,
                    (AccessorValueType)firstAccessor.componentType,
                    CachedEnum.Parse<AccessorVectorType>(firstAccessor.type, ignoreCase: true),
                    totalCount);
            }
            else
            {
                // IndexBufferが連続して格納されていない => Int[] を作り直す
                using (var indices = new NativeArray<byte>(totalCount * Marshal.SizeOf(typeof(int)), Allocator.Persistent))
                {
                    var span = indices.Reinterpret<Int32>(1);
                    var offset = 0;
                    foreach (var accessorIndex in accessorIndices)
                    {
                        var accessor = Gltf.accessors[accessorIndex];
                        if (accessor.type != "SCALAR")
                        {
                            throw new ArgumentException($"accessor.type: {accessor.type}");
                        }
                        var view = Gltf.bufferViews[accessor.bufferView.Value];
                        if (!view.buffer.TryGetValidIndex(Gltf.buffers.Count, out int viewBufferIndex))
                        {
                            throw new Exception();
                        }
                        var buffer = Gltf.buffers[viewBufferIndex];
                        var bin = GetBufferBytes(buffer);
                        var start = view.byteOffset + accessor.byteOffset.GetValueOrDefault();
                        var bytes = bin.GetSubArray(start, accessor.count * accessor.GetStride());
                        var dst = indices.Reinterpret<Int32>(1).GetSubArray(offset, accessor.count);
                        offset += accessor.count;
                        switch ((AccessorValueType)accessor.componentType)
                        {
                            case AccessorValueType.UNSIGNED_BYTE:
                                {
                                    var src = bytes;
                                    for (int i = 0; i < src.Length; ++i)
                                    {
                                        // byte to int
                                        dst[i] = src[i];
                                    }
                                }
                                break;

                            case AccessorValueType.UNSIGNED_SHORT:
                                {
                                    var src = bytes.Reinterpret<UInt16>(1);
                                    for (int i = 0; i < src.Length; ++i)
                                    {
                                        // ushort to int
                                        dst[i] = src[i];
                                    }
                                }
                                break;

                            case AccessorValueType.UNSIGNED_INT:
                                {
                                    NativeArray<byte>.Copy(bytes, dst.Reinterpret<byte>(Marshal.SizeOf<Int32>()));
                                }
                                break;

                            default:
                                throw new NotImplementedException($"accessor.componentType: {accessor.componentType}");
                        }
                    }
                    return new BufferAccessor(m_data.NativeArrayManager, indices, AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR, totalCount);
                }
            }
        }

        public void CreateBufferAccessorAndAdd(int accessorIndex, VertexBuffer b, string key)
        {
            var a = CreateAccessor(accessorIndex);
            if (a != null)
            {
                b.Add(key, a);
            }
        }

        public Node CreateNode(int index)
        {
            var x = Gltf.nodes[index];
            var node = new Node(x.name);
            if (x.matrix != null)
            {
                if (x.matrix.Length != 16) throw new Exception("matrix member is not 16");
                if (x.translation != null && x.translation.Length > 0) throw new Exception("matrix with translation");
                if (x.rotation != null && x.rotation.Length > 0) throw new Exception("matrix with rotation");
                if (x.scale != null && x.scale.Length > 0) throw new Exception("matrix with scale");
                var m = UnityExtensions.MatrixFromArray(x.matrix);

                node.SetLocalMatrix(m, true);
            }
            else
            {
                if (x.translation != null && x.translation.Length == 3)
                {
                    node.LocalTranslation = x.translation.ToVector3();
                }
                if (x.rotation != null && x.rotation.Length == 4)
                {
                    node.LocalRotation = x.rotation.ToQuaternion();
                }
                if (x.scale != null && x.scale.Length == 3)
                {
                    node.LocalScaling = x.scale.ToVector3(Vector3.one);
                }
            }
            return node;
        }

        public IEnumerable<int> GetChildNodeIndices(int i)
        {
            var gltfNode = Gltf.nodes[i];
            if (gltfNode.children != null)
            {
                foreach (var j in gltfNode.children)
                {
                    yield return j;
                }
            }
        }

        public Skin CreateSkin(int index, List<Node> nodes)
        {
            var x = Gltf.skins[index];
            BufferAccessor inverseMatrices = null;
            if (x.inverseBindMatrices != -1)
            {
                inverseMatrices = CreateAccessor(x.inverseBindMatrices);
            }
            var skin = new Skin
            {
                InverseMatrices = inverseMatrices,
                Joints = x.joints.Select(y => nodes[y]).ToList(),
            };
            if (x.skeleton != -1) // TODO: proto to int
            {
                skin.Root = nodes[x.skeleton];
            }
            return skin;
        }

        public MeshGroup CreateMesh(int index)
        {
            var x = Gltf.meshes[index];
            var group = x.FromGltf(this);
            return group;
        }

        public (int, int) GetNodeMeshSkin(int index)
        {
            var x = Gltf.nodes[index];

            int meshIndex = -1;
            if (x.mesh.TryGetValidIndex(Gltf.meshes.Count, out int mi))
            {
                meshIndex = mi;
            }

            int skinIndex = -1;
            if (x.skin.TryGetValidIndex(Gltf.skins.Count, out int si))
            {
                skinIndex = si;
            }

            return (meshIndex, skinIndex);
        }
    }
}
