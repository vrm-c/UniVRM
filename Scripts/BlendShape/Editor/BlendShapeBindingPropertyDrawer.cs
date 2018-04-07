using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void DrawElement(Rect position, SerializedProperty property, string[] pathList)
        {
            var y = position.y;
            /*
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
            */
            var pathProp = property.FindPropertyRelative("RelativePath");

            var height = EditorGUI.GetPropertyHeight(property);
            var rect = new Rect(position.x, y, position.width, height);
            //EditorGUI.PropertyField(, pathProp, false);

            StringPopup(rect, pathProp, pathList);
        }

        static void StringPopup(Rect rect, SerializedProperty prop, string[] options)
        {
            var oldIndex = Array.IndexOf(options, prop.stringValue);
            var newIndex = EditorGUI.Popup(rect, oldIndex, options);
            if (newIndex != oldIndex && newIndex >= 0 && newIndex < options.Length)
            {
                prop.stringValue = options[newIndex];
            }
        }
    }
}
