using UnityEditor;
using UnityEngine;


namespace UniVRM10
{
    [CustomEditor(typeof(VRM10SpringBoneCollider))]
    class VRM10SpringBoneColliderEditor : Editor
    {
        VRM10SpringBoneCollider _target;
        Vrm10Instance _vrm;

        SerializedProperty _script;
        SerializedProperty _colliderType;
        SerializedProperty _offset;
        SerializedProperty _tail;
        Tool _last = Tool.None;
        bool _openUtility = false;

        enum HandleType
        {
            Offset,
            Tail,
            Normal,
        }
        static (VRM10SpringBoneCollider Collider, HandleType HandleType)? s_selected;

        private void OnEnable()
        {
            _target = (VRM10SpringBoneCollider)target;
            if (_target != null)
            {
                _vrm = _target.GetComponentInParent<Vrm10Instance>();
            }

            _script = serializedObject.FindProperty("m_Script");
            _colliderType = serializedObject.FindProperty(nameof(_target.ColliderType));
            _offset = serializedObject.FindProperty(nameof(_target.Offset));
            _tail = serializedObject.FindProperty(nameof(_target.Tail));
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_script);
            }

            switch (_target.ColliderType)
            {
                case VRM10SpringBoneColliderTypes.Plane:
                    // radius: x
                    // tail: x
                    // normal: o
                    DrawPropertiesExcluding(serializedObject, "m_Script", nameof(_target.Tail), nameof(_target.Radius));
                    break;

                case VRM10SpringBoneColliderTypes.Sphere:
                case VRM10SpringBoneColliderTypes.SphereInside:
                    // radius: o
                    // tail: x
                    // normal: x
                    DrawPropertiesExcluding(serializedObject, nameof(_target.Tail), nameof(_target.Normal), "m_Script");
                    break;

                case VRM10SpringBoneColliderTypes.Capsule:
                case VRM10SpringBoneColliderTypes.CapsuleInside:
                    // radius: o
                    // tail: o
                    // normal: x
                    DrawPropertiesExcluding(serializedObject, nameof(_target.Normal), "m_Script");
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Drag offset"))
                {
                    s_selected = (_target, HandleType.Offset);
                }
                if (
                    _target.ColliderType == VRM10SpringBoneColliderTypes.Capsule
                    || _target.ColliderType == VRM10SpringBoneColliderTypes.CapsuleInside)
                {
                    if (GUILayout.Button("Drag tail"))
                    {
                        s_selected = (_target, HandleType.Tail);
                    }
                }
                if (_target.ColliderType == VRM10SpringBoneColliderTypes.Plane)
                {
                    if (GUILayout.Button("Drag normal"))
                    {
                        s_selected = (_target, HandleType.Normal);
                    }
                }
                using (new EditorGUI.DisabledScope(s_selected == default))
                {
                    if (GUILayout.Button("Drag end"))
                    {
                        s_selected = default;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            _openUtility = EditorGUILayout.Foldout(_openUtility, "utility");
            if (_openUtility)
            {
                if (_target.transform.childCount > 0)
                {
                    if (GUILayout.Button("Fit head-tail capsule"))
                    {
                        _colliderType.enumValueFlag = (int)VRM10SpringBoneColliderTypes.Capsule;
                        _offset.vector3Value = Vector3.zero;
                        var tail = _target.transform.GetChild(0);
                        _tail.vector3Value = _target.transform.worldToLocalMatrix.MultiplyPoint(
                            tail.position);
                    }
                }
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                if (Application.isPlaying)
                {
                    // UniGLTF.UniGLTFLogger.Log("invaliate");
                    if (_vrm != null)
                    {
                        _vrm.Runtime.SpringBone.ReconstructSpringBone();
                    }
                }
            }
        }

        public void OnSceneGUI()
        {
            HandleUtility.Repaint();

            bool updated = false;

            if (s_selected.HasValue && s_selected.Value.Collider == _target)
            {
                if (Tools.current != Tool.None)
                {
                    _last = Tools.current;
                }
                Tools.current = Tool.None;

                Handles.matrix = _target.transform.localToWorldMatrix;
                switch (s_selected.Value.HandleType)
                {
                    case HandleType.Offset:
                        {
                            Handles.Label(_target.Offset, "Collider offset");
                            EditorGUI.BeginChangeCheck();
                            var newTargetPosition = Handles.PositionHandle(_target.Offset, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_target, "collider");
                                _target.Offset = newTargetPosition;
                                updated = true;
                            }
                        }
                        break;

                    case HandleType.Tail:
                        {
                            Handles.Label(_target.Tail, "Capsule tail");
                            EditorGUI.BeginChangeCheck();
                            var newTargetPosition = Handles.PositionHandle(_target.Tail, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_target, "collider");
                                _target.Tail = newTargetPosition;
                                updated = true;
                            }
                        }
                        break;

                    case HandleType.Normal:
                        {
                            Handles.Label(_target.Offset, "Plane normal");
                            EditorGUI.BeginChangeCheck();
                            var r = Quaternion.FromToRotation(Vector3.up, _target.Normal);
                            var newRotation = Handles.RotationHandle(r, _target.Offset);
                            Handles.DrawLine(_target.Offset, _target.Offset + _target.Normal);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_target, "collider");
                                _target.Normal = newRotation * Vector3.up;
                                updated = true;
                            }
                        }
                        break;
                }
            }
            else
            {
                if (_last != Tool.None)
                {
                    Tools.current = _last;
                    _last = Tool.None;
                }
            }

            if (updated)
            {
                _target.OnValidate();
                if (Application.isPlaying && updated)
                {
                    // 反映!
                    _vrm.Runtime.SpringBone.ReconstructSpringBone();
                }
            }
        }
    }
}