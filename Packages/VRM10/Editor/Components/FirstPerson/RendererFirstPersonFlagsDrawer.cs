using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace UniVRM10
{
    [CustomPropertyDrawer(typeof(RendererFirstPersonFlags))]
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

        const float WIDTH = 140.0f;

        public override void OnGUI(Rect position,
                          SerializedProperty property, GUIContent label)
        {
            var rendererProp = property.FindPropertyRelative("Renderer");
            var flagProp = property.FindPropertyRelative("FirstPersonFlag");

            var root = (property.serializedObject.targetObject as VRM10Object).Prefab;
            if (root != null)
            {
                var renderers = root.GetComponentsInChildren<Renderer>();
                var paths = renderers.Select(x => x.transform.RelativePathFrom(root.transform)).ToArray();
                var selected = Array.IndexOf(paths, rendererProp.stringValue);
                var newSelect = EditorGUI.Popup(LeftSide(position, WIDTH), selected, renderers.Select(x => x.name).ToArray());
                if (newSelect != selected && newSelect != -1)
                {
                    rendererProp.stringValue = paths[newSelect];
                }
                EditorGUI.PropertyField(RightSide(position, WIDTH), flagProp, new GUIContent(""), true);
            }
            else
            {
                EditorGUI.PropertyField(LeftSide(position, WIDTH), rendererProp, new GUIContent(""), true);
                EditorGUI.PropertyField(RightSide(position, WIDTH), flagProp, new GUIContent(""), true);
            }
        }

        /*
        public override float GetPropertyHeight(SerializedProperty property,
                                                                  GUIContent label)
        {
            return 60.0f;
        }
        */
    }
}
