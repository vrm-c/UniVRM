
using System;
using UnityEditor;
using UnityEngine;
using UniGLTF.M17N;
using System.Collections.Generic;

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

        List<CheckBoxProp> m_checkbox_list = new List<CheckBoxProp>();

        static string Msg(VRMExportOptions key)
        {
            return LanguageGetter.Msg(key);
        }


        private void OnEnable()
        {
            m_checkbox_list.Add(new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.PoseFreeze)), VRMExportOptions.NORMALIZE));
            m_checkbox_list.Add(new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.UseSparseAccessor)), VRMExportOptions.BLENDSHAPE_USE_SPARSE));
            m_checkbox_list.Add(new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.OnlyBlendshapePosition)), VRMExportOptions.BLENDSHAPE_EXCLUDE_NORMAL_AND_TANGENT));
            m_checkbox_list.Add(new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.ReduceBlendshape)), VRMExportOptions.BLENDSHAPE_ONLY_CLIP_USE));
            m_checkbox_list.Add(new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.ReduceBlendshapeClip)), VRMExportOptions.BLENDSHAPE_EXCLUDE_UNKNOWN));
            m_checkbox_list.Add(new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.DivideVertexBuffer)), VRMExportOptions.DIVIDE_VERTEX_BUFFER));
            m_checkbox_list.Add(new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.KeepVertexColor)), VRMExportOptions.KEEP_VERTEX_COLOR));
            m_checkbox_list.Add(new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.KeepAnimation)), VRMExportOptions.EXPORT_GLTF_ANIMATION));
        }


        public override void OnInspectorGUI()
        {
            GUILayout.Space(20);
            var settings = (VRMExportSettings)target;
            var root = settings.Root;

            // ToDo: 任意の BlendShapeClip を適用する

            EditorGUIUtility.labelWidth = 160;
            serializedObject.Update();
            foreach (var checkbox in m_checkbox_list)
            {
                checkbox.Draw();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
