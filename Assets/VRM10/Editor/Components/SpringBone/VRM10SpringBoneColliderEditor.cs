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

        static VRM10SpringBoneCollider s_selected;

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
                if (GUILayout.Button("Drag handle"))
                {
                    s_selected = _target;
                }

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
            EditorGUILayout.EndHorizontal();

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

            if (s_selected == _target)
            {
                var c = _target;
                Handles.matrix = c.transform.localToWorldMatrix;
                if (_target.ColliderType == VRM10SpringBoneColliderTypes.Capsule
                || _target.ColliderType == VRM10SpringBoneColliderTypes.CapsuleInside)
                {
                    EditorGUI.BeginChangeCheck();
                    var newTargetPosition = Handles.PositionHandle(c.Tail, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(c, "collider");
                        c.Tail = newTargetPosition;
                        updated = true;
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    var newTargetPosition = Handles.PositionHandle(c.Offset, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(c, "collider");
                        c.Offset = newTargetPosition;
                        updated = true;
                    }
                }
            }

            if (Application.isPlaying && updated)
            {
                // 反映!
                _vrm.Runtime.SpringBone.ReconstructSpringBone();
            }
        }
    }
}