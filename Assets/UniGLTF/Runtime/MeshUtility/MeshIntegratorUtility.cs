using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class MeshIntegratorUtility
    {
        const string ASSET_SUFFIX = ".mesh.asset";

        public static string INTEGRATED_MESH_NAME => MeshIntegrator.INTEGRATED_MESH_NAME;
        public static string INTEGRATED_MESH_BLENDSHAPE_NAME => MeshIntegrator.INTEGRATED_MESH_BLENDSHAPE_NAME;


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

        public static void CopyAndReplaceWithResults(GameObject root, List<MeshIntegrationResult> results)
        {
            // copy hierarchy
            var outputObject = GameObject.Instantiate(root);
            outputObject.name = outputObject.name + "_mesh_integration";

            // destroy original meshes in the copied GameObject
            foreach (var skinnedMesh in outputObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                GameObject.DestroyImmediate(skinnedMesh);
            }
            foreach (var normalMesh in outputObject.GetComponentsInChildren<MeshFilter>())
            {
                if (normalMesh.sharedMesh.name != MeshIntegratorUtility.INTEGRATED_MESH_NAME)
                {
                    if (normalMesh.gameObject.GetComponent<MeshRenderer>())
                    {
                        GameObject.DestroyImmediate(normalMesh.gameObject.GetComponent<MeshRenderer>());
                    }
                    GameObject.DestroyImmediate(normalMesh);
                }
            }

            foreach (var result in results)
            {
                // Add integrated
                result.IntegratedRenderer.transform.SetParent(outputObject.transform, false);
                // save mesh data
                SaveMeshData(result.IntegratedRenderer.sharedMesh);
            }
        }

        static void SaveMeshData(Mesh mesh)
        {
            var assetPath = string.Format("{0}{1}", Path.GetFileNameWithoutExtension(mesh.name), ASSET_SUFFIX);
            Debug.Log(assetPath);
            if (!string.IsNullOrEmpty((AssetDatabase.GetAssetPath(mesh))))
            {
                var directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh)).Replace("\\", "/");
                assetPath = string.Format("{0}/{1}{2}", directory, Path.GetFileNameWithoutExtension(mesh.name), ASSET_SUFFIX);
            }
            else
            {
                assetPath = string.Format("Assets/{0}{1}", Path.GetFileNameWithoutExtension(mesh.name), ASSET_SUFFIX);
            }
            Debug.LogFormat("CreateAsset: {0}", assetPath);
            AssetDatabase.CreateAsset(mesh, assetPath);
        }
    }
}