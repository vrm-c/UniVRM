using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(EmissionShowcase.EmissionEntry))]
    public sealed class EmissionEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(EmissionShowcase.EmissionEntry.emissiveTexture)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(EmissionShowcase.EmissionEntry.emissiveFactorLinear)));
        }
    }
}