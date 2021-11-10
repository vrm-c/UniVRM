using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using VRMShaders;

namespace UniGLTF
{
    public class MeshImporter
    {
        private const float FrameWeight = 100.0f;

        private static bool HasSharedVertexBuffer(glTFMesh gltfMesh)
        {
            glTFAttributes lastAttributes = null;
            var sharedAttributes = true;
            foreach (var prim in gltfMesh.primitives)
            {
                if (lastAttributes != null && !prim.attributes.Equals(lastAttributes))
                {
                    sharedAttributes = false;
                    break;
                }

                lastAttributes = prim.attributes;
            }
            return sharedAttributes;
        }

        public MeshContext ReadMesh(GltfData data, int meshIndex, IAxisInverter inverter)
        {
            var gltfMesh = data.GLTF.meshes[meshIndex];

            var meshContext = new MeshContext(gltfMesh.name, meshIndex);
            if (HasSharedVertexBuffer(gltfMesh))
            {
                meshContext.ImportMeshSharingVertexBuffer(data, gltfMesh, inverter);
            }
            else
            {
                meshContext.ImportMeshIndependentVertexBuffer(data, gltfMesh, inverter);
            }

            meshContext.RenameBlendShape(gltfMesh);

            meshContext.DropUnusedVertices();

            return meshContext;
        }

        private static (Mesh, bool) _BuildMesh(MeshContext meshContext)
        {
            meshContext.AddDefaultMaterial();

            //Debug.Log(prims.ToJson());
            var mesh = new Mesh
            {
                name = meshContext.Name
            };

            if (meshContext.Positions.Count > ushort.MaxValue)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.vertices = meshContext.Positions.ToArray();
            var recalculateNormals = false;
            if (meshContext.Normals != null && meshContext.Normals.Count > 0)
            {
                mesh.normals = meshContext.Normals.ToArray();
            }
            else
            {
                recalculateNormals = true;
            }

            if (meshContext.UV.Count == mesh.vertexCount)
            {
                mesh.uv = meshContext.UV.ToArray();
            }
            if (meshContext.UV2.Count == mesh.vertexCount)
            {
                mesh.uv2 = meshContext.UV2.ToArray();
            }

            var recalculateTangents = true;
#if UNIGLTF_IMPORT_TANGENTS
            if (meshContext.Tangents.Length > 0)
            {
                mesh.tangents = meshContext.Tangents.ToArray();
                recalculateTangents = false;
            }
#endif

            if (meshContext.Colors.Count == mesh.vertexCount)
            {
                mesh.colors = meshContext.Colors.ToArray();
            }
            if (meshContext.BoneWeights.Count > 0)
            {
                mesh.boneWeights = meshContext.BoneWeights.ToArray();
            }
            mesh.subMeshCount = meshContext.SubMeshes.Count;
            for (var i = 0; i < meshContext.SubMeshes.Count; ++i)
            {
                mesh.SetTriangles(meshContext.SubMeshes[i], i);
            }

            if (recalculateNormals)
            {
                mesh.RecalculateNormals();
            }

            return (mesh, recalculateTangents);
        }

        private static async Task BuildBlendShapeAsync(IAwaitCaller awaitCaller, Mesh mesh, BlendShape blendShape, Vector3[] emptyVertices)
        {
            Vector3[] positions = null;
            Vector3[] normals = null;
            await awaitCaller.Run(() =>
            {
                positions = blendShape.Positions.ToArray();
                normals = blendShape.Normals.ToArray();
            });

            Profiler.BeginSample("MeshImporter.BuildBlendShapeAsync");
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
                    Debug.LogWarningFormat("May be partial primitive has blendShape. Require separate mesh or extend blend shape, but not implemented: {0}", blendShape.Name);
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

        public static async Task<MeshWithMaterials> BuildMeshAsync(IAwaitCaller awaitCaller, Func<int, Material> ctx, MeshContext meshContext)
        {
            Profiler.BeginSample("MeshImporter._BuildMesh");
            var (mesh, recalculateTangents) = _BuildMesh(meshContext);
            Profiler.EndSample();

            if (recalculateTangents)
            {
                await awaitCaller.NextFrame();
                mesh.RecalculateTangents();
                await awaitCaller.NextFrame();
            }

            // 先にすべてのマテリアルを作成済みなのでテクスチャーは生成済み。Resultを使ってよい
            var result = new MeshWithMaterials
            {
                Mesh = mesh,
                Materials = meshContext.MaterialIndices.Select(ctx).ToArray()
            };

            await awaitCaller.NextFrame();
            if (meshContext.BlendShapes.Count > 0)
            {
                var emptyVertices = new Vector3[mesh.vertexCount];
                foreach (var blendShape in meshContext.BlendShapes)
                {
                    await BuildBlendShapeAsync(awaitCaller, mesh, blendShape, emptyVertices);
                }
            }

            Profiler.BeginSample("Mesh.UploadMeshData");
            mesh.UploadMeshData(false);
            Profiler.EndSample();

            return result;
        }
    }
}
