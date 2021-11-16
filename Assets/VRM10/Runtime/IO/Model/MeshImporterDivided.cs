using System;
using System.Collections.Generic;
using System.Linq;
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
    public static class MeshImporterDivided
    {
        public static Mesh LoadDivided(MeshGroup meshGroup)
        {
            Profiler.BeginSample("MeshImporterDivided.LoadDivided");

            var vertexCount = meshGroup.Meshes.Sum(mesh => mesh.VertexBuffer.Count);
            var indexCount = meshGroup.Meshes.Sum(mesh => mesh.IndexBuffer.Count);

            var resultMesh = new Mesh();

            // 頂点バッファ・BindPoseを構築して更新
            UpdateVerticesAndBindPose(meshGroup, vertexCount, resultMesh);

            // インデックスバッファを構築して更新
            UpdateIndices(meshGroup, vertexCount, indexCount, resultMesh);

            // SubMeshを更新
            resultMesh.subMeshCount = meshGroup.Meshes.Count;
            var indexOffset = 0;
            for (var i = 0; i < meshGroup.Meshes.Count; ++i)
            {
                var mesh = meshGroup.Meshes[i];
                resultMesh.SetSubMesh(i, new SubMeshDescriptor(indexOffset, mesh.IndexBuffer.Count));
                indexOffset += mesh.IndexBuffer.Count;
            }

            // 各種データを再構築
            Profiler.BeginSample("Bounds");
            resultMesh.RecalculateBounds();
            Profiler.EndSample();
            Profiler.BeginSample("Tangents");
            resultMesh.RecalculateTangents();
            Profiler.EndSample();

            // BlendShapeを更新
            Profiler.BeginSample("BlendShape");
            var blendShapeCount = meshGroup.Meshes[0].MorphTargets.Count;
            var blendShapePositions = new List<Vector3>();
            var blendShapeNormals = new List<Vector3>();
            for (var i = 0; i < blendShapeCount; ++i)
            {
                blendShapePositions.Clear();
                blendShapeNormals.Clear();
                var name = meshGroup.Meshes[0].MorphTargets[i].Name;
                foreach (var mesh in meshGroup.Meshes)
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
                        blendShapeNormals.AddRange(Enumerable.Range(0, morphTarget.VertexBuffer.Count)
                            .Select(x => Vector3.zero));
                    }
                }

                resultMesh.AddBlendShapeFrame(name, 100.0f, blendShapePositions.ToArray(), blendShapeNormals.ToArray(),
                    null);
            }
            Profiler.EndSample();

            Profiler.EndSample();

            return resultMesh;
        }

        /// <summary>
        /// インデックスバッファを更新する
        /// MEMO: 出力に対するushortを考慮することをやめればかなりシンプルに書ける
        /// </summary>
        private static void UpdateIndices(MeshGroup meshGroup, int vertexCount, int indexCount, Mesh resultMesh)
        {
            Profiler.BeginSample("MeshImporterDivided.UpdateIndices");

            JobHandle jobHandle = default;

            var disposables = new List<IDisposable>();

            // 出力をushortにするべきかどうかを判別
            if (vertexCount < ushort.MaxValue)
            {
                var indices = new NativeArray<ushort>(indexCount, Allocator.TempJob);
                disposables.Add(indices);
                var indexOffset = 0;
                var vertexOffset = 0;
                foreach (var mesh in meshGroup.Meshes)
                {
                    switch (mesh.IndexBuffer.ComponentType)
                    {
                        case AccessorValueType.SHORT:
                        {
                            // unsigned short -> unsigned short
                            var source = mesh.IndexBuffer.AsNativeArray<ushort>(Allocator.TempJob);
                            disposables.Add(source);
                            jobHandle = new CopyIndicesJobs.Ushort2Ushort(
                                    (ushort)vertexOffset,
                                    new NativeSlice<ushort>(source),
                                    new NativeSlice<ushort>(indices, indexOffset, mesh.IndexBuffer.Count))
                                .Schedule(mesh.IndexBuffer.Count, 1, jobHandle);
                            break;
                        }
                        case AccessorValueType.UNSIGNED_INT:
                        {
                            // unsigned int -> unsigned short
                            var source = mesh.IndexBuffer.AsNativeArray<uint>(Allocator.TempJob);
                            disposables.Add(source);
                            jobHandle = new CopyIndicesJobs.Uint2Ushort(
                                    (ushort)vertexOffset,
                                    source,
                                    new NativeSlice<ushort>(indices, indexOffset, mesh.IndexBuffer.Count))
                                .Schedule(mesh.IndexBuffer.Count, 1, jobHandle);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    vertexOffset += mesh.VertexBuffer.Count;
                    indexOffset += mesh.IndexBuffer.Count;
                }

                jobHandle.Complete();

                resultMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
                resultMesh.SetIndexBufferData(indices, 0, 0, indexCount);
            }
            else
            {
                var indices = new NativeArray<uint>(indexCount, Allocator.TempJob);
                disposables.Add(indices);
                var indexOffset = 0;
                var vertexOffset = 0;
                foreach (var mesh in meshGroup.Meshes)
                {
                    switch (mesh.IndexBuffer.ComponentType)
                    {
                        case AccessorValueType.SHORT:
                        {
                            // unsigned short -> unsigned int
                            var source = mesh.IndexBuffer.AsNativeArray<ushort>(Allocator.TempJob);
                            disposables.Add(source);
                            jobHandle = new CopyIndicesJobs.Ushort2Uint(
                                    (uint)vertexOffset,
                                    source,
                                    new NativeSlice<uint>(indices, indexOffset, mesh.IndexBuffer.Count))
                                .Schedule(mesh.IndexBuffer.Count, 1, jobHandle);
                            break;
                        }
                        case AccessorValueType.UNSIGNED_INT:
                        {
                            // unsigned int -> unsigned int
                            var source = mesh.IndexBuffer.AsNativeArray<uint>(Allocator.TempJob);
                            disposables.Add(source);
                            jobHandle = new CopyIndicesJobs.UInt2UInt(
                                    (uint)vertexOffset,
                                    source,
                                    new NativeSlice<uint>(indices, indexOffset, mesh.IndexBuffer.Count))
                                .Schedule(mesh.IndexBuffer.Count, 1, jobHandle);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    vertexOffset += mesh.VertexBuffer.Count;
                    indexOffset += mesh.IndexBuffer.Count;
                }

                jobHandle.Complete();

                resultMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
                resultMesh.SetIndexBufferData(indices, 0, 0, indexCount);
            }

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// メッシュの頂点情報の更新を行う際、MainThreadが空くため、その間にBindPoseの更新も行う
        /// </summary>
        private static void UpdateVerticesAndBindPose(
            MeshGroup meshGroup,
            int vertexCount,
            Mesh resultMesh)
        {
            Profiler.BeginSample("MeshImporterDivided.UpdateVerticesAndBindPose");

            var disposables = new List<IDisposable>();

            // JobのSchedule
            var vertices = new NativeArray<MeshVertex>(vertexCount, Allocator.TempJob);
            disposables.Add(vertices);

            var indexOffset = 0;
            JobHandle interleaveVertexJob = default;

            foreach (var mesh in meshGroup.Meshes)
            {
                var positions = mesh.VertexBuffer.Positions.AsNativeArray<Vector3>(Allocator.TempJob);
                var normals = mesh.VertexBuffer.Normals.AsNativeArray<Vector3>(Allocator.TempJob);
                var texCoords = mesh.VertexBuffer.TexCoords.AsNativeArray<Vector2>(Allocator.TempJob);
                var weights = meshGroup.Skin != null
                    ? mesh.VertexBuffer.Weights.AsNativeArray<Vector4>(Allocator.TempJob)
                    : default;
                var joints = meshGroup.Skin != null
                    ? mesh.VertexBuffer.Joints.AsNativeArray<SkinJoints>(Allocator.TempJob)
                    : default;
                if (positions.IsCreated) disposables.Add(positions);
                if (normals.IsCreated) disposables.Add(normals);
                if (texCoords.IsCreated) disposables.Add(texCoords);
                if (weights.IsCreated) disposables.Add(weights);
                if (joints.IsCreated) disposables.Add(joints);

                interleaveVertexJob = new InterleaveMeshVerticesJob(
                        new NativeSlice<MeshVertex>(vertices, indexOffset, mesh.VertexBuffer.Count),
                        positions,
                        normals,
                        texCoords,
                        default,
                        weights,
                        joints)
                    .Schedule(mesh.VertexBuffer.Count, 1, interleaveVertexJob);
                indexOffset += mesh.VertexBuffer.Count;
            }

            JobHandle.ScheduleBatchedJobs();

            // 並行してBindposeの更新を行う
            if (meshGroup.Skin != null)
            {
                resultMesh.bindposes = meshGroup.Skin.InverseMatrices.GetSpan<Matrix4x4>().ToArray();
            }

            // Jobを完了
            interleaveVertexJob.Complete();

            // VertexBufferを設定
            MeshVertex.SetVertexBufferParamsToMesh(resultMesh, vertices.Length);
            resultMesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            // 各種バッファを破棄
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            Profiler.EndSample();
        }
    }
}