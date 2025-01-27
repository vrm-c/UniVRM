using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace UniVRM10
{
    [CustomEditor(typeof(VRM10SpringBoneColliderGroup))]
    class VRM10SpringBoneColliderGroupEditor : Editor
    {
        VRM10SpringBoneColliderGroup m_target;
        ListView m_colliders;

        static VRM10SpringBoneCollider s_collider;

        void OnEnable()
        {
            m_target = (VRM10SpringBoneColliderGroup)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // root.TrackSerializedObjectValue(serializedObject, OnValueChanged);

            root.Bind(serializedObject);

            {
                var s = new PropertyField { bindingPath = "m_Script" };
                s.SetEnabled(false);
                root.Add(s);
            }


            root.Add(new PropertyField { bindingPath = nameof(VRM10SpringBoneColliderGroup.Name) });

            {
                m_colliders = new ListView
                {
                    bindingPath = nameof(VRM10SpringBoneColliderGroup.Colliders)
                };
#if UNITY_2022_3_OR_NEWER
                m_colliders.selectionChanged += (e) =>
#else
                m_colliders.onSelectionChange += (e) =>
#endif
                {
                    var item = (SerializedProperty)e.FirstOrDefault();
                    if (item == null)
                    {
                        s_collider = null;
                    }
                    else
                    {
                        s_collider = (VRM10SpringBoneCollider)item.objectReferenceValue;
                    }
                };
                m_colliders.showAddRemoveFooter = true;
                m_colliders.showFoldoutHeader = true;
                m_colliders.headerTitle = "Colliders";
                root.Add(m_colliders);
            }

            return root;
        }

        public void OnSceneGUI()
        {
            HandleUtility.Repaint();
            if (m_colliders == null)
            {
                return;
            }

            if (s_collider != null && m_target.Colliders.Contains(s_collider))
            {
                var c = s_collider;
                EditorGUI.BeginChangeCheck();
                Handles.matrix = c.transform.localToWorldMatrix;
                var newTargetPosition = Handles.PositionHandle(c.Offset, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(c, "collider");
                    c.Offset = newTargetPosition;
                }
            }
        }
    }
}