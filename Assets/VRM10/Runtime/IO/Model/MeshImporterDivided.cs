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
            resultMesh.RecalculateBounds();
            resultMesh.RecalculateTangents();
            if (meshGroup.Meshes.Any(mesh => mesh.VertexBuffer.Normals == null))
            {
                resultMesh.RecalculateNormals();
            }

            // BlendShapeを更新
            var blendShapeCount = meshGroup.Meshes[0].MorphTargets.Count;

            for (var i = 0; i < blendShapeCount; ++i)
            {
                var positionsCount = 0;
                var normalsCount = 0;
                foreach (var mesh in meshGroup.Meshes)
                {
                    var morphTarget = mesh.MorphTargets[i];
                    positionsCount += morphTarget.VertexBuffer.Positions.Count;
                    normalsCount += morphTarget.VertexBuffer.Normals?.Count ?? morphTarget.VertexBuffer.Count;
                }

                using (var blendShapePositions = new NativeArray<Vector3>(positionsCount, Allocator.Temp))
                using (var blendShapeNormals = new NativeArray<Vector3>(normalsCount, Allocator.Temp))
                {

                    var blendShapePositionOffset = 0;
                    var blendShapeNormalOffset = 0;
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        var morphTarget = mesh.MorphTargets[i];


                        NativeArray<Vector3>.Copy(
                            morphTarget.VertexBuffer.Positions.Bytes.Reinterpret<Vector3>(1),
                            blendShapePositions.GetSubArray(blendShapePositionOffset, morphTarget.VertexBuffer.Positions.Count));

                        if (morphTarget.VertexBuffer.Normals != null)
                        {
                            // nullならdefault(0)のまま
                            NativeArray<Vector3>.Copy(
                             morphTarget.VertexBuffer.Normals.Bytes.Reinterpret<Vector3>(1),
                            blendShapeNormals.GetSubArray(blendShapeNormalOffset, morphTarget.VertexBuffer.Normals.Count));
                        }

                        blendShapePositionOffset += morphTarget.VertexBuffer.Positions.Count;
                        blendShapeNormalOffset += morphTarget.VertexBuffer.Normals?.Count ?? morphTarget.VertexBuffer.Count;
                    }

                    resultMesh.AddBlendShapeFrame(meshGroup.Meshes[0].MorphTargets[i].Name,
                                        100.0f,
                                        blendShapePositions.ToArray(),
                                        blendShapeNormals.ToArray(),
                                        null);
                }
            }

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

            //
            // https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#_accessor_componenttype
            //
            if (vertexCount < ushort.MaxValue)
            {
                // vertex buffer への index が ushort に収まる
                var indices = new NativeArray<ushort>(indexCount, Allocator.TempJob);
                disposables.Add(indices);
                var indexOffset = 0;
                var vertexOffset = 0;
                foreach (var mesh in meshGroup.Meshes)
                {
                    switch (mesh.IndexBuffer.ComponentType)
                    {
                        case AccessorValueType.BYTE:
                        case AccessorValueType.UNSIGNED_BYTE:
                        case AccessorValueType.FLOAT:
                            throw new NotImplementedException($"{mesh.IndexBuffer.ComponentType}");

                        case AccessorValueType.SHORT:
                        case AccessorValueType.UNSIGNED_SHORT:
                            {
                                // unsigned short -> unsigned short
                                var source = mesh.IndexBuffer.Bytes.Reinterpret<ushort>(1);
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
                                var source = mesh.IndexBuffer.Bytes.Reinterpret<uint>(1);
                                jobHandle = new CopyIndicesJobs.Uint2Ushort(
                                        (ushort)vertexOffset,
                                        source,
                                        new NativeSlice<ushort>(indices, indexOffset, mesh.IndexBuffer.Count))
                                    .Schedule(mesh.IndexBuffer.Count, 1, jobHandle);
                                break;
                            }

                        default:
                            throw new ArgumentException($"unknown index buffer type: {mesh.IndexBuffer.ComponentType}");
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
                // vertex buffer への index が ushort を超える
                var indices = new NativeArray<uint>(indexCount, Allocator.TempJob);
                disposables.Add(indices);
                var indexOffset = 0;
                var vertexOffset = 0;
                foreach (var mesh in meshGroup.Meshes)
                {
                    switch (mesh.IndexBuffer.ComponentType)
                    {
                        case AccessorValueType.BYTE:
                        case AccessorValueType.UNSIGNED_BYTE:
                        case AccessorValueType.FLOAT:
                            throw new NotImplementedException($"{mesh.IndexBuffer.ComponentType}");

                        case AccessorValueType.SHORT:
                        case AccessorValueType.UNSIGNED_SHORT:
                            {
                                // unsigned short -> unsigned int
                                var source = mesh.IndexBuffer.Bytes.Reinterpret<ushort>(1);
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
                                var source = mesh.IndexBuffer.Bytes.Reinterpret<uint>(1);
                                jobHandle = new CopyIndicesJobs.UInt2UInt(
                                        (uint)vertexOffset,
                                        source,
                                        new NativeSlice<uint>(indices, indexOffset, mesh.IndexBuffer.Count))
                                    .Schedule(mesh.IndexBuffer.Count, 1, jobHandle);
                                break;
                            }

                        default:
                            throw new ArgumentException($"unknown index buffer type: {mesh.IndexBuffer.ComponentType}");
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
            var vertices0 = new NativeArray<MeshVertex0>(vertexCount, Allocator.TempJob);
            var vertices1 = new NativeArray<MeshVertex1>(vertexCount, Allocator.TempJob);
            var vertices2 = new NativeArray<MeshVertex2>(vertexCount, Allocator.TempJob);
            disposables.Add(vertices0);
            disposables.Add(vertices1);
            disposables.Add(vertices2);

            var indexOffset = 0;
            JobHandle interleaveVertexJob = default;

            foreach (var mesh in meshGroup.Meshes)
            {
                var positions = mesh.VertexBuffer.Positions.Bytes.Reinterpret<Vector3>(1);
                var normals = mesh.VertexBuffer.Normals?.Bytes.Reinterpret<Vector3>(1) ?? default;
                var texCoords = mesh.VertexBuffer.TexCoords?.Bytes.Reinterpret<Vector2>(1) ?? default;
                var weights = mesh.VertexBuffer.Weights?.GetAsVector4Array() ?? default;
                var joints = mesh.VertexBuffer.Joints?.GetAsSkinJointsArray() ?? default;

                interleaveVertexJob = new InterleaveMeshVerticesJob(
                        new NativeSlice<MeshVertex0>(vertices0, indexOffset, mesh.VertexBuffer.Count),
                        new NativeSlice<MeshVertex1>(vertices1, indexOffset, mesh.VertexBuffer.Count),
                        new NativeSlice<MeshVertex2>(vertices2, indexOffset, mesh.VertexBuffer.Count),
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
            MeshVertexUtility.SetVertexBufferParamsToMesh(resultMesh, vertexCount);
            resultMesh.SetVertexBufferData(vertices0, 0, 0, vertexCount);
            resultMesh.SetVertexBufferData(vertices1, 0, 0, vertexCount, 1);
            resultMesh.SetVertexBufferData(vertices2, 0, 0, vertexCount, 2);

            // 各種バッファを破棄
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            Profiler.EndSample();
        }
    }
}