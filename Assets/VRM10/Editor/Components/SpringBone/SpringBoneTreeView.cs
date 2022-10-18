using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace UniVRM10
{
    public class SpringBoneTreeView : TreeView
    {
        public Vrm10Instance Target { get; private set; }
        SerializedObject _so;

        TreeViewItem _root;
        TreeViewItem _colliderGroups;
        TreeViewItem _springs;

        int _nextNodeID = 0;

        Dictionary<int, object> _map = new Dictionary<int, object>();

        public SpringBoneTreeView(TreeViewState state, Vrm10Instance target, SerializedObject so) : base(state)
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
            if (target?.SpringBone?.ColliderGroups != null)
            {
                for (var i = 0; i < target.SpringBone.ColliderGroups.Count; ++i)
                {
                    var colliderGroup = target.SpringBone.ColliderGroups[i];
                    var name = colliderGroup.GUIName(i);
                    var id = _nextNodeID++;
                    var item = new TreeViewItem(id, 2, name);
                    _map.Add(id, colliderGroup);
                    _colliderGroups.AddChild(item);
                }
            }

            if (target?.SpringBone?.Springs != null)
            {
                for (var i = 0; i < target.SpringBone.Springs.Count; ++i)
                {
                    var spring = target.SpringBone.Springs[i];
                    var name = spring.GUIName(i);
                    var id = _nextNodeID++;
                    var item = new TreeViewItem(id, 2, name);
                    _map.Add(id, spring);
                    _springs.AddChild(item);
                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            return _root;
        }

        object _selected;

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            _selected = null;
            if (selectedIds.Count > 0 && _map.TryGetValue(selectedIds[0], out object value))
            {
                if (value is VRM10SpringBoneColliderGroup colliderGroup)
                {
                    var i = Target.SpringBone.ColliderGroups.IndexOf(colliderGroup);
                    _selected = new SelectedColliderGroupGUI(_so, i);
                }
                else if (value is Vrm10InstanceSpringBone.Spring spring)
                {
                    var i = Target.SpringBone.Springs.IndexOf(spring);
                    _selected = new SelectedSpringGUI(Target, _so, i);
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

        Color s_selectedColor = new Color(0.5f, 0, 0, 0.5f);
        Color s_Color = new Color(0.6f, 0.6f, 0.6f, 0.5f);

        public void Draw3D(Vrm10InstanceSpringBone springBone)
        {
            if (_selected is SelectedColliderGroupGUI colliderGroup)
            {
                // colliderGroup.Draw3D();                
            }
            else if (_selected is SelectedSpringGUI spring)
            {
                // TODO
            }

            foreach (var group in springBone.ColliderGroups)
            {
                foreach (var collider in group.Colliders)
                {
                    var color = collider.gameObject == Selection.activeObject ? s_selectedColor : s_Color;
                    if (DrawCollider(collider, color))
                    {
                        Selection.activeObject = collider.gameObject;
                    }
                }
            }
            foreach (var spring in springBone.Springs)
            {
                for (int i = 1; i < spring.Joints.Count; ++i)
                {
                    var head = spring.Joints[i - 1];
                    var tail = spring.Joints[i];
                    var color = tail.gameObject == Selection.activeObject ? s_selectedColor : s_Color;
                    if (DrawJoint(head, tail, color))
                    {
                        Selection.activeObject = tail.gameObject;
                    }
                }
            }
        }

        static bool IsLeftDown()
        {
            return Event.current.type == EventType.MouseDown && Event.current.button == 0;
        }

        bool DrawCollider(VRM10SpringBoneCollider collider, Color color)
        {
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
            {
                Handles.matrix = collider.transform.localToWorldMatrix;
                Handles.color = color;
                switch (collider.ColliderType)
                {
                    case VRM10SpringBoneColliderTypes.Sphere:
                        // Handles.color = Color.magenta;
                        Handles.SphereHandleCap(collider.gameObject.GetInstanceID(), collider.Offset, Quaternion.identity, collider.Radius * 2, Event.current.type);
                        break;

                    case VRM10SpringBoneColliderTypes.Capsule:
                        // Handles.color = Color.cyan;
                        Handles.SphereHandleCap(collider.gameObject.GetInstanceID(), collider.Offset, Quaternion.identity, collider.Radius * 2, Event.current.type);
                        Handles.SphereHandleCap(collider.gameObject.GetInstanceID(), collider.Tail, Quaternion.identity, collider.Radius * 2, Event.current.type);
                        Handles.DrawLine(collider.Offset, collider.Tail);
                        break;
                }
                Handles.matrix = Matrix4x4.identity;
            }
            else if (IsLeftDown())
            {
                return HandleUtility.nearestControl == collider.gameObject.GetInstanceID();
            }
            return false;
        }

        bool DrawJoint(VRM10SpringBoneJoint head, VRM10SpringBoneJoint tail, Color color)
        {
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
            {
                Handles.color = color;
                Handles.SphereHandleCap(tail.gameObject.GetInstanceID(), tail.transform.position, Quaternion.identity, tail.m_jointRadius * 2, Event.current.type);
            }
            else if (IsLeftDown())
            {
                return HandleUtility.nearestControl == tail.gameObject.GetInstanceID();
            }
            return false;
        }
    }
}
