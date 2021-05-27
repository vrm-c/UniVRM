using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UniVRM10
{
    public static class SpringBoneEditor
    {
        public class SpringBoneTreeView : TreeView
        {
            public VRM10Controller Target { get; private set; }
            TreeViewItem _root;
            TreeViewItem _colliderGroups;
            TreeViewItem _springs;

            int _nextNodeID = 0;

            public SpringBoneTreeView(TreeViewState state, VRM10Controller target) : base(state)
            {
                Target = target;

                _root = new TreeViewItem(_nextNodeID++, -1, "Root");
                var springBone = new TreeViewItem(_nextNodeID++, 0, "SpringBone");
                _root.AddChild(springBone);

                _colliderGroups = new TreeViewItem(_nextNodeID++, 1, "ColliderGroups");
                springBone.AddChild(_colliderGroups);

                _springs = new TreeViewItem(_nextNodeID++, 1, "Springs");
                springBone.AddChild(_springs);                
            }

            protected override TreeViewItem BuildRoot()
            {
                return _root;
            }
        }

        static SpringBoneTreeView s_treeView;
        static SpringBoneTreeView GetTree(VRM10Controller target)
        {
            if (s_treeView == null || s_treeView.Target != target)
            {
                var state = new TreeViewState();
                s_treeView = new SpringBoneTreeView(state, target);
                s_treeView.Reload();
            }
            return s_treeView;
        }


        /// <summary>
        /// 2D の GUI 描画
        /// </summary>
        public static void Draw2D(VRM10Controller target)
        {
            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(500));
            GetTree(target).OnGUI(r);
        }

        /// <summary>
        /// 3D の Handle 描画
        /// </summary>
        public static void Draw3D(VRM10Controller target)
        {

        }
    }
}
