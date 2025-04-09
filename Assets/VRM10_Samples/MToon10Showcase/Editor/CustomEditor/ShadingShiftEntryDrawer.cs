using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(ShadingShiftShowcase.ShadingShiftEntry))]
    public sealed class ShadingShiftEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(ShadingShiftShowcase.ShadingShiftEntry.shadingShiftTexture)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(ShadingShiftShowcase.ShadingShiftEntry.shadingShiftTextureScale)));
        }
    }
}