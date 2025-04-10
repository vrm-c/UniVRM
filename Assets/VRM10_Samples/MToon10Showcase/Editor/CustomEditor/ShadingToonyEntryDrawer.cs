using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(ShadingToonyShowcase.ShadingToonyEntry))]
    public sealed class ShadingToonyEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.Slider(
                property.FindPropertyRelative(nameof(ShadingToonyShowcase.ShadingToonyEntry.shadingToonyFactor)),
                0f, 1f);
            EditorGUILayout.Slider(
                property.FindPropertyRelative(nameof(ShadingToonyShowcase.ShadingToonyEntry.shadingShiftFactor)),
                -1f, 1f);
        }
    }
}