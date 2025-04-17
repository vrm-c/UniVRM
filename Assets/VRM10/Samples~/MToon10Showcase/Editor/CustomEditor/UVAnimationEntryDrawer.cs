using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(UVAnimationShowcase.UVAnimationEntry))]
    public class UVAnimationEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(UVAnimationShowcase.UVAnimationEntry.uvAnimationMaskTexture)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(
                    nameof(UVAnimationShowcase.UVAnimationEntry.uvAnimationScrollXSpeedFactor)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(
                    nameof(UVAnimationShowcase.UVAnimationEntry.uvAnimationScrollYSpeedFactor)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(
                    nameof(UVAnimationShowcase.UVAnimationEntry.uvAnimationRotationSpeedFactor)));
        }
    }
}