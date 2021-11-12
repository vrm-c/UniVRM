using System;
using UniGLTF;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace UniVRM10
{
    public static class MeshImporter
    {
        /// <summary>
        /// VrmLib.Mesh => UnityEngine.Mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="src"></param>
        /// <param name="skin"></param>
        public static Mesh LoadSharedMesh(VrmLib.Mesh src, VrmLib.Skin skin = null)
        {
            Profiler.BeginSample("MeshImporter.LoadSharedMesh");
            var mesh = new Mesh();
            if (src.IndexBuffer.Count > ushort.MaxValue)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            var positions = src.VertexBuffer.Positions.GetNativeArray<Vector3>(Allocator.TempJob);
            var normals = src.VertexBuffer.Normals?.GetNativeArray<Vector3>(Allocator.TempJob) ?? default;
            var texCoords = src.VertexBuffer.TexCoords?.GetNativeArray<Vector2>(Allocator.TempJob) ?? default;
            var colors = src.VertexBuffer.Colors?.GetNativeArray<Color>(Allocator.TempJob) ?? default;
            var weights = src.VertexBuffer.Weights?.GetNativeArray<Vector4>(Allocator.TempJob) ?? default;
            var joints = src.VertexBuffer.Joints?.GetNativeArray<SkinJoints>(Allocator.TempJob) ?? default;

            var vertices = new NativeArray<MeshVertex>(positions.Length, Allocator.TempJob);
            
            // JobとBindPoseの更新を並行して行う
            var jobHandle =
                new InterleaveMeshVerticesJob(vertices, positions, normals, texCoords, colors, weights, joints)
                    .Schedule(vertices.Length, 1);
            
            if (weights.IsCreated && joints.IsCreated)
            {
                if (weights.Length != positions.Length || joints.Length != positions.Length)
                {
                    throw new ArgumentException();
                }
                if (skin != null)
                {
                    mesh.bindposes = skin.InverseMatrices.GetSpan<Matrix4x4>().ToArray();
                }
            }

            // Jobを完了
            jobHandle.Complete();
            
            // 入力のNativeArrayを開放
            positions.Dispose();
            if (normals.IsCreated) normals.Dispose();
            if (texCoords.IsCreated) texCoords.Dispose();
            if (colors.IsCreated) colors.Dispose();
            if (weights.IsCreated) weights.Dispose();
            if (joints.IsCreated) joints.Dispose();
            
            // 頂点を更新
            MeshVertex.SetVertexBufferParamsToMesh(mesh, vertices.Length);
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            // 出力のNativeArrayを開放
            vertices.Dispose();

            // submesh 方式
            mesh.subMeshCount = src.Submeshes.Count;
            var triangles = src.IndexBuffer.GetAsIntList();
            for (var i = 0; i < src.Submeshes.Count; ++i)
            {
                var submesh = src.Submeshes[i];
                mesh.SetTriangles(triangles.GetRange(submesh.Offset, submesh.DrawCount), i);
            }

            foreach (var morphTarget in src.MorphTargets)
            {
                var morphTargetPositions =
                    morphTarget.VertexBuffer.Positions != null
                    ? morphTarget.VertexBuffer.Positions.GetSpan<Vector3>().ToArray()
                    : new Vector3[mesh.vertexCount] // dummy
                    ;
                mesh.AddBlendShapeFrame(morphTarget.Name, 100.0f, morphTargetPositions, null, null);
            }

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            
            Profiler.EndSample();

            return mesh;
        }
    }
}
