using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace UniVRM10
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
                    name = spring.Joints[0].transform.name;
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

        object _selected;

        ReorderableList _colliderGroupList;
        ReorderableList _springColliderGroupList;
        ReorderableList _springJointList;
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count > 0 && _map.TryGetValue(selectedIds[0], out object value))
            {
                _selected = value;
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

                        if (isFocused)
                        {
                            var id = element.objectReferenceValue.GetInstanceID();
                            if (id != VRM10SpringBoneCollider.SelectedGuid)
                            {
                                VRM10SpringBoneCollider.SelectedGuid = id;
                                SceneView.RepaintAll();
                            }
                        }
                    };
                }
                else if (value is VRM10ControllerSpringBone.Spring spring)
                {
                    var i = Target.SpringBone.Springs.IndexOf(spring);

                    {
                        var prop = _so.FindProperty($"SpringBone.Springs.Array.data[{i}].ColliderGroups");
                        _springColliderGroupList = new ReorderableList(_so, prop);
                    }

                    {
                        var prop = _so.FindProperty($"SpringBone.Springs.Array.data[{i}].Joints");
                        _springJointList = new ReorderableList(_so, prop);
                        _springJointList.drawElementCallback = (rect, index, isActive, isFocused) =>
                        {
                            SerializedProperty element = prop.GetArrayElementAtIndex(index);
                            rect.height -= 4;
                            rect.y += 2;
                            EditorGUI.PropertyField(rect, element);
                        };
                    }

                }
            }
            else
            {
                _selected = null;
            }
        }

        const int WINDOW_HEIGHT = 500;
        const int TREE_WIDTH = 160;

        static void LabelList(Rect r, string label, ReorderableList l)
        {
            GUI.Label(new Rect(r.x, r.y, r.width, EditorGUIUtility.singleLineHeight), label);
            l.DoList(new Rect(r.x, r.y + EditorGUIUtility.singleLineHeight, r.width, r.height - EditorGUIUtility.singleLineHeight));
        }

        //      |
        // left | right
        //      |
        public void Draw()
        {
            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(WINDOW_HEIGHT));

            // left
            OnGUI(new Rect(r.x, r.y, TREE_WIDTH, r.height));

            // right
            if (_selected is VRM10ControllerSpringBone.ColliderGroup colliderGroup)
            {
                LabelList(new Rect(r.x + TREE_WIDTH, r.y, r.width - TREE_WIDTH, r.height), "colliders", _colliderGroupList);
            }
            else if (_selected is VRM10ControllerSpringBone.Spring spring)
            {
                LabelList(new Rect(r.x + TREE_WIDTH, r.y, r.width - TREE_WIDTH, r.height / 2), "collider groups",
                _springColliderGroupList);
                LabelList(new Rect(r.x + TREE_WIDTH, r.y + r.height / 2, r.width - TREE_WIDTH, r.height / 2), "joints", _springJointList);
            }
        }
    }
}
