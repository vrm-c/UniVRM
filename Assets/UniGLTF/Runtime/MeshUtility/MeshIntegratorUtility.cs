using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class MeshIntegratorUtility
    {
        public static string INTEGRATED_MESH_NAME => MeshIntegrator.INTEGRATED_MESH_NAME;
        public static string INTEGRATED_MESH_BLENDSHAPE_NAME => MeshIntegrator.INTEGRATED_MESH_BLENDSHAPE_NAME;

        /// <summary>
        /// go を root としたヒエラルキーから Renderer を集めて、統合された Mesh 作成する
        /// </summary>
        /// <param name="go"></param>
        /// <param name="onlyBlendShapeRenderers">BlendShapeを保持するSkinnedMeshRendererのみ/BlendShapeを保持しないSkinnedMeshRenderer + MeshRenderer</param>
        /// <returns></returns>
        public static MeshIntegrationResult Integrate(GameObject go, bool onlyBlendShapeRenderers, IEnumerable<Mesh> excludes = null)
        {
            var exclude = new MeshExclude(excludes);

            var integrator = new MeshUtility.MeshIntegrator();

            if (onlyBlendShapeRenderers)
            {
                foreach (var x in EnumerateSkinnedMeshRenderer(go.transform, true))
                {
                    if (exclude.IsExcluded(x))
                    {
                        continue;
                    }
                    integrator.Push(x);
                }
            }
            else
            {
                foreach (var x in EnumerateSkinnedMeshRenderer(go.transform, false))
                {
                    if (exclude.IsExcluded(x))
                    {
                        continue;
                    }
                    integrator.Push(x);
                }

                foreach (var x in EnumerateMeshRenderer(go.transform))
                {
                    if (exclude.IsExcluded(x))
                    {
                        continue;
                    }
                    integrator.Push(x);
                }
            }

            integrator.Intgrate(onlyBlendShapeRenderers);
            integrator.Result.IntegratedRenderer.transform.SetParent(go.transform, false);
            return integrator.Result;
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