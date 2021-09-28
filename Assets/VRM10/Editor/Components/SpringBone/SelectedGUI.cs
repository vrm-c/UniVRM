using System.Linq;
using UnityEditor;
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
        public static (Rect line, Rect remain) LayoutLine(Rect rect)
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
        public static (Rect layout, Rect remain) LayoutVerticalHalf(Rect r)
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

    class SelectedColliderGroupGUI
    {
        SerializedObject _so;
        int _index;
        ReorderableList _colliderGroupList;

        public SelectedColliderGroupGUI(SerializedObject so, int i)
        {
            var target_prop = so.FindProperty($"SpringBone.ColliderGroups.Array.data[{i}]");
            _index = i;
            _so = new SerializedObject(target_prop.objectReferenceValue);

            var prop = _so.FindProperty("Colliders");
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
                        EditorUtility.SetDirty(element.objectReferenceValue);
                    }
                }
            };
        }

        public void Draw2D(Rect r)
        {
            Rect layout = default;
            (layout, r) = SelectedGUIBase.LayoutLine(r);
            EditorGUI.PropertyField(layout, _so.FindProperty("Name"));

            (layout, r) = SelectedGUIBase.LayoutLine(r);
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

        public void Draw3D()
        {
            var target = _so.targetObject as VRM10SpringBoneColliderGroup;

            foreach (var c in target.Colliders)
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

        public SelectedSpringGUI(Vrm10Instance target, SerializedObject so, int i) : base(so, i)
        {
            Property = so.FindProperty($"SpringBone.Springs.Array.data[{i}]");

            {
                var prop = Property.FindPropertyRelative("ColliderGroups");
                _springColliderGroupList = new ReorderableList(so, prop);
                _springColliderGroupList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.height -= 4;
                    rect.y += 2;

                    SerializedProperty element = prop.GetArrayElementAtIndex(index);
                    var elements = target.SpringBone.ColliderGroups;
                    var element_index = elements.IndexOf(element.objectReferenceValue as VRM10SpringBoneColliderGroup);
                    var colliderGroups = target.SpringBone.ColliderGroups.Select((x, y) => x.GUIName(y)).ToArray();
                    var new_index = EditorGUI.Popup(rect, element_index, colliderGroups);
                    if (new_index != element_index)
                    {
                        element.objectReferenceValue = elements[new_index];
                    }
                };
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
}