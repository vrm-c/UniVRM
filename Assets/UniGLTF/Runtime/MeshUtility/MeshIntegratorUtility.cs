using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class MeshIntegratorUtility
    {
        /// <summary>
        /// go を root としたヒエラルキーから Renderer を集めて、統合された Mesh 作成する
        /// </summary>
        /// <param name="go"></param>
        /// <param name="onlyBlendShapeRenderers">
        /// true: BlendShapeを保持するSkinnedMeshRendererのみ
        /// false: BlendShapeを保持しないSkinnedMeshRenderer + MeshRenderer
        /// null: すべてのSkinnedMeshRenderer + MeshRenderer
        /// </param>
        /// <returns></returns>
        public static MeshIntegrationResult Integrate(GameObject go, MeshEnumerateOption onlyBlendShapeRenderers,
            IEnumerable<Mesh> excludes = null,
            bool destroyIntegratedRenderer = false)
        {
            var exclude = new MeshExclude(excludes);

            var integrator = new MeshUtility.MeshIntegrator();

            switch (onlyBlendShapeRenderers)
            {
                case MeshEnumerateOption.OnlyWithBlendShape:
                    {
                        foreach (var x in EnumerateSkinnedMeshRenderer(go.transform, onlyBlendShapeRenderers))
                        {
                            if (exclude.IsExcluded(x))
                            {
                                continue;
                            }
                            integrator.Push(x);
                        }
                        break;
                    }

                case MeshEnumerateOption.OnlyWithoutBlendShape:
                    {
                        foreach (var x in EnumerateSkinnedMeshRenderer(go.transform, onlyBlendShapeRenderers))
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

                        break;
                    }

                case MeshEnumerateOption.All:
                    {
                        foreach (var x in EnumerateSkinnedMeshRenderer(go.transform, onlyBlendShapeRenderers))
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

                        break;
                    }
            }

            return integrator.Integrate(onlyBlendShapeRenderers);
        }

        public static IEnumerable<SkinnedMeshRenderer> EnumerateSkinnedMeshRenderer(Transform root, MeshEnumerateOption hasBlendShape)
        {
            foreach (var x in Traverse(root))
            {
                var renderer = x.GetComponent<SkinnedMeshRenderer>();
                if (renderer != null &&
                    renderer.gameObject.activeInHierarchy &&
                    renderer.sharedMesh != null &&
                    renderer.enabled)
                {
                    switch (hasBlendShape)
                    {
                        case MeshEnumerateOption.OnlyWithBlendShape:
                            if (renderer.sharedMesh.blendShapeCount > 0)
                            {
                                yield return renderer;
                            }
                            break;

                        case MeshEnumerateOption.OnlyWithoutBlendShape:
                            if (renderer.sharedMesh.blendShapeCount == 0)
                            {
                                yield return renderer;
                            }
                            break;

                        case MeshEnumerateOption.All:
                            {
                                yield return renderer;
                                break;
                            }
                    }
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

        public static void ReplaceMeshWithResults(GameObject copy, List<MeshIntegrationResult> results)
        {
            // destroy original meshes
            foreach (var skinnedMesh in copy.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                GameObject.DestroyImmediate(skinnedMesh);
            }
            foreach (var normalMesh in copy.GetComponentsInChildren<MeshFilter>())
            {
                if (normalMesh.gameObject.GetComponent<MeshRenderer>())
                {
                    GameObject.DestroyImmediate(normalMesh.gameObject.GetComponent<MeshRenderer>());
                }
                GameObject.DestroyImmediate(normalMesh);
            }

            // Add integrated
            foreach (var result in results)
            {
                result.IntegratedRenderer.transform.SetParent(copy.transform, false);
            }
        }
    }
}