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

        /// <summary>
        /// https://github.com/vrm-c/UniVRM/issues/800
        ///
        /// SubMesh 単位に分割する。
        /// SubMesh を Gltf の Primitive に対応させる。
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="materials"></param>
        /// <param name="storage"></param>
        /// <param name="gltfMesh"></param>
        /// <param name="option"></param>
        static IEnumerable<glTFPrimitives> ExportMeshDivided(this VrmLib.Mesh mesh, List<object> materials, Vrm10Storage storage, ExportArgs option)
        {
            var bufferIndex = 0;
            var usedIndices = new List<int>();
            var meshIndices = SpanLike.CopyFrom(mesh.IndexBuffer.GetAsIntArray());
            var positions = mesh.VertexBuffer.Positions.GetSpan<UnityEngine.Vector3>().ToArray();
            var normals = mesh.VertexBuffer.Normals.GetSpan<UnityEngine.Vector3>().ToArray();
            var uv = mesh.VertexBuffer.TexCoords.GetSpan<UnityEngine.Vector2>().ToArray();
            var hasSkin = mesh.VertexBuffer.Weights != null;
            var weights = mesh.VertexBuffer.Weights?.GetSpan<UnityEngine.Vector4>().ToArray();
            var joints = mesh.VertexBuffer.Joints?.GetSpan<SkinJoints>().ToArray();
            Func<int, int> getJointIndex = default;
            if (hasSkin)
            {
                getJointIndex = i =>
                {
                    return i;
                };
            }

            foreach (var submesh in mesh.Submeshes)
            {
                var indices = meshIndices.Slice(submesh.Offset, submesh.DrawCount).ToArray();
                var hash = new HashSet<int>(indices);

                // mesh
                // index の順に attributes を蓄える
                var buffer = new MeshExportUtil.VertexBuffer(indices.Length, getJointIndex);
                usedIndices.Clear();
                for (int k = 0; k < positions.Length; ++k)
                {
                    if (hash.Contains(k))
                    {
                        // indices から参照される頂点だけを蓄える
                        usedIndices.Add(k);
                        buffer.Push(k, positions[k], normals[k], uv[k]);
                        if (getJointIndex != null)
                        {
                            var j = joints[k];
                            var w = weights[k];
                            var boneWeight = new UnityEngine.BoneWeight
                            {
                                boneIndex0 = j.Joint0,
                                boneIndex1 = j.Joint1,
                                boneIndex2 = j.Joint2,
                                boneIndex3 = j.Joint3,
                                weight0 = w.x,
                                weight1 = w.y,
                                weight2 = w.z,
                                weight3 = w.w,
                            };
                            buffer.Push(boneWeight);
                        }
                    }
                }
                var materialIndex = submesh.Material;
                var gltfPrimitive = buffer.ToGltfPrimitive(storage.Gltf, bufferIndex, materialIndex, indices);

                // blendShape
                for (int j = 0; j < mesh.MorphTargets.Count; ++j)
                {
                    var blendShape = new MeshExportUtil.BlendShapeBuffer(usedIndices.Count);

                    // index の順に attributes を蓄える
                    var morph = mesh.MorphTargets[j];
                    var blendShapePositions = morph.VertexBuffer.Positions.GetSpan<UnityEngine.Vector3>();
                    SpanLike<UnityEngine.Vector3>? blendShapeNormals = default;
                    if (morph.VertexBuffer.Normals != null)
                    {
                        blendShapeNormals = morph.VertexBuffer.Normals.GetSpan<UnityEngine.Vector3>();
                    }
                    int l = 0;
                    foreach (var k in usedIndices)
                    {
                        blendShape.Set(l++,
                            blendShapePositions[k],
                            blendShapeNormals.HasValue ? blendShapeNormals.Value[k] : UnityEngine.Vector3.zero
                            );
                    }

                    gltfPrimitive.targets.Add(blendShape.ToGltf(storage.Gltf, bufferIndex, !option.removeMorphNormal, option.sparse));
                }

                yield return gltfPrimitive;
            }
        }

        /// <summary>
        /// ModelExporter.Export で作られた Model.MeshGroups[*] を GLTF 化する
        /// </summary>
        /// <param name="src"></param>
        /// <param name="materials"></param>
        /// <param name="storage"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static glTFMesh ExportMeshGroup(this MeshGroup src, List<object> materials, Vrm10Storage storage, ExportArgs option)
        {
            var gltfMesh = new glTFMesh
            {
                name = src.Name
            };

            if (src.Meshes.Count != 1)
            {
                throw new NotImplementedException();
            }

            foreach (var prim in src.Meshes[0].ExportMeshDivided(materials, storage, option))
            {
                gltfMesh.primitives.Add(prim);
            }

            var targetNames = src.Meshes[0].MorphTargets.Select(x => x.Name).ToArray();
            gltf_mesh_extras_targetNames.Serialize(gltfMesh, targetNames);

            return gltfMesh;
        }
    }
}
