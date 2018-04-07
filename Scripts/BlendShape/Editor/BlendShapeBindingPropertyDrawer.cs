using System;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomPropertyDrawer(typeof(BlendShapeBinding))]
    public class BlendShapeBindingPropertyDrawer : PropertyDrawer
    {
        public static int GUIElementHeight = 60;

        public override void OnGUI(Rect position,
           SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var y = position.y;
                for (var depth = property.depth;
                    property.NextVisible(true) && property.depth > depth;
                    )
                {
                    {
                        var height = EditorGUI.GetPropertyHeight(property);
                        EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), property, false);
                        y += height;
                    }
                }
            }
        }

        public static void DrawElement(Rect position, SerializedProperty property,
            PreviewSceneManager scene)
        {
            if (scene == null) return;
            var height = 16;

            var y = position.y;
            var rect = new Rect(position.x, y, position.width, height);
            var index=StringPopup(rect, property.FindPropertyRelative("RelativePath"), scene.SkinnedMeshRendererPathList);

            y += height;
            rect = new Rect(position.x, y, position.width, height);
            IntPopup(rect, property.FindPropertyRelative("Index"), scene.GetBlendShapeNames(index));

            y += height;
            rect = new Rect(position.x, y, position.width, height);
            FloatSlider(rect, property.FindPropertyRelative("Weight"), 100);
        }

        static int StringPopup(Rect rect, SerializedProperty prop, string[] options)
        {
            if (options == null) { return -1; }

            var oldIndex = Array.IndexOf(options, prop.stringValue);
            var newIndex = EditorGUI.Popup(rect, oldIndex, options);
            if (newIndex != oldIndex && newIndex >= 0 && newIndex < options.Length)
            {
                prop.stringValue = options[newIndex];
            }
            return newIndex;
        }

        static int IntPopup(Rect rect, SerializedProperty prop, string[] options)
        {
            if (options == null){ return -1; }

            var oldIndex = prop.intValue;
            var newIndex = EditorGUI.Popup(rect, oldIndex, options);
            if (newIndex != oldIndex && newIndex >= 0 && newIndex < options.Length)
            {
                prop.intValue = newIndex;
            }
            return newIndex;
        }

        static void FloatSlider(Rect rect, SerializedProperty prop, float maxValue)
        {
            var oldValue = prop.floatValue;
            var newValue = EditorGUI.Slider(rect, prop.floatValue, 0, 100f);
            if (newValue != oldValue)
            {
                prop.floatValue = newValue;
            }
        }
    }
}
