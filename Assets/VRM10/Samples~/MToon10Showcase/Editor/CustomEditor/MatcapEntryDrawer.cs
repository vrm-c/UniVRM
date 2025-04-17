using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(MatcapShowcase.MatcapEntry))]
    public sealed class MatcapEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(MatcapShowcase.MatcapEntry.matcapTexture)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(MatcapShowcase.MatcapEntry.matcapColorFactor)));
        }
    }
}