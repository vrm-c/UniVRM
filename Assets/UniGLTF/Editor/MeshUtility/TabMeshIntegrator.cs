using System.Collections.Generic;
using System.IO;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabMeshIntegrator
    {
        public static bool TryExecutable(GameObject root, out string msg)
        {
            // check
            if (root == null)
            {
                msg = MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg();
                return false;
            }

            if (HasVrm(root))
            {
                msg = MeshProcessingMessages.VRM_DETECTED.Msg();
                return false;
            }

            if (root.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0 && root.GetComponentsInChildren<MeshFilter>().Length == 0)
            {
                msg = MeshProcessingMessages.NO_MESH.Msg();
                return false;
            }

            msg = "";
            return true;
        }

        const string VRM_META = "VRMMeta";
        static bool HasVrm(GameObject root)
        {
            var allComponents = root.GetComponents(typeof(Component));
            foreach (var component in allComponents)
            {
                if (component == null) continue;
                var sourceString = component.ToString();
                if (sourceString.Contains(VRM_META))
                {
                    return true;
                }
            }
            return false;
        }

        const string ASSET_SUFFIX = ".mesh.asset";

        static string GetMeshWritePath(Mesh mesh)
        {
            if (!string.IsNullOrEmpty((AssetDatabase.GetAssetPath(mesh))))
            {
                var directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh)).Replace("\\", "/");
                return $"{directory}/{Path.GetFileNameWithoutExtension(mesh.name)}{ASSET_SUFFIX}";
            }
            else
            {
                return $"Assets/{Path.GetFileNameWithoutExtension(mesh.name)}{ASSET_SUFFIX}";
            }
        }

        /// <param name="src">GameObject instance in scene or prefab</param>
        public static bool Execute(GameObject src, bool onlyBlendShapeRenderers)
        {
            var results = new List<MeshIntegrationResult>();

            // instance or prefab => copy
            var copy = GameObject.Instantiate(src);
            copy.name = copy.name + "_mesh_integration";

            // integrate
            if (onlyBlendShapeRenderers)
            {
                results.Add(MeshIntegratorUtility.Integrate(copy, onlyBlendShapeRenderers: MeshEnumerateOption.OnlyWithBlendShape));
                results.Add(MeshIntegratorUtility.Integrate(copy, onlyBlendShapeRenderers: MeshEnumerateOption.OnlyWithoutBlendShape));
            }
            else
            {
                results.Add(MeshIntegratorUtility.Integrate(copy, onlyBlendShapeRenderers: MeshEnumerateOption.All));
            }

            // replace
            MeshIntegratorUtility.ReplaceMeshWithResults(copy, results);

            // write mesh asset.
            foreach (var result in results)
            {
                var mesh = result.IntegratedRenderer.sharedMesh;
                var assetPath = GetMeshWritePath(mesh);
                Debug.LogFormat("CreateAsset: {0}", assetPath);
                AssetDatabase.CreateAsset(mesh, assetPath);
            }

            if (src.GetGameObjectType() == GameObjectType.AssetPrefab)
            {
                // write prefab.
                {
                    var prefabPath = UnityPath.FromAsset(src);
                    prefabPath = prefabPath.Parent.Child($"{prefabPath.FileNameWithoutExtension}_integrated.prefab");
                    Debug.LogFormat("WritePrefab: {0}", prefabPath);
                    PrefabUtility.SaveAsPrefabAsset(copy, prefabPath.Value);
                }

                // destroy copy in scene.
                GameObject.DestroyImmediate(copy);
            }
            else
            {
                // do nothing. keep copy.
            }

            return true;
        }
    }
}
