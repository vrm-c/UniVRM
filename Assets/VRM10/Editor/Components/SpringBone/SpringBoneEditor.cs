using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// TreeView でアクティブな SpringBone, ColliderGroup を管理して、
    /// アクティブな SpringBone と ColliderGroup を SceneHandle で Edit する。
    /// </summary>
    public static class SpringBoneEditor
    {
        static SpringBoneTreeView s_treeView;
        static SpringBoneTreeView GetTree(VRM10Controller target, SerializedObject so)
        {
            if (s_treeView == null || s_treeView.Target != target)
            {
                var state = new TreeViewState();
                s_treeView = new SpringBoneTreeView(state, target, so);
                s_treeView.Reload();
            }
            return s_treeView;
        }

        public static void Disable()
        {
            s_treeView = null;
        }

        /// <summary>
        /// 2D の GUI 描画
        /// </summary>
        public static void Draw2D(VRM10Controller target, SerializedObject so)
        {
            var tree = GetTree(target, so);
            if (GUILayout.Button("Reload"))
            {
                tree.Reload();
            }

            tree.Draw();
        }

        /// <summary>
        /// 3D の Handle 描画
        /// </summary>
        public static void Draw3D(VRM10Controller target)
        {
            if (target == null)
            {
                return;
            }
        }

        //         private void OnSceneGUI()
        //         {
        //             Undo.RecordObject(m_target, "VRMSpringBoneColliderGroupEditor");

        //             Handles.matrix = m_target.transform.localToWorldMatrix;
        //             Gizmos.color = Color.green;

        //             bool changed = false;

        //             foreach (var x in m_target.Colliders)
        //             {
        //                 var offset = Handles.PositionHandle(x.Offset, Quaternion.identity);
        //                 if (offset != x.Offset)
        //                 {
        //                     changed = true;
        //                     x.Offset = offset;
        //                 }
        //             }

        //             if (changed)
        //             {
        //                 EditorUtility.SetDirty(m_target);
        //             }
        //         }

    }
}
