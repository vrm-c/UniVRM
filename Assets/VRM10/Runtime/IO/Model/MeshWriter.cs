using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UniGLTF;
using VrmLib;

namespace UniVRM10
{
    /// <summary>
    /// VrmLib.MeshGroup => GLTF
    /// </summary>
    public static class MeshWriter
    {
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

        static void ExportMesh(this VrmLib.Mesh mesh, List<object> materials, Vrm10Storage storage, glTFMesh gltfMesh, ExportArgs option)
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
                    material = y.Material,
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
