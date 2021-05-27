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

                // load
                for (var i = 0; i < target.SpringBone.ColliderGroups.Count; ++i)
                {
                    var colliderGroup = target.SpringBone.ColliderGroups[i];
                    var name = colliderGroup.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = $"colliderGroup{i:00}";
                    }
                    var item = new TreeViewItem(_nextNodeID++, 2, name);
                    _colliderGroups.AddChild(item);
                }
                for (var i = 0; i < target.SpringBone.Springs.Count; ++i)
                {
                    var spring = target.SpringBone.Springs[i];
                    var name = spring.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = spring.Joints[0].Transform.name;
                    }
                    var item = new TreeViewItem(_nextNodeID++, 2, name);
                    _springs.AddChild(item);
                }
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
            if (GUILayout.Button("Reload"))
            {
                GetTree(target).Reload();
            }

            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(500));
            GetTree(target).OnGUI(r);
        }

        /// <summary>
        /// 3D の Handle 描画
        /// </summary>
        public static void Draw3D(VRM10Controller target)
        {

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
