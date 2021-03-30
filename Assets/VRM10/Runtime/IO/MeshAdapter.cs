using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using UniGLTF;
using VrmLib;

namespace UniVRM10
{
    public static class MeshAdapter
    {
        /// <summary>
        /// VertexBufferはひとつでIndexBufferの参照が異なる
        ///
        ///  VertexBuffer
        ///  +----------------------------------+
        ///  |                                  |
        ///  +----------------------------------+
        ///       A         A        A
        ///       |         |        |
        ///  +---------+--------+--------+
        ///  | submesh0|submesh1|submesh2|
        ///  +---------+--------+--------+
        ///  IndexBuffer
        /// </summary>
        public static Mesh SharedBufferFromGltf(this glTFMesh x, Vrm10Storage storage)
        {
            // 先頭を使う
            return FromGltf(storage, x, x.primitives[0], true);
        }

        /// <summary>
        /// IndexBuffer毎に異なるVertexBufferを参照する
        ///
        ///  VertexBuffer
        ///  +--------+ +--------+ +--------+
        ///  |0       | |1       | |2       |
        ///  +--------+ +--------+ +--------+
        ///       A         A        A
        ///       |         |        |
        ///  +---------+--------+--------+
        ///  | submesh0|submesh1|submesh2|
        ///  +---------+--------+--------+
        ///  IndexBuffer
        /// </summary>
        public static Mesh FromGltf(this glTFPrimitives primitive, Vrm10Storage storage, glTFMesh x)
        {
            return FromGltf(storage, x, primitive, false);
        }

        static Mesh FromGltf(Vrm10Storage storage, glTFMesh x, glTFPrimitives primitive, bool isShared)
        {
            var mesh = new Mesh((TopologyType)primitive.mode)
            {
                VertexBuffer = primitive.attributes.FromGltf(storage)
            };

            if (isShared)
            {
                // create joined index buffer
                mesh.IndexBuffer = storage.CreateAccessor(x.primitives.Select(y => y.indices).ToArray());
            }
            else
            {
                mesh.IndexBuffer = storage.CreateAccessor(primitive.indices);
            }

            {
                gltf_mesh_extras_targetNames.TryGet(x, out List<string> targetNames);

                for (int i = 0; i < primitive.targets.Count; ++i)
                {
                    var gltfTarget = primitive.targets[i];

                    string targetName = null;
                    {
                        targetName = targetNames[i];
                    }
                    var target = new MorphTarget(targetName)
                    {
                        VertexBuffer = gltfTarget.FromGltf(storage)
                    };

                    // validate count
                    foreach (var kv in target.VertexBuffer)
                    {
                        if (kv.Value.Count != mesh.VertexBuffer.Count)
                        {
                            throw new Exception();
                        }
                    }

                    mesh.MorphTargets.Add(target);
                }
            }

            return mesh;
        }

        public static VertexBuffer FromGltf(this glTFAttributes attributes,
            Vrm10Storage storage)
        {
            var b = new VertexBuffer();

            if (storage.TryCreateAccessor(attributes.POSITION, out BufferAccessor position))
            {
                b.Add(VertexBuffer.PositionKey, position);
            }
            else
            {
                // position required
                throw new Exception();
            }

            if (storage.TryCreateAccessor(attributes.NORMAL, out BufferAccessor normal)) b.Add(VertexBuffer.NormalKey, normal);
            if (storage.TryCreateAccessor(attributes.COLOR_0, out BufferAccessor color)) b.Add(VertexBuffer.ColorKey, color);
            if (storage.TryCreateAccessor(attributes.TEXCOORD_0, out BufferAccessor tex0)) b.Add(VertexBuffer.TexCoordKey, tex0);
            if (storage.TryCreateAccessor(attributes.TEXCOORD_1, out BufferAccessor tex1)) b.Add(VertexBuffer.TexCoordKey2, tex1);
            // if(storage.TryCreateAccessor(attributes.TANGENT, out BufferAccessor tangent))b.Add(VertexBuffer.TangentKey, tangent);
            if (storage.TryCreateAccessor(attributes.WEIGHTS_0, out BufferAccessor weights)) b.Add(VertexBuffer.WeightKey, weights);
            if (storage.TryCreateAccessor(attributes.JOINTS_0, out BufferAccessor joints)) b.Add(VertexBuffer.JointKey, joints);

            return b;
        }

        public static VertexBuffer FromGltf(this gltfMorphTarget target, Vrm10Storage storage)
        {
            var b = new VertexBuffer();
            storage.CreateBufferAccessorAndAdd(target.POSITION, b, VertexBuffer.PositionKey);
            storage.CreateBufferAccessorAndAdd(target.NORMAL, b, VertexBuffer.NormalKey);
            storage.CreateBufferAccessorAndAdd(target.TANGENT, b, VertexBuffer.TangentKey);
            return b;
        }

