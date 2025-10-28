using UniGLTF.Extensions.VRMC_vrm;
using UnityEditor;

namespace UniVRM10
{
    public class VRM10ObjectLookAtEditor
    {
        private readonly SerializedObject _serializedObject;
        private readonly SerializedProperty _OffsetFromHead;
        private readonly SerializedProperty _LookAtType;


        class CurveMapEditor
        {
            private readonly SerializedProperty _CurveXRangeDegree;
            private readonly SerializedProperty _CurveYRangeDegree;
            private readonly string _name;

            public CurveMapEditor(SerializedObject serializedObject, string name)
            {
                _CurveXRangeDegree = serializedObject.FindProperty($"LookAt.{name}.CurveXRangeDegree");
                _CurveYRangeDegree = serializedObject.FindProperty($"LookAt.{name}.CurveYRangeDegree");
                _name = name;
            }

            public void OnInspectorGUI(float yMax)
            {
                EditorGUILayout.LabelField(_name);
                EditorGUI.indentLevel++;
                Vrm10EditorUtility.LimitBreakSlider(_CurveXRangeDegree, 0, 90.0f, 0, 90.0f);
                Vrm10EditorUtility.LimitBreakSlider(_CurveYRangeDegree, 0, yMax, 0, 90.0f);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }
        private readonly CurveMapEditor _HorizontalOuter;
        private readonly CurveMapEditor _HorizontalInner;
        private readonly CurveMapEditor _VerticalDown;
        private readonly CurveMapEditor _VerticalUp;

        public VRM10ObjectLookAtEditor(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            _OffsetFromHead = serializedObject.FindProperty("LookAt.OffsetFromHead");
            _LookAtType = serializedObject.FindProperty("LookAt.LookAtType");
            _HorizontalOuter = new(serializedObject, "HorizontalOuter");
            _HorizontalInner = new(serializedObject, "HorizontalInner");
            _VerticalDown = new(serializedObject, "VerticalDown");
            _VerticalUp = new(serializedObject, "VerticalUp");
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_OffsetFromHead);
            EditorGUILayout.PropertyField(_LookAtType);
            EditorGUILayout.Space();

            switch ((LookAtType)_LookAtType.enumValueIndex)
            {
                case LookAtType.bone:
                    {
                        EditorGUILayout.HelpBox("Degree Input (0-90) => EyeBone Degree(0-90)", MessageType.Info);
                        _HorizontalOuter.OnInspectorGUI(90);
                        _HorizontalInner.OnInspectorGUI(90);
                        _VerticalDown.OnInspectorGUI(90);
                        _VerticalUp.OnInspectorGUI(90);
                    }
                    break;

                case LookAtType.expression:
                    {
                        EditorGUILayout.HelpBox("Degree Input (0-90) => Expression Weight(0-1.0)", MessageType.Info);
                        _HorizontalOuter.OnInspectorGUI(1);
                        _HorizontalInner.OnInspectorGUI(1);
                        _VerticalDown.OnInspectorGUI(1);
                        _VerticalUp.OnInspectorGUI(1);
                    }
                    break;

                default:
                    break;
            }

            _serializedObject.ApplyModifiedProperties();
        }
    }
}