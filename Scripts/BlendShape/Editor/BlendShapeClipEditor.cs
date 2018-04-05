using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeClip))]
    public class BlendShapeClipEditor : Editor
    {
        GameObject m_prefab;
        PreviewFaceRenderer m_renderer;
        BlendShapeClip m_target;

        private void OnEnable()
        {
            m_target = (BlendShapeClip)target;
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
            base.OnInspectorGUI();

            //攻撃力の数値をラベルとして表示する
            //EditorGUILayout.LabelField("攻撃力", character.攻撃力.ToString());
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
                BlendShapeKey.CreateFrom(m_target).ToString());
        }
    }
}
