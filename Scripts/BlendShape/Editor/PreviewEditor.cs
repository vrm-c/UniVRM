using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;

namespace VRM
{
    /// <summary>
    /// Prefabをインスタンス化してPreviewに表示する
    /// 
    /// * https://github.com/Unity-Technologies/UnityCsReference/blob/11bcfd801fccd2a52b09bb6fd636c1ddcc9f1705/Editor/Mono/Inspector/ModelInspector.cs
    /// 
    /// </summary>
    public class PreviewEditor : Editor
    {
        /// <summary>
        /// PreviewRenderUtilityを管理する。
        /// 
        /// * PreviewRenderUtility.m_cameraのUnityVersionによる切り分け
        /// 
        /// </summary>
        PreviewFaceRenderer m_renderer;

        /// <summary>
        /// Prefabをインスタンス化したシーンを管理する。
        /// 
        /// * BlendShapeのBake
        /// * MaterialMorphの適用
        /// * Previewカメラのコントロール
        /// * Previewライティングのコントロール
        /// 
        /// </summary>
        PreviewSceneManager m_scene;
        protected PreviewSceneManager PreviewSceneManager
        {
            get { return m_scene; }
        }

        /// <summary>
        /// Previewシーンに表示するPrefab
        /// </summary>
        GameObject m_prefab;
        protected GameObject Prefab
        {
            get { return m_prefab; }
            private set
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
                    RaisePrefabChanged();
                }
            }
        }
        protected event Action PrefabChanged;
        void RaisePrefabChanged()
        {
            var handler = PrefabChanged;
            if (handler == null) return;
            handler();
        }

        /// <summary>
        /// シーンにBlendShapeとMaterialMorphを適用する
        /// </summary>
        /// <param name="values"></param>
        /// <param name="materialValues"></param>
        /// <param name="weight"></param>
        protected void Bake(BlendShapeBinding[] values, MaterialValueBinding[] materialValues, float weight)
        {
            if (m_scene != null)
            {
                Debug.Log("Bake");
                m_scene.Bake(values, materialValues, weight);
            }
        }

        protected virtual GameObject GetPrefab()
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }

        protected virtual void OnEnable()
        {
            m_renderer = new PreviewFaceRenderer();
            Prefab = GetPrefab();
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

        private static int sliderHash = "Slider".GetHashCode();
        Vector2 m_previewDir;
        float m_distance = 1.0f;

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
                    EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), 
                        "Mesh preview requires\nrender texture support");
                }
                return;
            }

            //previewDir = Drag2D(previewDir, r);
            {
                int controlId = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
                Event current = Event.current;
                switch (current.GetTypeForControl(controlId))
                {
                    case EventType.MouseDown:
                        if (r.Contains(current.mousePosition) && (double)r.width > 50.0)
                        {
                            GUIUtility.hotControl = controlId;
                            current.Use();
                            EditorGUIUtility.SetWantsMouseJumping(1);
                            break;
                        }
                        break;

                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlId)
                            GUIUtility.hotControl = 0;
                        EditorGUIUtility.SetWantsMouseJumping(0);
                        break;

                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlId)
                        {
                            m_previewDir -= current.delta * (!current.shift ? 1f : 3f) / Mathf.Min(r.width, r.height) * 140f;
                            m_previewDir.y = Mathf.Clamp(m_previewDir.y, -90f, 90f);
                            current.Use();
                            GUI.changed = true;
                            break;
                        }
                        break;

                    case EventType.ScrollWheel:
                        //Debug.LogFormat("wheel: {0}", current.delta);
                        if (r.Contains(current.mousePosition)){
                            if (current.delta.y > 0)
                            {
                                m_distance *= 1.1f;
                                Repaint();
                            }
                            else if (current.delta.y < 0)
                            {
                                m_distance *= 0.9f;
                                Repaint();
                            }
                        }
                        break;
                }
                //return scrollPosition;
            }
            //Debug.LogFormat("{0}", previewDir);

            if (Event.current.type != EventType.Repaint)
            {
                // if we don't need to update yet, then don't
                return;
            }

            if (m_renderer != null && m_scene != null)
            {
                var texture = m_renderer.Render(r, background, m_scene, m_previewDir, m_distance);
                if (texture != null)
                {
                    // draw the RenderTexture in the ObjectPreview pane
                    GUI.DrawTexture(r, texture, ScaleMode.StretchToFill, false);
                }
            }
        }
    }
}
