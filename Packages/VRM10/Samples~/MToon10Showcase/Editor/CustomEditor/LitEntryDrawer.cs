using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(LitShowcase.LitEntry))]
    public sealed class LitEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(LitShowcase.LitEntry.litColor)));
            EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(LitShowcase.LitEntry.litTexture)));
        }
    }
}