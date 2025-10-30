using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace VRM
{
    /// <summary>
    /// Prefabをインスタンス化してPreviewに表示する
    /// 
    /// * https://github.com/Unity-Technologies/UnityCsReference/blob/11bcfd801fccd2a52b09bb6fd636c1ddcc9f1705/Editor/Mono/Inspector/ModelInspector.cs
    /// 
    /// </summary>
    public abstract class PreviewEditor : Editor
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

                    Bake();
                }
            }
        }

        protected abstract PreviewSceneManager.BakeValue GetBakeValue();

        /// <summary>
        /// Preview シーンに BlendShape と MaterialValue を適用する
        /// </summary>
        protected void Bake()
        {
            if (m_scene != null)
            {
                m_scene.Bake(GetBakeValue());
            }
        }

        protected virtual GameObject GetPrefab()
        {
            return BlendShapeClip.VrmPrefabSearch(target);
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
                m_scene.Clean();
                GameObject.DestroyImmediate(m_scene.gameObject);
                m_scene = null;
            }
        }

        protected static void Separator()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            //GUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            Prefab = (GameObject)EditorGUILayout.ObjectField("Preview Prefab", Prefab, typeof(GameObject), false);

            //Separator();
        }

        private static int sliderHash = "Slider".GetHashCode();
        float m_yaw = 180.0f;
        float m_pitch;
        Vector3 m_position = new Vector3(0, 0, -0.8f);

        // very important to override this, it tells Unity to render an ObjectPreview at the bottom of the inspector
        public override bool HasPreviewGUI() { return true; }

        public RenderTexture PreviewTexture;

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

            var src = r;

            var min = Mathf.Min(r.width, r.height);
            r.width = min;
            r.height = min;
            r.x = src.x + (src.width - min) / 2;
            r.y = src.y + (src.height - min) / 2;

            //previewDir = Drag2D(previewDir, r);
            {
                int controlId = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
                Event e = Event.current;
                switch (e.GetTypeForControl(controlId))
                {
                    case EventType.MouseDown:
                        if (r.Contains(e.mousePosition) && (double)r.width > 50.0)
                        {
                            GUIUtility.hotControl = controlId;
                            e.Use();
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
                            if (e.button == 2)
                            {
                                var shift = e.delta * (!e.shift ? 1f : 3f) / Mathf.Min(r.width, r.height);
                                m_position.x -= shift.x;
                                m_position.y += shift.y;
                                e.Use();
                                GUI.changed = true;
                            }
                            else if (
                                e.button == 0 ||
                                e.button == 1)
                            {
                                var shift = e.delta * (!e.shift ? 1f : 3f) / Mathf.Min(r.width, r.height) * 140f;
                                m_yaw += shift.x;
                                m_pitch += shift.y;
                                m_pitch = Mathf.Clamp(m_pitch, -90f, 90f);
                                e.Use();
                                GUI.changed = true;
                            }
                            break;
                        }
                        break;

                    case EventType.ScrollWheel:
                        if (r.Contains(e.mousePosition))
                        {
                            if (e.delta.y > 0)
                            {
                                m_position.z *= 1.1f;
                                Repaint();
                            }
                            else if (e.delta.y < 0)
                            {
                                m_position.z *= 0.9f;
                                Repaint();
                            }
                        }
                        break;
                }
                //return scrollPosition;
            }

            if (Event.current.type != EventType.Repaint)
            {
                // if we don't need to update yet, then don't
                return;
            }

            if (m_renderer != null && m_scene != null)
            {
                PreviewTexture = m_renderer.Render(r, background, m_scene, m_yaw, m_pitch, m_position) as RenderTexture;
                if (PreviewTexture != null)
                {
                    // draw the RenderTexture in the ObjectPreview pane
                    GUI.DrawTexture(r, PreviewTexture, ScaleMode.StretchToFill, false);
                }
            }
        }
    }
}
