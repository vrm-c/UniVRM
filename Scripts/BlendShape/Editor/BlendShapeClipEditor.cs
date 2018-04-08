using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeClip))]
    public class BlendShapeClipEditor : PreviewEditor
    {
        #region for Editor
        SerializedProperty m_BlendShapeNameProp;
        SerializedProperty m_PresetProp;
        SerializedProperty m_ValuesProp;
        ReorderableList m_ValuesList;
        SerializedProperty m_MaterialValuesProp;
        ReorderableList m_MaterialValuesList;
        #endregion

        BlendShapeClip m_target;
        bool m_changed;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_target = (BlendShapeClip)target;
            Bake(m_target.Values, m_target.MaterialValues);

            m_BlendShapeNameProp = serializedObject.FindProperty("BlendShapeName");
            m_PresetProp = serializedObject.FindProperty("Preset");
            m_ValuesProp = serializedObject.FindProperty("Values");

            m_ValuesList = new ReorderableList(serializedObject, m_ValuesProp);
            m_ValuesList.elementHeight = BlendShapeBindingHeight;
            m_ValuesList.drawElementCallback =
              (rect, index, isActive, isFocused) => {
                  var element = m_ValuesProp.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  if(DrawBlendShapeBinding(rect, element, PreviewSceneManager))
                  {
                      m_changed = true;
                  }
              };

            m_MaterialValuesProp = serializedObject.FindProperty("MaterialValues");
            m_MaterialValuesList = new ReorderableList(serializedObject, m_MaterialValuesProp);
            m_MaterialValuesList.elementHeight = MaterialValueBindingHeight;
            m_MaterialValuesList.drawElementCallback =
              (rect, index, isActive, isFocused) => {
                  var element = m_MaterialValuesProp.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  if(DrawMaterialValueBinding(rect, element, PreviewSceneManager))
                  {
                      m_changed = true;
                  }
              };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_changed = false;

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_BlendShapeNameProp, true);
            EditorGUILayout.PropertyField(m_PresetProp, true);

            EditorGUILayout.LabelField("BlendShapeBindings", EditorStyles.boldLabel);
            m_ValuesList.DoLayoutList();

            EditorGUILayout.LabelField("MaterialValueBindings", EditorStyles.boldLabel);
            m_MaterialValuesList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

            if (m_changed && PreviewSceneManager!=null)
            {
                PreviewSceneManager.Bake(m_target.Values, m_target.MaterialValues);
            }
        }

        public override string GetInfoString()
        {
            return BlendShapeKey.CreateFrom((BlendShapeClip)target).ToString();
        }

        public static int BlendShapeBindingHeight = 60;
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

        public static int MaterialValueBindingHeight = 90;
        public static bool DrawMaterialValueBinding(Rect position, SerializedProperty property,
            PreviewSceneManager scene)
        {
            bool changed = false;
            if (scene != null)
            {
                var height = 16;

                var y = position.y;
                var rect = new Rect(position.x, y, position.width, height);
                int pathIndex;
                if (StringPopup(rect, property.FindPropertyRelative("RelativePath"), scene.RendererPathList, out pathIndex))
                {
                    changed = true;
                }

                y += height;
                rect = new Rect(position.x, y, position.width, height);
                int materialIndex;
                if (IntPopup(rect, property.FindPropertyRelative("Index"), scene.GetMaterialNames(pathIndex), out materialIndex))
                {
                    changed = true;
                }

                y += height;
                rect = new Rect(position.x, y, position.width, height);
                int propIndex;
                if (StringPopup(rect, property.FindPropertyRelative("ValueName"), scene.GetMaterialPropNames(pathIndex, materialIndex), out propIndex))
                {
                    changed = true;
                }

                if (propIndex >= 0)
                {
                    var propTypes = scene.GetMaterialPropTypes(pathIndex, materialIndex);
                    if (propTypes != null && propIndex < propTypes.Length)
                    {
                        switch (propTypes[propIndex])
                        {
                            case ShaderUtil.ShaderPropertyType.Color:
                                {
                                    var baseValues= scene.GetMaterialPropBaseValues(pathIndex, materialIndex);
                                    property.FindPropertyRelative("BaseValue").vector4Value = baseValues[propIndex];

                                    // max
                                    y += height;
                                    rect = new Rect(position.x, y, position.width, height);
                                    if (ColorProp(rect, property.FindPropertyRelative("TargetValue")))
                                    {
                                        changed = true;
                                    }
                                }
                                break;
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
    }
}
