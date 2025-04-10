using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(ParametricRimShowcase.ParametricRimEntry))]
    public sealed class ParametricRimEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(ParametricRimShowcase.ParametricRimEntry.parametricRimColor)));
            EditorGUILayout.Slider(property.FindPropertyRelative(nameof(ParametricRimShowcase.ParametricRimEntry
                .parametricRimFresnelPowerFactor)), 0, 100);
            EditorGUILayout.Slider(
                property.FindPropertyRelative(nameof(ParametricRimShowcase.ParametricRimEntry
                    .parametricRimLiftFactor)), 0, 1);
        }
    }
}