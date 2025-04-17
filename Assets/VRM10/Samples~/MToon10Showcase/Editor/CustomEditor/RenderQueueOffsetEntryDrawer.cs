using UnityEditor;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    [CustomPropertyDrawer(typeof(RenderQueueOffsetShowcase.RenderQueueOffsetEntry))]
    public sealed class RenderQueueOffsetEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative(
                    nameof(RenderQueueOffsetShowcase.RenderQueueOffsetEntry.renderQueueOffset)));
        }
    }
}