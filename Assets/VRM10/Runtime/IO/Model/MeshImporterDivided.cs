using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace UniVRM10
{
    public static class MeshImporterDivided
    {
        public static Mesh LoadDivided(VrmLib.MeshGroup src)
        {
            Profiler.BeginSample("MeshImporterDivided.LoadDivided");
            
            var dst = new UnityEngine.Mesh();
            if (src.Meshes.Sum(x => x.IndexBuffer.Count) > ushort.MaxValue)
            {
                dst.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            // 頂点バッファを構築
            var vertexCount = src.Meshes.Sum(x => x.VertexBuffer.Count);
            var vertices = new NativeArray<MeshVertex>(vertexCount, Allocator.TempJob);

            var jobDisposables = new List<IDisposable>();
            
            // JobのSchedule
            JobHandle jobHandle = default;
            var indexOffset = 0;
            foreach (var mesh in src.Meshes)
            {
                // これらのNativeArrayはJobによって開放される
                var positions = mesh.VertexBuffer.Positions.AsNativeArray<Vector3>(Allocator.TempJob);
                var normals = mesh.VertexBuffer.Normals.AsNativeArray<Vector3>(Allocator.TempJob);
                var texCoords = mesh.VertexBuffer.TexCoords.AsNativeArray<Vector2>(Allocator.TempJob);
                var weights = src.Skin != null ? mesh.VertexBuffer.Weights.AsNativeArray<Vector4>(Allocator.TempJob) : default;
                var joints = src.Skin != null ? mesh.VertexBuffer.Joints.AsNativeArray<SkinJoints>(Allocator.TempJob) : default;
                if (positions.IsCreated) jobDisposables.Add(positions);
                if (normals.IsCreated) jobDisposables.Add(normals);
                if (texCoords.IsCreated) jobDisposables.Add(texCoords);
                if (weights.IsCreated) jobDisposables.Add(weights);
                if (joints.IsCreated) jobDisposables.Add(joints);

                jobHandle = new InterleaveMeshVerticesJob(vertices, positions, normals, texCoords, default, weights, joints, indexOffset)
                    .Schedule(mesh.VertexBuffer.Count, 1, jobHandle);
                indexOffset += mesh.VertexBuffer.Count;
            }

            // Jobと並行してBindposeの更新を行う
            if (src.Skin != null)
            {
                dst.bindposes = src.Skin.InverseMatrices.GetSpan<Matrix4x4>().ToArray();
            }
            
            // Jobを完了
            jobHandle.Complete();
            
            foreach (var disposable in jobDisposables)
            {
                disposable.Dispose();
            }

            MeshVertex.SetVertexBufferParamsToMesh(dst, vertices.Length);
            dst.SetVertexBufferData(vertices, 0, 0, vertices.Length);
            
            vertices.Dispose();

            // triangles
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

            // blendshape
            var blendShapeCount = src.Meshes[0].MorphTargets.Count;
            var blendShapePositions = new List<Vector3>();
            var blendShapeNormals = new List<Vector3>();
            for (var i = 0; i < blendShapeCount; ++i)
            {
                blendShapePositions.Clear();
                blendShapeNormals.Clear();
                var name = src.Meshes[0].MorphTargets[i].Name;
                foreach (var mesh in src.Meshes)
                {
                    var morphTarget = mesh.MorphTargets[i];
                    blendShapePositions.AddRange(morphTarget.VertexBuffer.Positions.GetSpan<Vector3>());
                    if (morphTarget.VertexBuffer.Normals != null)
                    {
                        blendShapeNormals.AddRange(morphTarget.VertexBuffer.Normals.GetSpan<Vector3>());
                    }
                    else
                    {
                        // fill zero
                        blendShapeNormals.AddRange(Enumerable.Range(0, morphTarget.VertexBuffer.Count).Select(x => Vector3.zero));
                    }
                }
                dst.AddBlendShapeFrame(name, 100.0f, blendShapePositions.ToArray(), blendShapeNormals.ToArray(), null);
            }

            Profiler.EndSample();
            
            return dst;
        }
    }
}
