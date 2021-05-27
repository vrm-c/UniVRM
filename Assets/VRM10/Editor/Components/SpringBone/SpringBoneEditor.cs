using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
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
            SerializedObject _so;

            TreeViewItem _root;
            TreeViewItem _colliderGroups;
            TreeViewItem _springs;

            int _nextNodeID = 0;

            Dictionary<int, object> _map = new Dictionary<int, object>();

            public SpringBoneTreeView(TreeViewState state, VRM10Controller target, SerializedObject so) : base(state)
            {
                Target = target;
                _so = so;

                _root = new TreeViewItem(_nextNodeID++, -1, "Root");
                var springBone = new TreeViewItem(_nextNodeID++, 0, "SpringBone");
                _root.AddChild(springBone);

                _colliderGroups = new TreeViewItem(_nextNodeID++, 1, "ColliderGroups");
                springBone.AddChild(_colliderGroups);

                _springs = new TreeViewItem(_nextNodeID++, 1, "Springs");
                springBone.AddChild(_springs);

                // load
                _map = new Dictionary<int, object>();
                for (var i = 0; i < target.SpringBone.ColliderGroups.Count; ++i)
                {
                    var colliderGroup = target.SpringBone.ColliderGroups[i];
                    var name = colliderGroup.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = $"colliderGroup{i:00}";
                    }
                    var id = _nextNodeID++;
                    var item = new TreeViewItem(id, 2, name);
                    _map.Add(id, colliderGroup);
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
                    var id = _nextNodeID++;
                    var item = new TreeViewItem(id, 2, name);
                    _map.Add(id, spring);
                    _springs.AddChild(item);
                }
            }

            protected override TreeViewItem BuildRoot()
            {
                return _root;
            }

            public ReorderableList Selected { get; private set; }

            ReorderableList _colliderGroupList;
            ReorderableList _springList;

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                if (selectedIds.Count > 0 && _map.TryGetValue(selectedIds[0], out object value))
                {
                    if (value is VRM10ControllerSpringBone.ColliderGroup colliderGroup)
                    {
                        var i = Target.SpringBone.ColliderGroups.IndexOf(colliderGroup);
                        var prop = _so.FindProperty($"SpringBone.ColliderGroups.Array.data[{i}].Colliders");
                        _colliderGroupList = new ReorderableList(_so, prop);

                        _colliderGroupList.drawElementCallback = (rect, index, isActive, isFocused) =>
                        {
                            SerializedProperty element = prop.GetArrayElementAtIndex(index);
                            rect.height -= 4;
                            rect.y += 2;
                            EditorGUI.PropertyField(rect, element);
                        };

                        Selected = _colliderGroupList;
                    }
                    else if (value is VRM10ControllerSpringBone.Spring spring)
                    {
                        var i = Target.SpringBone.Springs.IndexOf(spring);
                        var prop = _so.FindProperty($"SpringBone.Springs.Array.data[{i}].Joints");
                        _springList = new ReorderableList(_so, prop);

                        Selected = _springList;
                    }
                    else
                    {
                        Selected = null;
                    }
                }
                else
                {
                    Selected = null;
                }
            }
        }

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

        const int WINDOW_HEIGHT = 500;
        const int TREE_WIDTH = 160;
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

            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(WINDOW_HEIGHT));
            tree.OnGUI(new Rect(r.x, r.y, TREE_WIDTH, r.height));

            var ro = tree.Selected;
            if (ro != null)
            {
                ro.DoList(new Rect(r.x + TREE_WIDTH, r.y, r.width - TREE_WIDTH, r.height));
            }
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
