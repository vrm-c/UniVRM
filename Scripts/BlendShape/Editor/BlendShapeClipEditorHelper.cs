using System;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    public static class BlendShapeClipEditorHelper
    {
        public static bool DrawBlendShapeBinding(Rect position, SerializedProperty property,
            PreviewSceneManager scene)
        {
            bool changed = false;
            if (scene != null)
            {
                var height = 16;

                var y = position.y;
                var rect = new Rect(position.x, y, position.width, height);
                int pathIndex;
                if (StringPopup(rect, property.FindPropertyRelative("RelativePath"), scene.SkinnedMeshRendererPathList, out pathIndex))
                {
                    changed = true;
                }

                y += height;
                rect = new Rect(position.x, y, position.width, height);
                int blendShapeIndex;
                if (IntPopup(rect, property.FindPropertyRelative("Index"), scene.GetBlendShapeNames(pathIndex), out blendShapeIndex))
                {
                    changed = true;
                }

                y += height;
                rect = new Rect(position.x, y, position.width, height);
                if (FloatSlider(rect, property.FindPropertyRelative("Weight"), 100))
                {
                    changed = true;
                }
            }
            return changed;
        }

        public static bool DrawMaterialValueBinding(Rect position, SerializedProperty property,
            PreviewSceneManager scene)
        {
            bool changed = false;
            if (scene != null)
            {
                var height = 16;

                var y = position.y;
                var rect = new Rect(position.x, y, position.width, height);
                int materialIndex;
                if (StringPopup(rect, property.FindPropertyRelative("MaterialName"), scene.MaterialNames, out materialIndex))
                {
                    changed = true;
                }

                if (materialIndex >= 0)
                {
                    var materialItem = scene.GetMaterialItem(scene.MaterialNames[materialIndex]);
                    if (materialItem != null)
                    {
                        y += height;
                        rect = new Rect(position.x, y, position.width, height);

                        // プロパティ名のポップアップ
                        int propIndex;
                        if (StringPopup(rect, property.FindPropertyRelative("ValueName"), materialItem.PropNames, out propIndex))
                        {
                            changed = true;
                        }

                        if (propIndex >= 0)
                        {
                            // 有効なプロパティ名が選択された
                            var propItem = materialItem.PropMap[materialItem.PropNames[propIndex]];
                            {
                                switch (propItem.PropertyType)
                                {
                                    case ShaderUtil.ShaderPropertyType.Color:
                                        {
                                            property.FindPropertyRelative("BaseValue").vector4Value = propItem.DefaultValues;

                                            // max
                                            y += height;
                                            rect = new Rect(position.x, y, position.width, height);
                                            if (ColorProp(rect, property.FindPropertyRelative("TargetValue")))
                                            {
                                                changed = true;
                                            }
                                        }
                                        break;

                                    case ShaderUtil.ShaderPropertyType.TexEnv:
                                        {
                                            property.FindPropertyRelative("BaseValue").vector4Value = propItem.DefaultValues;

                                            // max
                                            y += height;
                                            rect = new Rect(position.x, y, position.width, height);
                                            if (OffsetProp(rect, property.FindPropertyRelative("TargetValue")))
                                            {
                                                changed = true;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return changed;
        }


        static bool StringPopup(Rect rect, SerializedProperty prop, string[] options, out int newIndex)
        {
            if (options == null)
            {
                newIndex = -1;
                return false;
            }

            var oldIndex = Array.IndexOf(options, prop.stringValue);
            newIndex = EditorGUI.Popup(rect, oldIndex, options);
            if (newIndex != oldIndex && newIndex >= 0 && newIndex < options.Length)
            {
                prop.stringValue = options[newIndex];
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool IntPopup(Rect rect, SerializedProperty prop, string[] options, out int newIndex)
        {
            if (options == null)
            {
                newIndex = -1;
                return false;
            }

            var oldIndex = prop.intValue;
            newIndex = EditorGUI.Popup(rect, oldIndex, options);
            if (newIndex != oldIndex && newIndex >= 0 && newIndex < options.Length)
            {
                prop.intValue = newIndex;
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool FloatSlider(Rect rect, SerializedProperty prop, float maxValue)
        {
            var oldValue = prop.floatValue;
            var newValue = EditorGUI.Slider(rect, prop.floatValue, 0, 100f);
            if (newValue != oldValue)
            {
                prop.floatValue = newValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool ColorProp(Rect rect, SerializedProperty prop)
        {
            var oldValue = (Color)prop.vector4Value;
            var newValue = EditorGUI.ColorField(rect, prop.displayName, oldValue);
            if (newValue != oldValue)
            {
                prop.vector4Value = newValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool OffsetProp(Rect rect, SerializedProperty prop)
        {
            var oldValue = prop.vector4Value;
            var newValue = EditorGUI.Vector4Field(rect, prop.displayName, oldValue);
            if (newValue != oldValue)
            {
                prop.vector4Value = newValue;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
