using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class MeshIntegratorUtility
    {
        public const string INTEGRATED_MESH_NAME = "MeshesIntegrated";
        public const string INTEGRATED_MESH_BLENDSHAPE_NAME = "MeshesBlendShapeIntegrated";
        /// <summary>
        /// go を root としたヒエラルキーから Renderer を集めて、統合された Mesh 作成する
        /// </summary>
        /// <param name="go"></param>
        /// <param name="onlyBlendShapeRenderers">BlendShapeを保持するSkinnedMeshRendererのみ/BlendShapeを保持しないSkinnedMeshRenderer + MeshRenderer</param>
        /// <returns></returns>
        public static MeshIntegrationResult Integrate(GameObject go, bool onlyBlendShapeRenderers)
        {
            var result = new MeshIntegrationResult();

            var meshNode = new GameObject();
            if (onlyBlendShapeRenderers)
            {
                meshNode.name = "MeshIntegrator(BlendShape)";
            }
            else
            {
                meshNode.name = "MeshIntegrator";
            }
            meshNode.transform.SetParent(go.transform, false);

            // レンダラから情報を集める
            var integrator = new MeshUtility.MeshIntegrator();

            if (onlyBlendShapeRenderers)
            {
                foreach (var x in EnumerateSkinnedMeshRenderer(go.transform, true))
                {
                    integrator.Push(x);
                    result.SourceSkinnedMeshRenderers.Add(x);
                }
            }
            else
            {
                foreach (var x in EnumerateSkinnedMeshRenderer(go.transform, false))
                {
                    integrator.Push(x);
                    result.SourceSkinnedMeshRenderers.Add(x);
                }

                foreach (var x in EnumerateMeshRenderer(go.transform))
                {
                    integrator.Push(x);
                    result.SourceMeshRenderers.Add(x);
                }
            }

            var mesh = new Mesh();

            if (integrator.Positions.Count > ushort.MaxValue)
            {
                Debug.LogFormat("exceed 65535 vertices: {0}", integrator.Positions.Count);
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.vertices = integrator.Positions.ToArray();
            mesh.normals = integrator.Normals.ToArray();
            mesh.uv = integrator.UV.ToArray();
            mesh.tangents = integrator.Tangents.ToArray();
            mesh.boneWeights = integrator.BoneWeights.ToArray();
            mesh.subMeshCount = integrator.SubMeshes.Count;
            for (var i = 0; i < integrator.SubMeshes.Count; ++i)
            {
                mesh.SetIndices(integrator.SubMeshes[i].Indices.ToArray(), MeshTopology.Triangles, i);
            }
            mesh.bindposes = integrator.BindPoses.ToArray();

            if (onlyBlendShapeRenderers)
            {
                integrator.AddBlendShapesToMesh(mesh);
                mesh.name = INTEGRATED_MESH_BLENDSHAPE_NAME;
            }
            else
            {
                mesh.name = INTEGRATED_MESH_NAME;
            }

            var integrated = meshNode.AddComponent<SkinnedMeshRenderer>();
            integrated.sharedMesh = mesh;
            integrated.sharedMaterials = integrator.SubMeshes.Select(x => x.Material).ToArray();
            integrated.bones = integrator.Bones.ToArray();
            result.IntegratedRenderer = integrated;

            return result;
        }

        public static IEnumerable<SkinnedMeshRenderer> EnumerateSkinnedMeshRenderer(Transform root, bool hasBlendShape)
        {
            foreach (var x in Traverse(root))
            {
                var renderer = x.GetComponent<SkinnedMeshRenderer>();
                if (renderer != null &&
                    renderer.gameObject.activeInHierarchy &&
                    renderer.sharedMesh != null &&
                    renderer.enabled &&
                    renderer.sharedMesh.blendShapeCount > 0 == hasBlendShape)
                {
                    yield return renderer;
                }
            }
        }

        public static IEnumerable<MeshRenderer> EnumerateMeshRenderer(Transform root)
        {
            foreach (var x in Traverse(root))
            {
                var renderer = x.GetComponent<MeshRenderer>();
                var filter = x.GetComponent<MeshFilter>();

                if (renderer != null &&
                    filter != null &&
                    renderer.gameObject.activeInHierarchy &&
                    filter.sharedMesh != null)
                {
                    yield return renderer;
                }
            }
        }

        private static IEnumerable<Transform> Traverse(Transform parent)
        {
            if (parent.gameObject.activeSelf)
            {
                yield return parent;

                foreach (Transform child in parent)
                {
                    foreach (var x in Traverse(child))
                    {
                        yield return x;
                    }
                }
            }
        }
    }
}