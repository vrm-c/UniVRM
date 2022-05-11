using UniGLTF.M17N;
using UnityEditor;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    /// <summary>
    /// BlendShape の有無で Mesh を分割する
    /// </summary>
    public static class TabMeshSeparator
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
                    _isInvokeSuccess = TabMeshSeparator.Execute(root);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            return _isInvokeSuccess;
        }

        static bool Execute(GameObject root)
        {
            if (root == null)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_GAMEOBJECT_SELECTED.Msg(), "ok");
                return false;
            }

            if (root.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0)
            {
                EditorUtility.DisplayDialog("Failed", MeshProcessingMessages.NO_SKINNED_MESH.Msg(), "ok");
                return false;
            }

            // copy
            var outputObject = GameObject.Instantiate(root);
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
    }
}
