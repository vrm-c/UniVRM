using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    public static class MeshImporterDivided
    {
        public static UnityEngine.Mesh LoadDivided(VrmLib.MeshGroup src)
        {
            var dst = new UnityEngine.Mesh();

            //
            // vertices
            //
            var vertexCount = src.Meshes.Sum(x => x.VertexBuffer.Count);
            var positions = new List<Vector3>(vertexCount);
            var normals = new List<Vector3>(vertexCount);
            var uv = new List<Vector2>(vertexCount);
            var boneWeights = new List<BoneWeight>(vertexCount);
            for (int meshIndex = 0; meshIndex < src.Meshes.Count; ++meshIndex)
            {
                var mesh = src.Meshes[meshIndex];
                positions.AddRange(mesh.VertexBuffer.Positions.GetSpan<Vector3>());
                normals.AddRange(mesh.VertexBuffer.Normals.GetSpan<Vector3>());
                uv.AddRange(mesh.VertexBuffer.TexCoords.GetSpan<Vector2>());
                if (src.Skin != null)
                {
                    var j = mesh.VertexBuffer.Joints.GetSpan<SkinJoints>();
                    var w = mesh.VertexBuffer.Weights.GetSpan<Vector4>();
                    for (int i = 0; i < mesh.VertexBuffer.Count; ++i)
                    {
                        var jj = j[i];
                        var ww = w[i];
                        boneWeights.Add(new BoneWeight
                        {
                            boneIndex0 = jj.Joint0,
                            boneIndex1 = jj.Joint1,
                            boneIndex2 = jj.Joint2,
                            boneIndex3 = jj.Joint3,
                            weight0 = ww.x,
                            weight1 = ww.y,
                            weight2 = ww.z,
                            weight3 = ww.w,
                        });
                    }
                }
            }

            dst.name = src.Name;
            dst.vertices = positions.ToArray();
            dst.normals = normals.ToArray();
            dst.uv = uv.ToArray();
            if (src.Skin != null)
            {
                dst.boneWeights = boneWeights.ToArray();
            }

            //
            // skin
            //
            if (src.Skin != null)
            {
                dst.bindposes = src.Skin.InverseMatrices.GetSpan<Matrix4x4>().ToArray();
            }

            //
            // triangles
            //
            dst.subMeshCount = src.Meshes.Count;
            var offset = 0;
            for (int meshIndex = 0; meshIndex < src.Meshes.Count; ++meshIndex)
            {
                var mesh = src.Meshes[meshIndex];
                var indices = mesh.IndexBuffer.GetAsIntArray().Select(x => offset + x).ToArray();
                dst.SetTriangles(indices, meshIndex);
                offset += mesh.VertexBuffer.Count;
            }

            dst.RecalculateBounds();
            dst.RecalculateTangents();

            //
            // blendshape
            //
            var blendShapeCount = src.Meshes[0].MorphTargets.Count;
            for (int i = 0; i < blendShapeCount; ++i)
            {
                positions.Clear();
                normals.Clear();
                var name = src.Meshes[0].MorphTargets[i].Name;
                for (int meshIndex = 0; meshIndex < src.Meshes.Count; ++meshIndex)
                {
                    var morphTarget = src.Meshes[meshIndex].MorphTargets[i];
                    positions.AddRange(morphTarget.VertexBuffer.Positions.GetSpan<Vector3>());
                    if (morphTarget.VertexBuffer.Normals != null)
                    {
                        normals.AddRange(morphTarget.VertexBuffer.Normals.GetSpan<Vector3>());
                    }
                    else
                    {
                        // fill zero
                        normals.AddRange(Enumerable.Range(0, morphTarget.VertexBuffer.Count).Select(x => Vector3.zero));
                    }
                }
                dst.AddBlendShapeFrame(name, 100.0f, positions.ToArray(), normals.ToArray(), null);
            }

            return dst;
        }
    }
}
