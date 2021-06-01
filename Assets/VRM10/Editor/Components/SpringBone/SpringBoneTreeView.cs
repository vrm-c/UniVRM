using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace UniVRM10
{
    abstract class SelectedGUIBase
    {
        protected SerializedObject _so { get; private set; }
        protected int _index { get; private set; }

        protected SelectedGUIBase(SerializedObject so, int i)
        {
            _so = so;
            _index = i;
        }

        public SerializedProperty Property { get; protected set; }
        public abstract void Draw2D(Rect r);
        public abstract void Draw3D();

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
        ReorderableList _colliderGroupList;

        public SelectedColliderGroupGUI(SerializedObject so, int i) : base(so, i)
        {
            Property = so.FindProperty($"SpringBone.ColliderGroups.Array.data[{i}]");
            var prop = Property.FindPropertyRelative("Colliders");
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

        public override void Draw2D(Rect r)
        {
            Rect layout = default;
            (layout, r) = LayoutLine(r);
            EditorGUI.PropertyField(layout, Property.FindPropertyRelative("Name"));

            (layout, r) = LayoutLine(r);
            GUI.Label(layout, "colliders");
            _colliderGroupList.DoList(r);
        }

        public static void DrawWireCapsule(Vector3 headPos, Vector3 tailPos, float radius)
        {
            var headToTail = tailPos - headPos;
            if (headToTail.sqrMagnitude <= float.Epsilon)
            {
                Handles.DrawWireDisc(headPos, -SceneView.currentDrawingSceneView.camera.transform.forward, radius);
                return;
            }

            var forward = headToTail.normalized * radius;

            var xLen = Mathf.Abs(forward.x);
            var yLen = Mathf.Abs(forward.y);
            var zLen = Mathf.Abs(forward.z);
            var rightWorldAxis = (yLen > xLen && yLen > zLen) ? Vector3.right : Vector3.up;

            var up = Vector3.Cross(forward, rightWorldAxis).normalized * radius;
            var right = Vector3.Cross(up, forward).normalized * radius;

            const int division = 24;
            DrawWireCircle(headPos, up, right, division, division);
            DrawWireCircle(headPos, up, -forward, division, division / 2);
            DrawWireCircle(headPos, right, -forward, division, division / 2);

            DrawWireCircle(tailPos, up, right, division, division);
            DrawWireCircle(tailPos, up, forward, division, division / 2);
            DrawWireCircle(tailPos, right, forward, division, division / 2);

            Handles.DrawLine(headPos + right, tailPos + right);
            Handles.DrawLine(headPos - right, tailPos - right);
            Handles.DrawLine(headPos + up, tailPos + up);
            Handles.DrawLine(headPos - up, tailPos - up);
        }

        private static void DrawWireCircle(Vector3 centerPos, Vector3 xAxis, Vector3 yAxis, int division, int count)
        {
            for (var idx = 0; idx < division && idx < count; ++idx)
            {
                var s = ((idx + 0) % division) / (float)division * Mathf.PI * 2f;
                var t = ((idx + 1) % division) / (float)division * Mathf.PI * 2f;

                Gizmos.DrawLine(
                    centerPos + xAxis * Mathf.Cos(s) + yAxis * Mathf.Sin(s),
                    centerPos + xAxis * Mathf.Cos(t) + yAxis * Mathf.Sin(t)
                );
            }
        }

        public override void Draw3D()
        {
            var target = _so.targetObject as VRM10Controller;

            foreach (var c in target.SpringBone.ColliderGroups[_index].Colliders)
            {
                Handles.color = c.IsSelected ? Color.red : Color.cyan;

                Matrix4x4 mat = c.transform.localToWorldMatrix;
                Handles.matrix = mat * Matrix4x4.Scale(new Vector3(
                    1.0f / c.transform.lossyScale.x,
                    1.0f / c.transform.lossyScale.y,
                    1.0f / c.transform.lossyScale.z
                    ));
                switch (c.ColliderType)
                {
                    case VRM10SpringBoneColliderTypes.Sphere:
                        Handles.DrawWireDisc(c.Offset, -SceneView.currentDrawingSceneView.camera.transform.forward, c.Radius);
                        break;

                    case VRM10SpringBoneColliderTypes.Capsule:
                        DrawWireCapsule(c.Offset, c.Tail, c.Radius);
                        break;
                }

                if (c.IsSelected)
                {
                    Handles.color = Color.green;
                    EditorGUI.BeginChangeCheck();
                    Handles.matrix = c.transform.localToWorldMatrix;
                    var offset = Handles.PositionHandle(c.Offset, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(c, "VRM10SpringBoneCollider");
                        c.Offset = offset;
                        EditorUtility.SetDirty(_so.targetObject);
                    }
                }
            }
        }
    }

    class SelectedSpringGUI : SelectedGUIBase
    {
        ReorderableList _springColliderGroupList;
        ReorderableList _springJointList;

        public SelectedSpringGUI(SerializedObject so, int i) : base(so, i)
        {
            Property = so.FindProperty($"SpringBone.Springs.Array.data[{i}]");

            {
                var prop = Property.FindPropertyRelative("ColliderGroups");
                _springColliderGroupList = new ReorderableList(so, prop);
            }

            {
                var prop = Property.FindPropertyRelative("Joints");
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

        public override void Draw2D(Rect r)
        {
            Rect layout = default;
            (layout, r) = LayoutLine(r);
            EditorGUI.PropertyField(layout, Property.FindPropertyRelative("Name"));

            var (top, bottom) = LayoutVerticalHalf(r);

            (layout, r) = LayoutLine(top);
            GUI.Label(layout, "collider groups");
            _springColliderGroupList.DoList(r);

            (layout, r) = LayoutLine(bottom);
            GUI.Label(layout, "joints");
            _springJointList.DoList(r);
        }

        public override void Draw3D()
        {

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
        public void Draw2D()
        {
            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(WINDOW_HEIGHT));

            // left
            OnGUI(new Rect(r.x, r.y, TREE_WIDTH, r.height));

            // right
            if (_selected is SelectedColliderGroupGUI colliderGroup)
            {
                colliderGroup.Draw2D(new Rect(r.x + TREE_WIDTH, r.y, r.width - TREE_WIDTH, r.height));
            }
            else if (_selected is SelectedSpringGUI spring)
            {
                spring.Draw2D(new Rect(r.x + TREE_WIDTH, r.y, r.width - TREE_WIDTH, r.height));
            }
        }

        public void Draw3D()
        {
            if (_selected is SelectedColliderGroupGUI colliderGroup)
            {
                colliderGroup.Draw3D();
            }
            else if (_selected is SelectedSpringGUI spring)
            {
                // TODO
            }
        }
    }
}
