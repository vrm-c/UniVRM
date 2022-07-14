using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using VRMShaders;

namespace UniGLTF
{
    internal static class MeshUploader
    {
        private const float FrameWeight = 100.0f;

        /// <summary>
        /// 頂点情報をMeshに対して送る
        /// </summary>
        public static void UploadMeshVertices(MeshData data, Mesh mesh)
        {
            var vertexAttributeDescriptor = MeshVertex.GetVertexAttributeDescriptor();

            // Weight情報等は存在しないパターンがあり、かつこの存在の有無によって内部的に条件分岐が走ってしまうため、
            // Streamを分けて必要に応じてアップロードする
            if (data.SkinnedMeshVertices.Length > 0)
            {
                vertexAttributeDescriptor = vertexAttributeDescriptor.Concat(SkinnedMeshVertex
                    .GetVertexAttributeDescriptor().Select(
                        attr =>
                        {
                            attr.stream = 1;
                            return attr;
                        })).ToArray();
            }

            mesh.SetVertexBufferParams(data.Vertices.Length, vertexAttributeDescriptor);

            mesh.SetVertexBufferData(data.Vertices, 0, 0, data.Vertices.Length);
            if (data.SkinnedMeshVertices.Length > 0)
            {
                mesh.SetVertexBufferData(data.SkinnedMeshVertices, 0, 0, data.SkinnedMeshVertices.Length, 1);
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

        private static async Task BuildBlendShapeAsync(IAwaitCaller awaitCaller, Mesh mesh, BlendShape blendShape,
            Vector3[] emptyVertices)
        {
            Vector3[] positions = null;
            Vector3[] normals = null;
            await awaitCaller.Run(() =>
            {
                positions = blendShape.Positions.ToArray();
                if (blendShape.Normals != null)
                {
                    normals = blendShape.Normals.ToArray();
                }
            });

            Profiler.BeginSample("MeshUploader.BuildBlendShapeAsync");
            if (blendShape.Positions.Count > 0)
            {
                if (blendShape.Positions.Count == mesh.vertexCount)
                {
                    mesh.AddBlendShapeFrame(blendShape.Name, FrameWeight,
                        blendShape.Positions.ToArray(),
                        normals.Length == mesh.vertexCount && normals.Length == positions.Length ? normals : null,
                        null
                    );
                }
                else
                {
                    Debug.LogWarningFormat(
                        "May be partial primitive has blendShape. Require separate mesh or extend blend shape, but not implemented: {0}",
                        blendShape.Name);
                }
            }
            else
            {
                // Debug.LogFormat("empty blendshape: {0}.{1}", mesh.name, blendShape.Name);
                // add empty blend shape for keep blend shape index
                mesh.AddBlendShapeFrame(blendShape.Name, FrameWeight,
                    emptyVertices,
                    null,
                    null
                );
            }

            Profiler.EndSample();
        }

        public static async Task<MeshWithMaterials> BuildMeshAndUploadAsync(
            IAwaitCaller awaitCaller,
            MeshData data,
            Func<int, Material> materialFromIndex)
        {
            Profiler.BeginSample("MeshUploader.BuildMesh");

            //Debug.Log(prims.ToJson());
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

            var result = new MeshWithMaterials
            {
                Mesh = mesh,
                Materials = data.MaterialIndices.Select(materialFromIndex).ToArray(),
                ShouldSetRendererNodeAsBone  = data.ShouldSetRendererNodeAsBone,
            };
            await awaitCaller.NextFrame();

            if (data.BlendShapes.Count > 0)
            {
                var emptyVertices = new Vector3[mesh.vertexCount];
                foreach (var blendShape in data.BlendShapes)
                {
                    await BuildBlendShapeAsync(awaitCaller, mesh, blendShape, emptyVertices);
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("Mesh.UploadMeshData");
            mesh.UploadMeshData(false);
            Profiler.EndSample();

            return result;
        }
    }
}
