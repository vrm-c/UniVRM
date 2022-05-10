using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public static class TabMeshSeparator
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
                    _isInvokeSuccess = TabMeshSeparator.Execute(_exportTarget);
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
            if (go.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 0)
            {
                // copy
                var outputObject = GameObject.Instantiate(go);
                outputObject.name = outputObject.name + "_mesh_separation";
                // 改変と asset の作成
                var list = MeshUtility.SeparationProcessing(outputObject);
                foreach (var (src, with, without) in list)
                {
                    // asset の永続化
                    MeshUtility.SaveMesh(src, with, MeshUtility.BlendShapeLogic.WithBlendShape);
                    MeshUtility.SaveMesh(src, without, MeshUtility.BlendShapeLogic.WithoutBlendShape);
                }
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_SKINNED_MESH.Msg(), "ok");
                return false;
            }
        }
    }
}
