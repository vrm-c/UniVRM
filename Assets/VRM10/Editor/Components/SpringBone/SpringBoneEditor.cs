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
        static SpringBoneTreeView GetTree(Vrm10Instance target, SerializedObject so)
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
        public static void Draw2D(Vrm10Instance target, SerializedObject so)
        {
            var tree = GetTree(target, so);
            if (GUILayout.Button("Reload"))
            {
                Disable();
                return;
            }

            tree.Draw2D();
        }

        /// <summary>
        /// 3D の Handle 描画
        /// </summary>
        public static void Draw3D(Vrm10Instance target, SerializedObject so)
        {
            var tree = GetTree(target, so);
            tree.Draw3D();
        }
    }
}
