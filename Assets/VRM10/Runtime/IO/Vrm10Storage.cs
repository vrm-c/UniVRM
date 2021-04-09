using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using UniGLTF;
using UniJSON;
using VrmLib;


namespace UniVRM10
{
    public class Vrm10Storage
    {
        UniGLTF.GltfParser m_parser;
        public UniGLTF.glTF Gltf => m_parser.GLTF;

        public List<UniGLTF.IBytesBuffer> Buffers;

        public UniGLTF.Extensions.VRMC_vrm.VRMC_vrm gltfVrm;

        public UniGLTF.Extensions.VRMC_springBone.VRMC_springBone gltfVrmSpringBone;

        /// <summary>
        /// for export
        /// </summary>
        public Vrm10Storage()
        {
            m_parser = new UniGLTF.GltfParser
            {
                GLTF = new UniGLTF.glTF()
                {
                    extensionsUsed = new List<string>(),
                }
            };
            Buffers = new List<UniGLTF.IBytesBuffer>()
            {
                new UniGLTF.ArrayByteBuffer()
            };

            Gltf.AddBuffer(Buffers[0]);
        }

        /// <summary>
        /// for import
        /// </summary>
        /// <param name="json"></param>
        /// <param name="bin"></param>
        public Vrm10Storage(UniGLTF.GltfParser parser)
        {
            m_parser = parser;

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

            Buffers = new List<UniGLTF.IBytesBuffer>()
            {
                Gltf.buffers[0].Buffer,
            };
        }

        public void Reserve(int bytesLength)
        {
            Buffers[0].ExtendCapacity(bytesLength);
        }

        public int AppendToBuffer(int bufferIndex, ArraySegment<byte> segment)
        {
            var gltfBufferView = Buffers[bufferIndex].Extend(segment);
            var viewIndex = Gltf.bufferViews.Count;
            Gltf.bufferViews.Add(gltfBufferView);
            return viewIndex;
        }

        static ArraySegment<byte> RestoreSparseAccessorUInt16<T>(ArraySegment<byte> bytes, int accessorCount, ArraySegment<byte> indicesBytes, ArraySegment<byte> valuesBytes)
            where T : struct
        {
            var stride = Marshal.SizeOf(typeof(T));
            if (bytes.Count == 0)
            {
                bytes = new ArraySegment<byte>(new byte[accessorCount * stride]);
            }
            var dst = SpanLike.Wrap<T>(bytes);

            var indices = SpanLike.Wrap<UInt16>(indicesBytes);
            var values = SpanLike.Wrap<T>(valuesBytes);

            for (int i = 0; i < indices.Length; ++i)
            {
                var index = indices[i];
                var value = values[i];
                dst[index] = value;
            }

            return bytes;
        }

        static ArraySegment<byte> RestoreSparseAccessorUInt32<T>(ArraySegment<byte> bytes, int accessorCount, ArraySegment<byte> indicesBytes, ArraySegment<byte> valuesBytes)
            where T : struct
        {
            var stride = Marshal.SizeOf(typeof(T));
            if (bytes.Count == 0)
            {
                bytes = new ArraySegment<byte>(new byte[accessorCount * stride]);
            }
            var dst = SpanLike.Wrap<T>(bytes);

            var indices = SpanLike.Wrap<Int32>(indicesBytes);
            var values = SpanLike.Wrap<T>(valuesBytes);

            for (int i = 0; i < indices.Length; ++i)
            {
                var index = indices[i];
                var value = values[i];
                dst[index] = value;
            }

            return bytes;
        }

