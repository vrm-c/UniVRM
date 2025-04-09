using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(OutlineShowcase.OutlineEntry))]
    public sealed class OutlineEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(OutlineShowcase.OutlineEntry.outlineWidthMode)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(OutlineShowcase.OutlineEntry.outlineWidthFactor)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(OutlineShowcase.OutlineEntry.outlineColorFactor)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(OutlineShowcase.OutlineEntry.outlineWidthMultiplyTexture)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(OutlineShowcase.OutlineEntry.outlineLightingMixFactor)));
        }
    }
}