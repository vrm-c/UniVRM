using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
using UnityEngine.Profiling;
using Mesh = VrmLib.Mesh;

namespace UniVRM10
{
    public static class MeshImporterDivided
    {
        public static UnityEngine.Mesh LoadDivided(VrmLib.MeshGroup src)
        {
            Profiler.BeginSample("MeshImporterDivided.LoadDivided");
            
            var dst = new UnityEngine.Mesh();
            if (src.Meshes.Sum(x => x.IndexBuffer.Count) > ushort.MaxValue)
            {
                dst.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            //
            // vertices
            //
            var vertexCount = src.Meshes.Sum(x => x.VertexBuffer.Count);
            var positions = new List<Vector3>(vertexCount);
            var normals = new List<Vector3>(vertexCount);
            var uv = new List<Vector2>(vertexCount);
            var boneWeights = new List<BoneWeight>(vertexCount);
            
            foreach (var mesh in src.Meshes)
            {
                positions.AddRange(mesh.VertexBuffer.Positions.GetSpan<Vector3>());
                normals.AddRange(mesh.VertexBuffer.Normals.GetSpan<Vector3>());
                uv.AddRange(mesh.VertexBuffer.TexCoords.GetSpan<Vector2>());
                if (src.Skin == null) continue;
                var joints = mesh.VertexBuffer.Joints.GetSpan<SkinJoints>();
                var weights = mesh.VertexBuffer.Weights.GetSpan<Vector4>();
                for (var i = 0; i < mesh.VertexBuffer.Count; ++i)
                {
                    var joint = joints[i];
                    var weight = weights[i];
                    boneWeights.Add(new BoneWeight
                    {
                        boneIndex0 = joint.Joint0,
                        boneIndex1 = joint.Joint1,
                        boneIndex2 = joint.Joint2,
                        boneIndex3 = joint.Joint3,
                        weight0 = weight.x,
                        weight1 = weight.y,
                        weight2 = weight.z,
                        weight3 = weight.w,
                    });
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
            for (var meshIndex = 0; meshIndex < src.Meshes.Count; ++meshIndex)
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
            for (var i = 0; i < blendShapeCount; ++i)
            {
                positions.Clear();
                normals.Clear();
                var name = src.Meshes[0].MorphTargets[i].Name;
                for (var meshIndex = 0; meshIndex < src.Meshes.Count; ++meshIndex)
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

            Profiler.EndSample();
            
            return dst;
        }
    }
}
