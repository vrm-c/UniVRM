using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
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
            if (target != null)
            {
                if (target.SpringBone?.ColliderGroups != null)
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

                if (target.SpringBone?.Springs != null)
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
        Color s_hoverColor = new Color(0.5f, 0.5f, 0, 0.5f);
        Color s_Color = new Color(0.6f, 0.6f, 0.6f, 0.5f);

        Color GetColor(MonoBehaviour target)
        {
            if (target == Active)
            {
                return s_selectedColor;
            }
            if (target == Hover)
            {
                return s_hoverColor;
            }
            return s_Color;
        }

        Vrm10InstanceSpringBone last = null;
        MonoBehaviour Hover = null;
        public MonoBehaviour Active = null;

        public bool Draw3D(Vrm10InstanceSpringBone springBone)
        {
            var lastActive = Active;
            var lastHover = Hover;

            if (springBone != last)
            {
                Active = null;
                Hover = null;
                last = springBone;
            }

            if (_selected is SelectedColliderGroupGUI colliderGroup)
            {
                // colliderGroup.Draw3D();                
            }
            else if (_selected is SelectedSpringGUI spring)
            {
                // TODO
            }

            MonoBehaviour newActive = null;

            //
            // collider
            //
            foreach (var group in springBone.ColliderGroups)
            {
                foreach (var collider in group.Colliders)
                {
                    var color = GetColor(collider);
                    var (isActive, isHover) = DrawCollider(collider, color);
                    if (isActive)
                    {
                        newActive = collider;
                    }
                    if (isHover)
                    {
                        Hover = collider;
                    }
                }
            }

            //
            // joint
            //
            foreach (var spring in springBone.Springs)
            {
                VRM10SpringBoneJoint head = null;
                for (int i = 0; i < spring.Joints.Count; ++i)
                {
                    var joint = spring.Joints[i];
                    var color = GetColor(joint);
                    var (isActive, isHover) = DrawJoint(joint, color, head);
                    if (isActive)
                    {
                        newActive = joint;
                    }
                    if (isHover)
                    {
                        Hover = joint;
                    }
                    head = joint;
                }
            }

            if (Active is VRM10SpringBoneCollider activeCollider)
            {
                // Active な collider の移動ハンドル(Offset, Tail)
                EditorGUI.BeginChangeCheck();
                Handles.matrix = activeCollider.transform.localToWorldMatrix;
                var offset = Handles.PositionHandle(activeCollider.Offset, Quaternion.identity);
                var tail = Vector3.zero;
                if (activeCollider.ColliderType == VRM10SpringBoneColliderTypes.Capsule)
                {
                    tail = Handles.PositionHandle(activeCollider.Tail, Quaternion.identity);
                }
                var offsetChanged = EditorGUI.EndChangeCheck();
                if (offsetChanged)
                {
                    // apply
                    Undo.RecordObject(activeCollider, "activeCollider");
                    activeCollider.Offset = offset;
                    if (activeCollider.ColliderType == VRM10SpringBoneColliderTypes.Capsule)
                    {
                        activeCollider.Tail = tail;
                    }
                }
                Handles.matrix = Matrix4x4.identity;
            }

            if (newActive != null)
            {
                Selection.activeObject = newActive.gameObject;
                Active = newActive;
            }
            if (Hover != null)
            {
                Handles.Label(Hover.transform.position, Hover.name);
            }

            return lastActive != Active || lastHover != Hover;
        }

        static bool IsLeftDown()
        {
            return Event.current.type == EventType.MouseDown && Event.current.button == 0;
        }

        (bool IsActive, bool IsHover) DrawCollider(VRM10SpringBoneCollider collider, Color color)
        {
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
            {
                Handles.matrix = collider.transform.localToWorldMatrix;
                Handles.color = color;
                switch (collider.ColliderType)
                {
                    case VRM10SpringBoneColliderTypes.Sphere:
                        // Handles.color = Color.magenta;
                        Handles.SphereHandleCap(collider.GetInstanceID(), collider.Offset, Quaternion.identity, collider.Radius * 2, Event.current.type);
                        break;

                    case VRM10SpringBoneColliderTypes.Capsule:
                        // Handles.color = Color.cyan;
                        Handles.SphereHandleCap(collider.GetInstanceID(), collider.Offset, Quaternion.identity, collider.Radius * 2, Event.current.type);
                        Handles.SphereHandleCap(collider.GetInstanceID(), collider.Tail, Quaternion.identity, collider.Radius * 2, Event.current.type);
                        Handles.DrawLine(collider.Offset, collider.Tail);
                        break;
                }
                Handles.matrix = Matrix4x4.identity;
            }

            var isHover = HandleUtility.nearestControl == collider.GetInstanceID();
            return (IsLeftDown() && isHover, isHover);
        }

        (bool IsActive, bool IsHover) DrawJoint(VRM10SpringBoneJoint joint, Color color, VRM10SpringBoneJoint head)
        {
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
            {

                if (head == null)
                {
                    // 先頭
                    Handles.color = color;
                    Handles.CubeHandleCap(joint.GetInstanceID(), joint.transform.position, joint.transform.rotation,
                        // head の radius
                        joint.m_jointRadius, Event.current.type);
                }
                else
                {
                    // line
                    Handles.color = Color.green;
                    Handles.DrawLine(joint.transform.position, head.transform.position);
                    // joint
                    Handles.color = color;
                    Handles.SphereHandleCap(joint.GetInstanceID(), joint.transform.position, joint.transform.rotation,
                        // head の radius
                        head.m_jointRadius * 2, Event.current.type);
                }
            }

            var isHover = HandleUtility.nearestControl == joint.GetInstanceID();
            return (IsLeftDown() && isHover, isHover);
        }
    }
}
