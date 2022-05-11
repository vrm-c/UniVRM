using System.IO;
using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabMeshIntegrator
    {
        const string ASSET_SUFFIX = ".mesh.asset";

        public static bool OnGUI(GameObject root)
        {
            var _isInvokeSuccess = false;
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                {
                    _isInvokeSuccess = TabMeshIntegrator.Execute(root);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return _isInvokeSuccess;
        }

        static string VRM_META = "VRMMeta";
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

        static bool Execute(GameObject root)
        {
            if (root == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg(), "ok");
                return false;
            }

            if (HasVrm(root))
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.VRM_DETECTED.Msg(), "ok");
                return false;
            }

            if (root.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0 && root.GetComponentsInChildren<MeshFilter>().Length == 0)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_MESH.Msg(), "ok");
                return false;
            }

            MeshIntegrator(root);
            return true;
        }

        /// <summary>
        /// from dialog
        /// </summary>
        /// <param name="go"></param>
        public static void MeshIntegrator(GameObject go)
        {
            MeshIntegratorUtility.Integrate(go, onlyBlendShapeRenderers: true);
            MeshIntegratorUtility.Integrate(go, onlyBlendShapeRenderers: false);

            var outputObject = GameObject.Instantiate(go);
            outputObject.name = outputObject.name + "_mesh_integration";
            var skinnedMeshes = outputObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            var normalMeshes = outputObject.GetComponentsInChildren<MeshFilter>();

            // destroy integrated meshes in the source
            // ?
            foreach (var skinnedMesh in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_NAME ||
                    skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_BLENDSHAPE_NAME)
                {
                    GameObject.DestroyImmediate(skinnedMesh.gameObject);
                }
            }
            foreach (var skinnedMesh in skinnedMeshes)
            {
                // destroy original meshes in the copied GameObject
                if (!(skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_NAME ||
                    skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_BLENDSHAPE_NAME))
                {
                    GameObject.DestroyImmediate(skinnedMesh);
                }
                // check if the integrated mesh is empty
                else if (skinnedMesh.sharedMesh.subMeshCount == 0)
                {
                    GameObject.DestroyImmediate(skinnedMesh.gameObject);
                }
                // save mesh data
                else if (skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_NAME ||
                         skinnedMesh.sharedMesh.name == MeshIntegratorUtility.INTEGRATED_MESH_BLENDSHAPE_NAME)
                {
                    SaveMeshData(skinnedMesh.sharedMesh);
                }
            }
            foreach (var normalMesh in normalMeshes)
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
