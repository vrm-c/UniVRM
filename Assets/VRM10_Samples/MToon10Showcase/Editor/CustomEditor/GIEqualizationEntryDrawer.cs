using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(GIEqualizationShowcase.GIEqualizationEntry))]
    public sealed class GIEqualizationEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.Slider(
                property.FindPropertyRelative(nameof(GIEqualizationShowcase.GIEqualizationEntry.giEqualizationFactor)),
                0, 1);
        }
    }
}