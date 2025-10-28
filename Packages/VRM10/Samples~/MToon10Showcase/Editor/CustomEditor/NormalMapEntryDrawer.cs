using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(NormalMapShowcase.NormalMapEntry))]
    public class NormalMapEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(NormalMapShowcase.NormalMapEntry.normalTexture)));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(nameof(NormalMapShowcase.NormalMapEntry.normalTextureScale)));
        }
    }
}