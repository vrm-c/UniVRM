
using System;
using UnityEditor;
using UnityEngine;
using UniGLTF.M17N;

namespace VRM
{
    [CustomEditor(typeof(VRMExportSettings))]
    public class VRMExportSettingsEditor : Editor
    {
        class CheckBoxProp
        {
            public SerializedProperty Property;
            public Func<string> Description;

            public CheckBoxProp(SerializedProperty property, Func<string> desc)
            {
                Property = property;
                Description = desc;
            }

            public CheckBoxProp(SerializedProperty property, VRMExportOptions desc) : this(property, () => Msg(desc))
            {
            }

            public CheckBoxProp(SerializedProperty property, string desc) : this(property, () => desc)
            {
            }

            public void Draw()
            {
                EditorGUILayout.PropertyField(Property);
                EditorGUILayout.HelpBox(Description(), MessageType.None);
                EditorGUILayout.Space();
            }
        }

        CheckBoxProp m_poseFreeze;
        CheckBoxProp m_useSparseAccessor;
        CheckBoxProp m_onlyBlendShapePosition;
        CheckBoxProp m_reduceBlendShape;
        CheckBoxProp m_reduceBlendShapeClip;
        CheckBoxProp m_divideVertexBuffer;

        static string Msg(VRMExportOptions key)
        {
            return LanguageGetter.Msg(key);
        }


        private void OnEnable()
        {
            m_poseFreeze = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.PoseFreeze)), VRMExportOptions.NORMALIZE);
            m_useSparseAccessor = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.UseSparseAccessor)), VRMExportOptions.BLENDSHAPE_USE_SPARSE);
            m_onlyBlendShapePosition = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.OnlyBlendshapePosition)), VRMExportOptions.BLENDSHAPE_EXCLUDE_NORMAL_AND_TANGENT);
            m_reduceBlendShape = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.ReduceBlendshape)), VRMExportOptions.BLENDSHAPE_ONLY_CLIP_USE);
            m_reduceBlendShapeClip = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.ReduceBlendshapeClip)), VRMExportOptions.BLENDSHAPE_EXCLUDE_UNKNOWN);
            m_divideVertexBuffer = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.DivideVertexBuffer)), VRMExportOptions.DIVIDE_VERTEX_BUFFER);
        }


        public override void OnInspectorGUI()
        {
            GUILayout.Space(20);
            var settings = (VRMExportSettings)target;
            var root = settings.Root;

            // ToDo: 任意の BlendShapeClip を適用する

            EditorGUIUtility.labelWidth = 160;
            serializedObject.Update();
            m_poseFreeze.Draw();
            m_useSparseAccessor.Draw();
            m_onlyBlendShapePosition.Draw();
            m_reduceBlendShape.Draw();
            m_reduceBlendShapeClip.Draw();
            m_divideVertexBuffer.Draw();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
