using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniVRM10
{
    public static class MeshLoader
    {
        public static void LoadMesh(this Mesh mesh, VrmLib.Mesh src, VrmLib.Skin skin = null)
        {
            mesh.vertices = src.VertexBuffer.Positions.GetSpan<Vector3>().ToArray();
            mesh.normals = src.VertexBuffer.Normals?.GetSpan<Vector3>().ToArray();
            mesh.uv = src.VertexBuffer.TexCoords?.GetSpan<Vector2>().ToArray();
            mesh.colors = src.VertexBuffer.Colors?.GetSpan<Color>().ToArray();
            if (src.VertexBuffer.Weights != null && src.VertexBuffer.Joints != null)
            {
                var boneWeights = new BoneWeight[mesh.vertexCount];
                if (src.VertexBuffer.Weights.Count != mesh.vertexCount || src.VertexBuffer.Joints.Count != mesh.vertexCount)
                {
                    throw new ArgumentException();
                }
                var weights = src.VertexBuffer.Weights.GetSpan<Vector4>();
                var joints = src.VertexBuffer.Joints.GetSpan<VrmLib.SkinJoints>();
                if (skin != null)
                {
                    mesh.bindposes = skin.InverseMatrices.GetSpan<Matrix4x4>().ToArray();
                }

                for (int i = 0; i < weights.Length; ++i)
                {
                    var w = weights[i];
                    boneWeights[i].weight0 = w.x;
                    boneWeights[i].weight1 = w.y;
                    boneWeights[i].weight2 = w.z;
                    boneWeights[i].weight3 = w.w;
                }
                for (int i = 0; i < joints.Length; ++i)
                {
                    var j = joints[i];
                    boneWeights[i].boneIndex0 = j.Joint0;
                    boneWeights[i].boneIndex1 = j.Joint1;
                    boneWeights[i].boneIndex2 = j.Joint2;
                    boneWeights[i].boneIndex3 = j.Joint3;
                }
                mesh.boneWeights = boneWeights;
            }

            mesh.subMeshCount = src.Submeshes.Count;
            var triangles = src.IndexBuffer.GetAsIntList();
            for (int i = 0; i < src.Submeshes.Count; ++i)
            {
                var submesh = src.Submeshes[i];
                mesh.SetTriangles(triangles.GetRange(submesh.Offset, submesh.DrawCount), i);
            }

            foreach (var morphTarget in src.MorphTargets)
            {
                var positions =
                    morphTarget.VertexBuffer.Positions != null
                    ? morphTarget.VertexBuffer.Positions.GetSpan<Vector3>().ToArray()
                    : new Vector3[mesh.vertexCount] // dummy
                    ;
                mesh.AddBlendShapeFrame(morphTarget.Name, 100.0f, positions, null, null);
            }

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
        }
    }
}