        public static bool HasSameVertexBuffer(this glTFPrimitives lhs, glTFPrimitives rhs)
        {
            if (lhs.attributes.POSITION != rhs.attributes.POSITION) return false;
            if (lhs.attributes.NORMAL != rhs.attributes.NORMAL) return false;
            if (lhs.attributes.TEXCOORD_0 != rhs.attributes.TEXCOORD_0) return false;
            if (lhs.attributes.TEXCOORD_1 != rhs.attributes.TEXCOORD_1) return false;
            if (lhs.attributes.COLOR_0 != rhs.attributes.COLOR_0) return false;
            if (lhs.attributes.WEIGHTS_0 != rhs.attributes.WEIGHTS_0) return false;
            if (lhs.attributes.JOINTS_0 != rhs.attributes.JOINTS_0) return false;
            return true;
        }

        public static bool AllPrimitivesHasSameVertexBuffer(this glTFMesh m)
        {
            if (m.primitives.Count <= 1)
            {
                return true;
            }

            var first = m.primitives[0];
            for (int i = 1; i < m.primitives.Count; ++i)
            {
                if (!first.HasSameVertexBuffer(m.primitives[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static MeshGroup FromGltf(this glTFMesh x, Vrm10Storage storage)
        {
            var group = new MeshGroup(x.name);

            if (x.primitives.Count == 1)
            {
                var primitive = x.primitives[0];
                var mesh = primitive.FromGltf(storage, x);
                var materialIndex = primitive.material;

                mesh.Submeshes.Add(
                    new Submesh(0, mesh.IndexBuffer.Count, materialIndex));

                group.Meshes.Add(mesh);
            }
            else if (!x.AllPrimitivesHasSameVertexBuffer())
            {
                int offset = 0;
                foreach (var primitive in x.primitives)
                {
                    var mesh = primitive.FromGltf(storage, x);
                    var materialIndex = primitive.material;

                    mesh.Submeshes.Add(
                        new Submesh(offset, mesh.IndexBuffer.Count, materialIndex));
                    offset += mesh.IndexBuffer.Count;

                    group.Meshes.Add(mesh);
                }
            }
            else
            {
                // for VRM

                var mesh = x.SharedBufferFromGltf(storage);
                int offset = 0;
                foreach (var primitive in x.primitives)
                {
                    var materialIndex = primitive.material;
                    var count = storage.Gltf.accessors[primitive.indices].count;
                    mesh.Submeshes.Add(
                        new Submesh(offset, count, materialIndex));
                    offset += count;
                }

                group.Meshes.Add(mesh);
            }

            return group;
        }

        static void Vec3MinMax(ArraySegment<byte> bytes, glTFAccessor accessor)
        {
            var positions = SpanLike.Wrap<Vector3>(bytes);
            var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            foreach (var p in positions)
            {
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
            accessor.min = min.ToFloat3();
            accessor.max = max.ToFloat3();
        }

        static int ExportIndices(Vrm10Storage storage, BufferAccessor x, int offset, int count, ExportArgs option)
        {
            if (x.Count <= ushort.MaxValue)
            {
                if (x.ComponentType == AccessorValueType.UNSIGNED_INT)
                {
                    // ensure ushort
                    var src = x.GetSpan<UInt32>().Slice(offset, count);
                    var bytes = new byte[src.Length * 2];
                    var dst = SpanLike.Wrap<UInt16>(new ArraySegment<byte>(bytes));
                    for (int i = 0; i < src.Length; ++i)
                    {
                        dst[i] = (ushort)src[i];
                    }
                    var accessor = new BufferAccessor(new ArraySegment<byte>(bytes), AccessorValueType.UNSIGNED_SHORT, AccessorVectorType.SCALAR, count);
                    return accessor.AddAccessorTo(storage, 0, option.sparse, null, 0, count);
                }
                else
                {
                    return x.AddAccessorTo(storage, 0, option.sparse, null, offset, count);
                }
            }
            else
            {
                return x.AddAccessorTo(storage, 0, option.sparse, null, offset, count);
            }
        }

        static void ExportMesh(this Mesh mesh, List<object> materials, Vrm10Storage storage, glTFMesh gltfMesh, ExportArgs option)
        {
            //
            // primitive share vertex buffer
            //
            var attributeAccessorIndexMap = mesh.VertexBuffer
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.AddAccessorTo(
                        storage, 0, option.sparse,
                        kv.Key == VertexBuffer.PositionKey ? (Action<ArraySegment<byte>, glTFAccessor>)Vec3MinMax : null
                    )
                );

            List<Dictionary<string, int>> morphTargetAccessorIndexMapList = null;
            if (mesh.MorphTargets.Any())
            {
                morphTargetAccessorIndexMapList = new List<Dictionary<string, int>>();
                foreach (var morphTarget in mesh.MorphTargets)
                {
                    var dict = new Dictionary<string, int>();

                    foreach (var kv in morphTarget.VertexBuffer)
                    {
                        if (option.removeTangent && kv.Key == VertexBuffer.TangentKey)
                        {
                            // remove tangent
                            continue;
                        }
                        if (option.removeMorphNormal && kv.Key == VertexBuffer.NormalKey)
                        {
                            // normal normal
                            continue;
                        }
                        if (kv.Value.Count != mesh.VertexBuffer.Count)
                        {
                            throw new Exception("inavlid data");
                        }
                        var accessorIndex = kv.Value.AddAccessorTo(storage, 0,
                        option.sparse,
                        kv.Key == VertexBuffer.PositionKey ? (Action<ArraySegment<byte>, glTFAccessor>)Vec3MinMax : null);
                        dict.Add(kv.Key, accessorIndex);
                    }

                    morphTargetAccessorIndexMapList.Add(dict);
                }
            }

            var drawCountOffset = 0;
            foreach (var y in mesh.Submeshes)
            {
                // index
                // slide index buffer accessor
                var indicesAccessorIndex = ExportIndices(storage, mesh.IndexBuffer, drawCountOffset, y.DrawCount, option);
                drawCountOffset += y.DrawCount;

                var prim = new glTFPrimitives
                {
                    mode = (int)mesh.Topology,
                    material = materials.IndexOf(y.Material),
                    indices = indicesAccessorIndex,
                    attributes = new glTFAttributes(),
                };
                gltfMesh.primitives.Add(prim);

                // attribute
                foreach (var kv in mesh.VertexBuffer)
                {
                    var attributeAccessorIndex = attributeAccessorIndexMap[kv.Key];

                    switch (kv.Key)
                    {
                        case VertexBuffer.PositionKey: prim.attributes.POSITION = attributeAccessorIndex; break;
                        case VertexBuffer.NormalKey: prim.attributes.NORMAL = attributeAccessorIndex; break;
                        case VertexBuffer.ColorKey: prim.attributes.COLOR_0 = attributeAccessorIndex; break;
                        case VertexBuffer.TexCoordKey: prim.attributes.TEXCOORD_0 = attributeAccessorIndex; break;
                        case VertexBuffer.TexCoordKey2: prim.attributes.TEXCOORD_1 = attributeAccessorIndex; break;
                        case VertexBuffer.JointKey: prim.attributes.JOINTS_0 = attributeAccessorIndex; break;
                        case VertexBuffer.WeightKey: prim.attributes.WEIGHTS_0 = attributeAccessorIndex; break;
                    }
                }

                // morph target
                if (mesh.MorphTargets.Any())
                {
                    foreach (var (t, accessorIndexMap) in
                        Enumerable.Zip(mesh.MorphTargets, morphTargetAccessorIndexMapList, (t, v) => (t, v)))
                    {
                        var target = new gltfMorphTarget();
                        prim.targets.Add(target);

                        foreach (var kv in t.VertexBuffer)
                        {
                            if (!accessorIndexMap.TryGetValue(kv.Key, out int targetAccessorIndex))
                            {
                                continue;
                            }
                            switch (kv.Key)
                            {
                                case VertexBuffer.PositionKey:
                                    target.POSITION = targetAccessorIndex;
                                    break;
                                case VertexBuffer.NormalKey:
                                    target.NORMAL = targetAccessorIndex;
                                    break;
                                case VertexBuffer.TangentKey:
                                    target.TANGENT = targetAccessorIndex;
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }

                }
            }

            // target name
            if (mesh.MorphTargets.Any())
            {
                gltf_mesh_extras_targetNames.Serialize(gltfMesh, mesh.MorphTargets.Select(z => z.Name));
            }
        }

        public static glTFMesh ExportMeshGroup(this MeshGroup src, List<object> materials, Vrm10Storage storage, ExportArgs option)
        {
            var mesh = new glTFMesh
            {
                name = src.Name
            };

            foreach (var x in src.Meshes)
            {
                // MeshとSubmeshがGltfのPrimitiveに相当する？
                x.ExportMesh(materials, storage, mesh, option);
            }

            return mesh;
        }
    }
}