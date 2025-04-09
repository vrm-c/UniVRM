using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(GIEqualizationShowcase.GIEqualizationEntry))]
    public sealed class GIEqualizationEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(GIEqualizationShowcase.GIEqualizationEntry.giEqualizationFactor)));
        }
    }
}