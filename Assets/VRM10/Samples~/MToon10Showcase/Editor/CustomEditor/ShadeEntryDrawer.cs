using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(ShadeShowcase.ShadeEntry))]
    public sealed class ShadeEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(ShadeShowcase.ShadeEntry.shadeColor)));
            EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(ShadeShowcase.ShadeEntry.shadeTexture)));
        }
    }
}