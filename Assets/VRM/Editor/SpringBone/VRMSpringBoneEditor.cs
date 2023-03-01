using UnityEditor;
using UnityEngine;

namespace VRM
{
    [CustomEditor(typeof(VRMSpringBone))]
    [CanEditMultipleObjects]
    class VRMSpringBoneEditor : Editor
    {
        private VRMSpringBone m_target;

        private SerializedProperty m_commentProp;
        private SerializedProperty m_gizmoColorProp;
        private SerializedProperty m_stiffnessForceProp;
        private SerializedProperty m_gravityPowerProp;
        private SerializedProperty m_gravityDirProp;
        private SerializedProperty m_dragForceProp;
        private SerializedProperty m_centerProp;
        private SerializedProperty m_rootBonesProp;
        private SerializedProperty m_hitRadiusProp;
        private SerializedProperty m_colliderGroupsProp;
        private SerializedProperty m_updateTypeProp;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_target = (VRMSpringBone)target;

            m_commentProp = serializedObject.FindProperty(nameof(VRMSpringBone.m_comment));
            m_gizmoColorProp = serializedObject.FindProperty("m_gizmoColor");
            m_stiffnessForceProp = serializedObject.FindProperty(nameof(VRMSpringBone.m_stiffnessForce));
            m_gravityPowerProp = serializedObject.FindProperty(nameof(VRMSpringBone.m_gravityPower));
            m_gravityDirProp = serializedObject.FindProperty(nameof(VRMSpringBone.m_gravityDir));
            m_dragForceProp = serializedObject.FindProperty(nameof(VRMSpringBone.m_dragForce));
            m_centerProp = serializedObject.FindProperty(nameof(VRMSpringBone.m_center));
            m_rootBonesProp = serializedObject.FindProperty(nameof(VRMSpringBone.RootBones));
            m_hitRadiusProp = serializedObject.FindProperty(nameof(VRMSpringBone.m_hitRadius));
            m_colliderGroupsProp = serializedObject.FindProperty(nameof(VRMSpringBone.ColliderGroups));
            m_updateTypeProp = serializedObject.FindProperty(nameof(VRMSpringBone.m_updateType));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_commentProp);
            EditorGUILayout.PropertyField(m_gizmoColorProp);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            LimitBreakSlider(m_stiffnessForceProp, 0.0f, 4.0f, 0.0f, Mathf.Infinity);
            LimitBreakSlider(m_gravityPowerProp, 0.0f, 2.0f, 0.0f, Mathf.Infinity);
            EditorGUILayout.PropertyField(m_gravityDirProp);
            EditorGUILayout.PropertyField(m_dragForceProp);
            EditorGUILayout.PropertyField(m_centerProp);
            EditorGUILayout.PropertyField(m_rootBonesProp);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Collision", EditorStyles.boldLabel);

            LimitBreakSlider(m_hitRadiusProp, 0.0f, 0.5f, 0.0f, Mathf.Infinity);
            EditorGUILayout.PropertyField(m_colliderGroupsProp);
            EditorGUILayout.PropertyField(m_updateTypeProp);


            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// スライダーと数値入力で限界値の違う、所謂「限界突破スライダー」を作成する
        /// `EditorGUILayout.PropertyField` の代替として利用する
        /// </summary>
        private static void LimitBreakSlider(SerializedProperty property, float sliderLeft, float sliderRight, float numberLeft, float numberRight)
        {
            var label = new GUIContent(property.displayName);
            var currentValue = property.floatValue;

            var rect = EditorGUILayout.GetControlRect();

            EditorGUI.BeginProperty(rect, label, property);

            rect = EditorGUI.PrefixLabel(rect, label);

            // slider
            {
                EditorGUI.BeginChangeCheck();

                var sliderRect = rect;
                sliderRect.width -= 55.0f;
                rect.xMin += rect.width - 50.0f;

                var clampedvalue = Mathf.Clamp(currentValue, sliderLeft, sliderRight);
                var sliderValue = GUI.HorizontalSlider(sliderRect, clampedvalue, sliderLeft, sliderRight);

                if (EditorGUI.EndChangeCheck())
                {
                    property.floatValue = sliderValue;
                }
            }

            // number
            {
                EditorGUI.BeginChangeCheck();

                var numberValue = Mathf.Clamp(EditorGUI.FloatField(rect, currentValue), numberLeft, numberRight);

                if (EditorGUI.EndChangeCheck())
                {
                    property.floatValue = numberValue;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
