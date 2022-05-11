using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabMeshIntegrator
    {
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

            MeshUtility.MeshIntegrator(root);
            return true;
        }
    }
}
