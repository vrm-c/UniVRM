using System;
using UniGLTF;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using VrmLib;
using Mesh = UnityEngine.Mesh;

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

            var positions = src.VertexBuffer.Positions.GetAsNativeArray<Vector3>(Allocator.TempJob);
            var normals = src.VertexBuffer.Normals?.GetAsNativeArray<Vector3>(Allocator.TempJob) ?? default;
            var texCoords = src.VertexBuffer.TexCoords?.GetAsNativeArray<Vector2>(Allocator.TempJob) ?? default;
            var colors = src.VertexBuffer.Colors?.GetAsNativeArray<Color>(Allocator.TempJob) ?? default;
            var weights = src.VertexBuffer.Weights?.GetAsNativeArray<Vector4>(Allocator.TempJob) ?? default;
            var joints = src.VertexBuffer.Joints?.GetAsNativeArray<SkinJoints>(Allocator.TempJob) ?? default;

            var vertices = new NativeArray<MeshVertex>(positions.Length, Allocator.TempJob);
            
            // JobとBindPoseの更新を並行して行う
            var jobHandle =
                new InterleaveMeshVerticesJob(vertices, positions, normals, texCoords, colors, weights, joints)
                    .Schedule(vertices.Length, 1);
            
            // BindPoseを更新
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

            // Indexを更新
            switch (src.IndexBuffer.ComponentType)
            {
                case AccessorValueType.UNSIGNED_SHORT:
                    var shortIndices = src.IndexBuffer.GetAsNativeArray<short>(Allocator.Temp);
                    mesh.SetIndexBufferParams(shortIndices.Length, IndexFormat.UInt16);
                    mesh.SetIndexBufferData(shortIndices, 0, 0, shortIndices.Length);
                    shortIndices.Dispose();
                    break;
                case AccessorValueType.UNSIGNED_INT:
                    var intIndices = src.IndexBuffer.GetAsNativeArray<int>(Allocator.Temp);
                    mesh.SetIndexBufferParams(intIndices.Length, IndexFormat.UInt32);
                    mesh.SetIndexBufferData(intIndices, 0, 0, intIndices.Length);
                    intIndices.Dispose();
                    break;
                default:
                    throw new NotImplementedException();
            }

            // SubMeshを更新
            mesh.subMeshCount = src.Submeshes.Count;
            for (var i = 0; i < src.Submeshes.Count; ++i)
            {
                var subMesh = src.Submeshes[i];
                mesh.SetSubMesh(i, new SubMeshDescriptor(subMesh.Offset, subMesh.DrawCount));
            }

            // MorphTargetを更新
            foreach (var morphTarget in src.MorphTargets)
            {
                var morphTargetPositions =
                    morphTarget.VertexBuffer.Positions != null
                    ? morphTarget.VertexBuffer.Positions.GetSpan<Vector3>().ToArray()
                    : new Vector3[mesh.vertexCount] // dummy
                    ;
                mesh.AddBlendShapeFrame(morphTarget.Name, 100.0f, morphTargetPositions, null, null);
            }

            // 各種パラメーターを再計算
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            
            Profiler.EndSample();

            return mesh;
        }
    }
}
