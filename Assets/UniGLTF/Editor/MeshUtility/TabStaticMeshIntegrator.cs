using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabStaticMeshIntegrator
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
                    _isInvokeSuccess = TabStaticMeshIntegrator.Execute(_exportTarget);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return _isInvokeSuccess;
        }

        static bool Execute(GameObject _exportTarget)
        {
            if (_exportTarget == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg(), "ok");
                return false;
            }
            var go = _exportTarget;

            if (go.GetComponentsInChildren<MeshFilter>().Length > 0)
            {
                MeshUtility.IntegrateSelected(go);
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_STATIC_MESH.Msg(), "ok");
                return false;
            }
        }
    }
}
