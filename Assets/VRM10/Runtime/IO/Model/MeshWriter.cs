using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using Unity.Collections;
using VrmLib;

namespace UniVRM10
{
    /// <summary>
    /// VrmLib.MeshGroup => GLTF
    /// </summary>
    public static class MeshWriter
    {
        /// <summary>
        /// https://github.com/vrm-c/UniVRM/issues/800
        ///
        /// SubMesh 単位に分割する。
        /// SubMesh を Gltf の Primitive に対応させる。
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="materials"></param>
        /// <param name="data"></param>
        /// <param name="gltfMesh"></param>
        /// <param name="option"></param>
        static IEnumerable<glTFPrimitives> ExportMeshDivided(this VrmLib.Mesh mesh, List<object> materials,
            ExportingGltfData writer, ExportArgs option)
        {
            var usedIndices = new List<int>();
            var meshIndices = mesh.IndexBuffer.GetAsIntArray();
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
                var indices = meshIndices.GetSubArray(submesh.Offset, submesh.DrawCount).ToArray();
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
                        buffer.PushVertex(k, positions[k], normals[k], uv[k]);
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
                            buffer.PushBoneWeight(boneWeight);
                        }
                    }
                }
                var materialIndex = submesh.Material;
                var gltfPrimitive = buffer.ToGltfPrimitive(writer, materialIndex, indices);

                // blendShape
                for (int j = 0; j < mesh.MorphTargets.Count; ++j)
                {
                    var blendShape = new MeshExportUtil.BlendShapeBuffer(usedIndices.Count);

                    // index の順に attributes を蓄える
                    var morph = mesh.MorphTargets[j];
                    var blendShapePositions = morph.VertexBuffer.Positions.GetSpan<UnityEngine.Vector3>();
                    NativeArray<UnityEngine.Vector3>? blendShapeNormals = default;
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

                    gltfPrimitive.targets.Add(blendShape.ToGltf(writer, !option.removeMorphNormal, option.sparse));
                }

                yield return gltfPrimitive;
            }
        }

        /// <summary>
        /// ModelExporter.Export で作られた Model.MeshGroups[*] を GLTF 化する
        /// </summary>
        /// <param name="src"></param>
        /// <param name="materials"></param>
        /// <param name="data"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static glTFMesh ExportMeshGroup(this MeshGroup src, List<object> materials, ExportingGltfData writer, ExportArgs option)
        {
            var gltfMesh = new glTFMesh
            {
                name = src.Name
            };

            if (src.Meshes.Count != 1)
            {
                throw new NotImplementedException();
            }

            foreach (var prim in src.Meshes[0].ExportMeshDivided(materials, writer, option))
            {
                gltfMesh.primitives.Add(prim);
            }

            var targetNames = src.Meshes[0].MorphTargets.Select(x => x.Name).ToArray();
            gltf_mesh_extras_targetNames.Serialize(gltfMesh, targetNames, BlendShapeTargetNameLocationFlags.Mesh);

            return gltfMesh;
        }
    }
}
