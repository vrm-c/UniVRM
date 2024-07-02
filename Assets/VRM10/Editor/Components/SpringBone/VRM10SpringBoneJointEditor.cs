using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10SpringBoneJoint))]
    [CanEditMultipleObjects]
    class VRM10SpringBoneJointEditor : Editor
    {
        private VRM10SpringBoneJoint m_target;
        private SerializedProperty m_stiffnessForceProp;
        private SerializedProperty m_gravityPowerProp;
        private SerializedProperty m_gravityDirProp;
        private SerializedProperty m_dragForceProp;
        private SerializedProperty m_jointRadiusProp;

        private Vrm10Instance m_root;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_target = (VRM10SpringBoneJoint)target;

            m_stiffnessForceProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_stiffnessForce));
            m_gravityPowerProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_gravityPower));
            m_gravityDirProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_gravityDir));
            m_dragForceProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_dragForce));
            m_jointRadiusProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_jointRadius));

            if (m_target != null)
            {
                m_root = m_target.GetComponentInParent<Vrm10Instance>();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // spring status
            EditorGUILayout.LabelField("Spring", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Vrm-1.0", m_root, typeof(Vrm10Instance), true, null);
            if (m_root != null)
            {
                var found = m_root.SpringBone.FindJoint(m_target);
                if (found.HasValue)
                {
                    var (spring, i) = found.Value;
                    // var j = spring.Joints.IndexOf(m_target);
                    EditorGUILayout.LabelField($"Springs[{i}]({spring.Name})");
                    for (int j = 0; j < spring.Joints.Count; ++j)
                    {
                        var label = $"Joints[{j}]";
                        if
                         (spring.Joints[j] == m_target)
                        {
                            label += "★";
                        }
                        EditorGUILayout.ObjectField(label, spring.Joints[j], typeof(VRM10SpringBoneJoint), true, null);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("This joint not belongs any spring", MessageType.Warning);
                }
            }
            EditorGUI.EndDisabledGroup();

            // joint
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            LimitBreakSlider(m_stiffnessForceProp, 0.0f, 4.0f, 0.0f, Mathf.Infinity);
            LimitBreakSlider(m_gravityPowerProp, 0.0f, 2.0f, 0.0f, Mathf.Infinity);
            EditorGUILayout.PropertyField(m_gravityDirProp);
            EditorGUILayout.PropertyField(m_dragForceProp);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Collision", EditorStyles.boldLabel);

            LimitBreakSlider(m_jointRadiusProp, 0.0f, 0.5f, 0.0f, Mathf.Infinity);

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
