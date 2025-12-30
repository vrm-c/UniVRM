using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UniGLTF
{
    internal static class MeshUploader
    {
        private const float FrameWeight = 100.0f;
        private const float EpsilonSqr = 1e-16f;

        private static bool HasAnyNonZero(Vector3[] delta)
        {
            if (delta == null) return false;
            for (int i = 0; i < delta.Length; i++)
            {
                if (delta[i].sqrMagnitude > EpsilonSqr) return true;
            }
            return false;
        }

        private static Vector3[] CalcDeltaNormalsForWeight(
            Vector3[] baseNormals,
            Vector3[] deltaNormalsAt100,
            float weight01)
        {
            var delta = new Vector3[baseNormals.Length];
            for (int i = 0; i < baseNormals.Length; i++)
            {
                var n = baseNormals[i] + deltaNormalsAt100[i] * weight01;
                var sqr = n.sqrMagnitude;
                if (sqr > float.Epsilon)
                {
                    n *= 1.0f / Mathf.Sqrt(sqr);
                }
                delta[i] = n - baseNormals[i];
            }
            return delta;
        }

        /// <summary>
        /// 頂点情報をMeshに対して送る
        /// </summary>
        public static void UploadMeshVertices(MeshData data, Mesh mesh)
        {
            MeshVertexUtility.SetVertexBufferParamsToMesh(mesh, data.Vertices0.Length, data.Vertices2.Length > 0);

            mesh.SetVertexBufferData(data.Vertices0, 0, 0, data.Vertices0.Length);
            mesh.SetVertexBufferData(data.Vertices1, 0, 0, data.Vertices0.Length, 1);
            if (data.Vertices2.Length > 0)
            {
                mesh.SetVertexBufferData(data.Vertices2, 0, 0, data.Vertices2.Length, 2);
            }
        }

        /// <summary>
        /// インデックス情報をMeshに対して送る
        /// </summary>
        /// <param name="mesh"></param>
        private static void UploadMeshIndices(MeshData data, Mesh mesh)
        {
            mesh.SetIndexBufferParams(data.Indices.Length, IndexFormat.UInt32);
            mesh.SetIndexBufferData(data.Indices, 0, 0, data.Indices.Length);
            mesh.subMeshCount = data.SubMeshes.Count;
            for (var i = 0; i < data.SubMeshes.Count; i++)
            {
                mesh.SetSubMesh(i, data.SubMeshes[i]);
            }
        }

        private static async Task BuildBlendShapeAsync(
            IAwaitCaller awaitCaller,
            Mesh mesh,
            BlendShape blendShape,
            Vector3[] emptyVertices,
            Vector3[] baseNormals)
        {
            Vector3[] positions = null;
            Vector3[] normals = null;
            await awaitCaller.Run(() =>
            {
                positions = blendShape.Positions != null ? blendShape.Positions.ToArray() : Array.Empty<Vector3>();
                normals = blendShape.Normals != null ? blendShape.Normals.ToArray() : Array.Empty<Vector3>();
            });

            Profiler.BeginSample("MeshUploader.BuildBlendShapeAsync");
            var hasPositions = positions.Length == mesh.vertexCount;
            var hasNormals = normals.Length == mesh.vertexCount;

            // Unity blendshape normal interpolation can look slightly off when vertex deltas are all-zero
            // (normal-only targets). Add a few intermediate frames with renormalized normals to keep the
            // interpolation closer to the intended (unit-length) normals across weights.
            if (hasNormals && !HasAnyNonZero(positions) && HasAnyNonZero(normals))
            {
                foreach (var frameWeight in new[] { 25.0f, 50.0f, 75.0f })
                {
                    var deltaNormals = CalcDeltaNormalsForWeight(baseNormals, normals, frameWeight / 100.0f);
                    mesh.AddBlendShapeFrame(blendShape.Name, frameWeight,
                        emptyVertices,
                        deltaNormals,
                        null
                    );
                }

                mesh.AddBlendShapeFrame(blendShape.Name, FrameWeight,
                    emptyVertices,
                    normals,
                    null
                );

                Profiler.EndSample();
                return;
            }

            if (positions.Length > 0)
            {
                if (hasPositions)
                {
                    var deltaNormals = hasNormals ? normals : null;

                    mesh.AddBlendShapeFrame(blendShape.Name, FrameWeight,
                        positions,
                        deltaNormals,
                        null
                    );
                }
                else
                {
                    UniGLTFLogger.Warning($"May be partial primitive has blendShape. Require separate mesh or extend blend shape, but not implemented: {blendShape.Name}");
                }
            }
            else
            {
                // add empty blend shape for keep blend shape index
                mesh.AddBlendShapeFrame(blendShape.Name, FrameWeight,
                    emptyVertices,
                    normals.Length == mesh.vertexCount ? normals : null,
                    null
                );
            }

            Profiler.EndSample();
        }

        public static async Task<MeshWithMaterials> BuildMeshAndUploadAsync(
            IAwaitCaller awaitCaller,
            MeshData data,
            Func<int?, Task<Material>> materialFromIndex)
        {
            var mesh = new Mesh
            {
                name = data.Name
            };

            UploadMeshVertices(data, mesh);
            await awaitCaller.NextFrame();

            UploadMeshIndices(data, mesh);
            await awaitCaller.NextFrame();

            // NOTE: mesh.vertices では自動的に行われていたが、SetVertexBuffer では行われないため、明示的に呼び出す.
            mesh.RecalculateBounds();
            await awaitCaller.NextFrame();

            if (!data.HasNormal)
            {
                mesh.RecalculateNormals();
                await awaitCaller.NextFrame();
            }

            mesh.RecalculateTangents();
            await awaitCaller.NextFrame();

            var materials = new Material[data.MaterialIndices.Count];
            for (var idx = 0; idx < data.MaterialIndices.Count; ++idx)
            {
                materials[idx] = await materialFromIndex(data.MaterialIndices[idx]);
            }

            var result = new MeshWithMaterials
            {
                Mesh = mesh,
                Materials = materials,
                ShouldSetRendererNodeAsBone = data.ShouldSetRendererNodeAsBone,
            };
            await awaitCaller.NextFrame();

            if (data.BlendShapes.Count > 0)
            {
                var baseNormals = mesh.normals;
                var emptyVertices = new Vector3[mesh.vertexCount];
                foreach (var blendShape in data.BlendShapes)
                {
                    await BuildBlendShapeAsync(
                        awaitCaller,
                        mesh,
                        blendShape,
                        emptyVertices,
                        baseNormals);
                }
            }

            Profiler.BeginSample("Mesh.UploadMeshData");
            mesh.UploadMeshData(false);
            Profiler.EndSample();

            return result;
        }
    }
}
