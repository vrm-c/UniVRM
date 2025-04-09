using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(ShadingToonyShowcase.ShadingToonyEntry))]
    public sealed class ShadingToonyEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(ShadingToonyShowcase.ShadingToonyEntry.shadingToonyFactor)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(ShadingToonyShowcase.ShadingToonyEntry.shadingShiftFactor)));
        }
    }
}