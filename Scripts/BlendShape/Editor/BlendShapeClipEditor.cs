using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeClip))]
    public class BlendShapeClipEditor : Editor
    {
        //const string PREVIEW_INSTANCE_NAME = "__FACE_PREVIEW_SCENE__";
        PreviewSceneManager m_scene;
        PreviewFaceRenderer m_renderer;

        GameObject m_prefab;
        GameObject Prefab
        {
            get { return m_prefab; }
            set
            {
                if (m_prefab == value) return;
                m_prefab = value;

                if (m_scene != null)
                {
                    //Debug.LogFormat("OnDestroy");
                    GameObject.DestroyImmediate(m_scene.gameObject);
                    m_scene = null;
                }

                if (m_prefab != null)
                {
                    m_scene = PreviewSceneManager.GetOrCreate(m_prefab, m_target.Values);
                    if (m_scene != null)
                    {
                        m_scene.gameObject.SetActive(false);
                    }
                }
            }
        }

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

        private void OnEnable()
        {
            m_target = (BlendShapeClip)target;

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
                  if(DrawBlendShapeBinding(rect, element, m_scene))
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
                  if(DrawMaterialValueBinding(rect, element, m_scene))
                  {
                      m_changed = true;
                  }
              };

            m_renderer = new PreviewFaceRenderer();

            var assetPath = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(assetPath))
            {
                Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            }
        }

        private void OnDisable()
        {
            if (m_renderer != null)
            {
                m_renderer.Dispose();
                m_renderer = null;
            }
        }

        private void OnDestroy()
        {
            if (m_scene != null)
            {
                //Debug.LogFormat("OnDestroy");
                GameObject.DestroyImmediate(m_scene.gameObject);
                m_scene = null;
            }
        }

        public override void OnInspectorGUI()
        {
            m_changed = false;

            Prefab = (GameObject)EditorGUILayout.ObjectField("prefab", Prefab, typeof(GameObject), false);

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_BlendShapeNameProp, true);
            EditorGUILayout.PropertyField(m_PresetProp, true);

            EditorGUILayout.LabelField("BlendShapeBindings", EditorStyles.boldLabel);
            //EditorGUILayout.PropertyField(m_ValuesProp, true);
            m_ValuesList.DoLayoutList();

            EditorGUILayout.LabelField("MaterialValueBindings", EditorStyles.boldLabel);
            //EditorGUILayout.PropertyField(m_BlendShapeNameProp);
            m_MaterialValuesList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

            if (m_changed && m_scene!=null)
            {
                m_scene.Bake(m_target.Values);
            }
        }

        // very important to override this, it tells Unity to render an ObjectPreview at the bottom of the inspector
        public override bool HasPreviewGUI() { return true; }

        // the main ObjectPreview function... it's called constantly, like other IMGUI On*GUI() functions
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            // if this is happening, you have bigger problems
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), "Mesh preview requires\nrender texture support");
                }
                return;
            }
            if (Event.current.type != EventType.Repaint)
            {
                // if we don't need to update yet, then don't
                return;
            }

            if (m_renderer != null && m_scene != null)
            {
                var texture = m_renderer.Render(r, background, m_scene);
                if (texture != null)
                {
                    // draw the RenderTexture in the ObjectPreview pane
                    GUI.DrawTexture(r, texture, ScaleMode.StretchToFill, false); 
                }
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

        public static int MaterialValueBindingHeight = 60;
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
                int blendShapeIndex;
                if (IntPopup(rect, property.FindPropertyRelative("Index"), scene.GetMaterialNames(pathIndex), out blendShapeIndex))
                {
                    changed = true;
                }

                /*
                y += height;
                rect = new Rect(position.x, y, position.width, height);
                if (FloatSlider(rect, property.FindPropertyRelative("Weight"), 100))
                {
                    changed = true;
                }
                */
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

    }
}
