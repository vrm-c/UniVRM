using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomPropertyDrawer(typeof(VRMFirstPerson.RendererFirstPersonFlags))]
    public class RendererFirstPersonFlagsDrawer : PropertyDrawer
    {
        static Rect LeftSide(Rect position, float width)
        {
            return new Rect(position.x, position.y, position.width - width, position.height);
        }
        static Rect RightSide(Rect position, float width)
        {
            return new Rect(position.x + (position.width - width), position.y, width, position.height);
        }

        public override void OnGUI(Rect position,
                          SerializedProperty property, GUIContent label)
        {
            var rendererProp = property.FindPropertyRelative("Renderer");
            var flagProp = property.FindPropertyRelative("FirstPersonFlag");

            const float WIDTH = 140.0f;
            EditorGUI.PropertyField(LeftSide(position, WIDTH), rendererProp, new GUIContent(""), true);
            EditorGUI.PropertyField(RightSide(position, WIDTH), flagProp, new GUIContent(""), true);
        }
    }
}