        public ArraySegment<byte> GetAccessorBytes(int accessorIndex)
        {
            var accessor = Gltf.accessors[accessorIndex];
            var sparse = accessor.sparse;

            ArraySegment<byte> bytes = default(ArraySegment<byte>);

            if (accessor.bufferView.TryGetValidIndex(Gltf.bufferViews.Count, out int bufferViewIndex))
            {
                var view = Gltf.bufferViews[bufferViewIndex];
                if (view.buffer.TryGetValidIndex(Gltf.buffers.Count, out int bufferIndex))
                {
                    var buffer = Buffers[bufferIndex];
                    var bin = buffer.Bytes;
                    var byteSize = accessor.CalcByteSize();
                    bytes = bin.Slice(view.byteOffset, view.byteLength).Slice(accessor.byteOffset, byteSize);
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
                    .Slice(sparseIndexView.byteOffset, sparseIndexView.byteLength)
                    .Slice(sparse.indices.byteOffset, ((AccessorValueType)sparse.indices.componentType).ByteSize() * sparse.count)
                    ;

                if (!sparse.values.bufferView.TryGetValidIndex(Gltf.bufferViews.Count, out int sparseValuesBufferViewIndex))
                {
                    throw new Exception();
                }
                var sparseValueView = Gltf.bufferViews[sparseValuesBufferViewIndex];
                var sparseValueBin = GetBufferBytes(sparseValueView);
                var sparseValueBytes = sparseValueBin
                    .Slice(sparseValueView.byteOffset, sparseValueView.byteLength)
                    .Slice(sparse.values.byteOffset, accessor.GetStride() * sparse.count);
                ;

                if (sparse.indices.componentType == (UniGLTF.glComponentType)AccessorValueType.UNSIGNED_SHORT
                    && accessor.componentType == (UniGLTF.glComponentType)AccessorValueType.FLOAT
                    && accessor.type == "VEC3")
                {
                    return RestoreSparseAccessorUInt16<Vector3>(bytes, accessor.count, sparseIndexBytes, sparseValueBytes);
                }
                if (sparse.indices.componentType == (UniGLTF.glComponentType)AccessorValueType.UNSIGNED_INT
                    && accessor.componentType == (UniGLTF.glComponentType)AccessorValueType.FLOAT
                    && accessor.type == "VEC3")
                {
                    return RestoreSparseAccessorUInt32<Vector3>(bytes, accessor.count, sparseIndexBytes, sparseValueBytes);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (bytes.Count == 0)
                {
                    // sparse and all value is zero
                    return new ArraySegment<byte>(new byte[accessor.GetStride() * accessor.count]);
                }

                return bytes;
            }
        }

        /// <summary>
        /// submeshのindexが連続した領域に格納されているかを確認する
        /// </summary>
        bool AccessorsIsContinuous(int[] accessorIndices)
        {
            var firstAccessor = Gltf.accessors[accessorIndices[0]];
            var firstView = Gltf.bufferViews[firstAccessor.bufferView];
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

                var view = Gltf.bufferViews[current.bufferView];
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
                var firstView = Gltf.bufferViews[firstAccessor.bufferView];
                var start = firstView.byteOffset + firstAccessor.byteOffset;
                if (!firstView.buffer.TryGetValidIndex(Gltf.buffers.Count, out int firstViewBufferIndex))
                {
                    throw new Exception();
                }
                var buffer = Gltf.buffers[firstViewBufferIndex];
                var bin = GetBufferBytes(buffer);
                var bytes = bin.Slice(start, totalCount * firstAccessor.GetStride());
                return new BufferAccessor(bytes,
                    (AccessorValueType)firstAccessor.componentType,
                    EnumUtil.Parse<AccessorVectorType>(firstAccessor.type),
                    totalCount);
            }
            else
            {
                // IndexBufferが連続して格納されていない => Int[] を作り直す
                var indices = new byte[totalCount * Marshal.SizeOf(typeof(int))];
                var span = SpanLike.Wrap<Int32>(new ArraySegment<byte>(indices));
                var offset = 0;
                foreach (var accessorIndex in accessorIndices)
                {
                    var accessor = Gltf.accessors[accessorIndex];
                    if (accessor.type != "SCALAR")
                    {
                        throw new ArgumentException($"accessor.type: {accessor.type}");
                    }
                    var view = Gltf.bufferViews[accessor.bufferView];
                    if (!view.buffer.TryGetValidIndex(Gltf.buffers.Count, out int viewBufferIndex))
                    {
                        throw new Exception();
                    }
                    var buffer = Gltf.buffers[viewBufferIndex];
                    var bin = GetBufferBytes(buffer);
                    var start = view.byteOffset + accessor.byteOffset;
                    var bytes = bin.Slice(start, accessor.count * accessor.GetStride());
                    var dst = SpanLike.Wrap<Int32>(new ArraySegment<byte>(indices)).Slice(offset, accessor.count);
                    offset += accessor.count;
                    switch ((AccessorValueType)accessor.componentType)
                    {
                        case AccessorValueType.UNSIGNED_BYTE:
                            {
                                var src = SpanLike.Wrap<Byte>(bytes);
                                for (int i = 0; i < src.Length; ++i)
                                {
                                    // byte to int
                                    dst[i] = src[i];
                                }
                            }
                            break;

                        case AccessorValueType.UNSIGNED_SHORT:
                            {
                                var src = SpanLike.Wrap<UInt16>(bytes);
                                for (int i = 0; i < src.Length; ++i)
                                {
                                    // ushort to int
                                    dst[i] = src[i];
                                }
                            }
                            break;

                        case AccessorValueType.UNSIGNED_INT:
                            {
                                Buffer.BlockCopy(bytes.Array, bytes.Offset, dst.Bytes.Array, dst.Bytes.Offset, bytes.Count);
                            }
                            break;

                        default:
                            throw new NotImplementedException($"accessor.componentType: {accessor.componentType}");
                    }
                }
                return new BufferAccessor(new ArraySegment<byte>(indices), AccessorValueType.UNSIGNED_INT, AccessorVectorType.SCALAR, totalCount);
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

        public bool TryCreateAccessor(int accessorIndex, out BufferAccessor ba)
        {
            if (accessorIndex < 0 || accessorIndex >= Gltf.accessors.Count)
            {
                ba = default;
                return false;
            }
            var accessor = Gltf.accessors[accessorIndex];
            var bytes = GetAccessorBytes(accessorIndex);
            var vectorType = EnumUtil.Parse<AccessorVectorType>(accessor.type);
            ba = new BufferAccessor(bytes,
                (AccessorValueType)accessor.componentType, vectorType, accessor.count);
            return true;
        }

        public string AssetVersion => Gltf.asset.version;

        public string AssetMinVersion => Gltf.asset.minVersion;

        public string AssetGenerator => Gltf.asset.generator;

        public string AssetCopyright => Gltf.asset.copyright;

        public int NodeCount => Gltf.nodes.Count;

        public int TextureCount => Gltf.textures.Count;

        public int SkinCount => Gltf.skins.Count;

        public int MeshCount => Gltf.meshes.Count;

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
                var m = new Matrix4x4(
                    x.matrix[0], x.matrix[1], x.matrix[2], x.matrix[3],
                    x.matrix[4], x.matrix[5], x.matrix[6], x.matrix[7],
                    x.matrix[8], x.matrix[9], x.matrix[10], x.matrix[11],
                    x.matrix[12], x.matrix[13], x.matrix[14], x.matrix[15]
                    );

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
                if (x.scale != null && x.scale.Length == 4)
                {
                    node.LocalScaling = x.scale.ToVector3(Vector3.One);
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

        public ArraySegment<byte> GetBufferBytes(UniGLTF.glTFBufferView bufferView)
        {
            if (!bufferView.buffer.TryGetValidIndex(Gltf.buffers.Count, out int bufferViewBufferIndex))
            {
                throw new Exception();
            }
            return GetBufferBytes(Gltf.buffers[bufferViewBufferIndex]);
        }

        public ArraySegment<byte> GetBufferBytes(UniGLTF.glTFBuffer buffer)
        {
            int index = Gltf.buffers.IndexOf(buffer);
            return Buffers[index].Bytes;
        }

        public byte[] ToBytes()
        {
            Gltf.buffers[0].byteLength = Buffers[0].Bytes.Count;

            var f = new JsonFormatter();
            UniGLTF.GltfSerializer.Serialize(f, Gltf);
            var json = f.GetStoreBytes();

            var glb = UniGLTF.Glb.Create(json, Buffers[0].Bytes);
            return glb.ToBytes();
        }
    }
}
