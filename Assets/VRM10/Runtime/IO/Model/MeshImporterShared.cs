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
    public static class MeshImporterShared
    {
        /// <summary>
        /// VrmLib.Mesh => UnityEngine.Mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="src"></param>
        /// <param name="skin"></param>
        public static Mesh LoadSharedMesh(VrmLib.Mesh src, Skin skin = null)
        {
            Profiler.BeginSample("MeshImporterShared.LoadSharedMesh");
            var mesh = new Mesh();

            var positions = src.VertexBuffer.Positions.Bytes.Reinterpret<Vector3>(1);
            var normals = src.VertexBuffer.Normals?.Bytes.Reinterpret<Vector3>(1) ?? default;
            var texCoords = src.VertexBuffer.TexCoords?.Bytes.Reinterpret<Vector2>(1) ?? default;
            NativeArray<Color> colors = default;
            if (src.VertexBuffer.Colors is BufferAccessor colorBuffer)
            {
                if (colorBuffer.ComponentType == AccessorValueType.FLOAT && colorBuffer.AccessorType == AccessorVectorType.VEC4)
                {
                    colors = colorBuffer.Bytes.Reinterpret<Color>(1);
                }
                else
                {
                    Debug.LogWarning($"COLOR_0: {colorBuffer.ComponentType}.{colorBuffer.AccessorType} not supported. skip.");
                }
            }
            var weights = src.VertexBuffer.Weights?.GetAsVector4Array() ?? default;
            var joints = src.VertexBuffer.Joints?.GetAsSkinJointsArray() ?? default;

            using (var vertices0 = new NativeArray<MeshVertex0>(positions.Length, Allocator.TempJob))
            using (var vertices1 = new NativeArray<MeshVertex1>(positions.Length, Allocator.TempJob))
            using (var vertices2 = new NativeArray<MeshVertex2>(positions.Length, Allocator.TempJob))
            {
                // JobとBindPoseの更新を並行して行う
                var jobHandle =
                    new InterleaveMeshVerticesJob(
                            vertices0,
                            vertices1,
                            vertices2,
                            positions,
                            normals,
                            texCoords,
                            colors,
                            weights,
                            joints
                        )
                        .Schedule(vertices0.Length, 1);
                JobHandle.ScheduleBatchedJobs();

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

                // 頂点を更新
                MeshVertexUtility.SetVertexBufferParamsToMesh(mesh, vertices0.Length);
                mesh.SetVertexBufferData(vertices0, 0, 0, vertices0.Length);
                mesh.SetVertexBufferData(vertices1, 0, 0, vertices0.Length, 1);
                mesh.SetVertexBufferData(vertices2, 0, 0, vertices0.Length, 2);

                // 出力のNativeArrayを開放
            }

            // Indexを更新
            switch (src.IndexBuffer.ComponentType)
            {
                case AccessorValueType.UNSIGNED_BYTE:
                {
                    var intIndices = src.IndexBuffer.GetAsIntArray();
                    mesh.SetIndexBufferParams(intIndices.Length, IndexFormat.UInt32);
                    mesh.SetIndexBufferData(intIndices, 0, 0, intIndices.Length);
                    break;
                }
                case AccessorValueType.UNSIGNED_SHORT:
                {
                    var shortIndices = src.IndexBuffer.Bytes.Reinterpret<ushort>(1);
                    mesh.SetIndexBufferParams(shortIndices.Length, IndexFormat.UInt16);
                    mesh.SetIndexBufferData(shortIndices, 0, 0, shortIndices.Length);
                    break;
                }
                case AccessorValueType.UNSIGNED_INT:
                {
                    var intIndices = src.IndexBuffer.Bytes.Reinterpret<uint>(1);
                    mesh.SetIndexBufferParams(intIndices.Length, IndexFormat.UInt32);
                    mesh.SetIndexBufferData(intIndices, 0, 0, intIndices.Length);
                    break;
                }
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
            if (src.VertexBuffer.Normals == null)
            {
                mesh.RecalculateNormals();
            }

            Profiler.EndSample();

            return mesh;
        }
    }
}