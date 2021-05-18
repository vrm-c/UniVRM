
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomPropertyDrawer(typeof(VRM10RotationOffset))]
    public class IngredientDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16f + 18f;
        }

        // Draw the property inside the given rect
        void DrawEuler(Rect position, SerializedProperty property)
        {
            Vector3 euler = property.quaternionValue.eulerAngles;
            euler = EditorGUI.Vector3Field(position, "Offset", euler);
            property.quaternionValue = Quaternion.Euler(euler);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawEuler(position, property.FindPropertyRelative(nameof(VRM10RotationOffset.Rotation)));
        }
    }
}
