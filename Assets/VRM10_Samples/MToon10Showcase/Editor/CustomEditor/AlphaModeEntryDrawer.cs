using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(AlphaModeShowcase.AlphaModeEntry))]
    public sealed class AlphaModeEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(AlphaModeShowcase.AlphaModeEntry.alphaMode)));
            EditorGUILayout.Slider(
                property.FindPropertyRelative(nameof(AlphaModeShowcase.AlphaModeEntry.alphaCutoff)), 0, 1);
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(AlphaModeShowcase.AlphaModeEntry.doubleSidedMode)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(AlphaModeShowcase.AlphaModeEntry.transparentWithZWriteMode)));
        }
    }
}