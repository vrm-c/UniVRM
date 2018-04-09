using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    public class PreviewEditor : Editor
    {
        PreviewSceneManager m_scene;
        protected PreviewSceneManager PreviewSceneManager
        {
            get { return m_scene; }
        }

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
                    m_scene = VRM.PreviewSceneManager.GetOrCreate(m_prefab);
                    if (m_scene != null)
                    {
                        m_scene.gameObject.SetActive(false);
                    }
                }
            }
        }

        protected void Bake(BlendShapeBinding[] values, MaterialValueBinding[] materialValues, float weight)
        {
            if (m_scene != null)
            {
                m_scene.Bake(values, materialValues, weight);
            }
        }

        protected virtual void OnEnable()
        {
            m_renderer = new PreviewFaceRenderer();
            var assetPath = AssetDatabase.GetAssetPath(target);
            //Debug.LogFormat("assetPath: {0}", assetPath);
            if (!string.IsNullOrEmpty(assetPath))
            {
                Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            }
        }

        protected virtual void OnDisable()
        {
            if (m_renderer != null)
            {
                m_renderer.Dispose();
                m_renderer = null;
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_scene != null)
            {
                //Debug.LogFormat("OnDestroy");
                m_scene.Clean();
                GameObject.DestroyImmediate(m_scene.gameObject);
                m_scene = null;
            }
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            Prefab = (GameObject)EditorGUILayout.ObjectField("prefab", Prefab, typeof(GameObject), false);
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
    }
}
