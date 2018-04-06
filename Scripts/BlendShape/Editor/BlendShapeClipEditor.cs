using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeClip))]
    public class BlendShapeClipEditor : Editor
    {
        #region for Editor
        SerializedProperty m_BlendShapeNameProp;
        SerializedProperty m_PresetProp;
        SerializedProperty m_ValuesProp;
        ReorderableList m_ValuesList;
        SerializedProperty m_MaterialValuesProp;
        ReorderableList m_MaterialValuesList;
        #endregion

        #region for Preview
        GameObject m_prefab;
        PreviewFaceRenderer m_renderer;
        #endregion

        private void OnEnable()
        {
            m_BlendShapeNameProp = serializedObject.FindProperty("BlendShapeName");
            m_PresetProp = serializedObject.FindProperty("Preset");
            m_ValuesProp = serializedObject.FindProperty("Values");

            m_ValuesList = new ReorderableList(serializedObject, m_ValuesProp);
            m_ValuesList.elementHeight = BlendShapeBindingPropertyDrawer.GUIElementHeight;
            m_ValuesList.drawElementCallback =
              (rect, index, isActive, isFocused) => {
                  var element = m_ValuesProp.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  EditorGUI.PropertyField(rect, element);
              };

            m_MaterialValuesProp = serializedObject.FindProperty("MaterialValues");
            m_MaterialValuesList = new ReorderableList(serializedObject, m_MaterialValuesProp);
            m_MaterialValuesList.elementHeight = MaterialValueBindingPropertyDrawer.GUIElementHeight;
            m_MaterialValuesList.drawElementCallback =
              (rect, index, isActive, isFocused) => {
                  var element = m_MaterialValuesProp.GetArrayElementAtIndex(index);
                  rect.height -= 4;
                  rect.y += 2;
                  EditorGUI.PropertyField(rect, element);
              };

            var assetPath = AssetDatabase.GetAssetPath(target);

            //Debug.LogFormat("BlendShapeClipEditor: {0}", assetPath);
            if (!string.IsNullOrEmpty(assetPath))
            {
                m_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (m_prefab != null)
                {
                    if (m_renderer != null)
                    {
                        m_renderer.Dispose();
                        m_renderer = null;
                    }
                    m_renderer = new PreviewFaceRenderer(m_prefab);
                }
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
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            //攻撃力の数値をラベルとして表示する
            //EditorGUILayout.LabelField("攻撃力", character.攻撃力.ToString());

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
            { // if we don't need to update yet, then don't
                return;
            }

            if (m_renderer == null)
            {
                return;
            }

            var image = m_renderer.Render(r, background);
            GUI.DrawTexture(r, image, ScaleMode.StretchToFill, false); // draw the RenderTexture in the ObjectPreview pane

            EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), 
            BlendShapeKey.CreateFrom((BlendShapeClip)target).ToString());
        }
    }
}
