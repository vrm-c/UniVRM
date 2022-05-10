using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabMeshIntegrator
    {
        public static bool OnGUI(GameObject _exportTarget)
        {
            var _isInvokeSuccess = false;
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Process", GUILayout.MinWidth(100)))
                {
                    _isInvokeSuccess = TabMeshIntegrator.Execute(_exportTarget);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return _isInvokeSuccess;
        }

        static string VRM_META = "VRMMeta";
        static bool HasVrm(GameObject go)
        {
            var allComponents = go.GetComponents(typeof(Component));
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

        static bool Execute(GameObject _exportTarget)
        {
            if (_exportTarget == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg(), "ok");
                return false;
            }
            var go = _exportTarget;

            if (HasVrm(go))
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.VRM_DETECTED.Msg(), "ok");
                return false;
            }

            if (go.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0 && go.GetComponentsInChildren<MeshFilter>().Length == 0)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_MESH.Msg(), "ok");
                return false;
            }

            MeshUtility.MeshIntegrator(go);
            return true;
        }
    }
}
