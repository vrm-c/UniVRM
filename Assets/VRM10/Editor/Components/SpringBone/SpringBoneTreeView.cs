using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace UniVRM10
{
    abstract class SelectedGUIBase
    {
        public abstract void Draw(Rect r);

        /// <summary>
        /// 領域を１行と残りに分割する
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        protected static (Rect line, Rect remain) LayoutLine(Rect rect)
        {
            return (
                new Rect(
                    rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight
                ),
                new Rect(
                    rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, rect.height - EditorGUIUtility.singleLineHeight
                )
            );
        }

        /// <summary>
        /// 領域を上下２分割
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        protected static (Rect layout, Rect remain) LayoutVerticalHalf(Rect r)
        {
            var half = r.height / 2;
            return (
                new Rect(
                    r.x, r.y, r.width, half
                ),
                new Rect(
                    r.x, r.y + half, r.width, r.height
                )
            );
        }
    }

    class SelectedColliderGroupGUI : SelectedGUIBase
    {
        SerializedProperty _selectedProp;
        ReorderableList _colliderGroupList;

        public SelectedColliderGroupGUI(SerializedObject so, int i)
        {
            _selectedProp = so.FindProperty($"SpringBone.ColliderGroups.Array.data[{i}]");
            var prop = _selectedProp.FindPropertyRelative("Colliders");
            _colliderGroupList = new ReorderableList(so, prop);

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
                        EditorUtility.SetDirty(element.objectReferenceValue);
                    }
                }
            };
        }

        public override void Draw(Rect r)
        {
            Rect layout = default;
            (layout, r) = LayoutLine(r);
            EditorGUI.PropertyField(layout, _selectedProp.FindPropertyRelative("Name"));

            (layout, r) = LayoutLine(r);
            GUI.Label(layout, "colliders");
            _colliderGroupList.DoList(r);
        }
    }

    class SelectedSpringGUI : SelectedGUIBase
    {
        SerializedProperty _selectedProp;
        ReorderableList _springColliderGroupList;
        ReorderableList _springJointList;

        public SelectedSpringGUI(SerializedObject so, int i)
        {
            _selectedProp = so.FindProperty($"SpringBone.Springs.Array.data[{i}]");

            {
                var prop = _selectedProp.FindPropertyRelative("ColliderGroups");
                _springColliderGroupList = new ReorderableList(so, prop);
            }

            {
                var prop = _selectedProp.FindPropertyRelative("Joints");
                _springJointList = new ReorderableList(so, prop);
                _springJointList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty element = prop.GetArrayElementAtIndex(index);
                    rect.height -= 4;
                    rect.y += 2;
                    EditorGUI.PropertyField(rect, element);
                };
            }
        }

        public override void Draw(Rect r)
        {
            Rect layout = default;
            (layout, r) = LayoutLine(r);
            EditorGUI.PropertyField(layout, _selectedProp.FindPropertyRelative("Name"));

            var (top, bottom) = LayoutVerticalHalf(r);

            (layout, r) = LayoutLine(top);
            GUI.Label(layout, "collider groups");
            _springColliderGroupList.DoList(r);

            (layout, r) = LayoutLine(bottom);
            GUI.Label(layout, "joints");
            _springJointList.DoList(r);
        }
    }

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
                var name = $"{i:00}:{colliderGroup.Name}";
                var id = _nextNodeID++;
                var item = new TreeViewItem(id, 2, name);
                _map.Add(id, colliderGroup);
                _colliderGroups.AddChild(item);
            }

            for (var i = 0; i < target.SpringBone.Springs.Count; ++i)
            {
                var spring = target.SpringBone.Springs[i];
                var name = $"{i:00}:{spring.Name}";
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

        SelectedGUIBase _selected;

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            _selected = null;
            if (selectedIds.Count > 0 && _map.TryGetValue(selectedIds[0], out object value))
            {
                if (value is VRM10ControllerSpringBone.ColliderGroup colliderGroup)
                {
                    var i = Target.SpringBone.ColliderGroups.IndexOf(colliderGroup);
                    _selected = new SelectedColliderGroupGUI(_so, i);
                }
                else if (value is VRM10ControllerSpringBone.Spring spring)
                {
                    var i = Target.SpringBone.Springs.IndexOf(spring);
                    _selected = new SelectedSpringGUI(_so, i);
                }
            }
        }

        const int WINDOW_HEIGHT = 500;
        const int TREE_WIDTH = 160;

        //      |
        // left | right
        //      |
        public void Draw()
        {
            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(WINDOW_HEIGHT));

            // left
            OnGUI(new Rect(r.x, r.y, TREE_WIDTH, r.height));

            // right
            if (_selected is SelectedColliderGroupGUI colliderGroup)
            {
                colliderGroup.Draw(new Rect(r.x + TREE_WIDTH, r.y, r.width - TREE_WIDTH, r.height));
            }
            else if (_selected is SelectedSpringGUI spring)
            {
                spring.Draw(new Rect(r.x + TREE_WIDTH, r.y, r.width - TREE_WIDTH, r.height));
            }
        }
    }
}
