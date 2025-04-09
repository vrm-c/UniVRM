using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(RimLightingShowcase.RimLightingMixFactorEntry))]
    public sealed class RimLightingEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(
                    nameof(RimLightingShowcase.RimLightingMixFactorEntry.rimMultiplyTexture)));
            EditorGUILayout.Slider(
                property.FindPropertyRelative(
                    nameof(RimLightingShowcase.RimLightingMixFactorEntry.rimLightingMixFactor)),
                0, 1);
        }
    }
}