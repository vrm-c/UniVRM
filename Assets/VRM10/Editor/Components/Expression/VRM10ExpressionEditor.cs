using System;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace UniVRM10
{
    [CustomEditor(typeof(VRM10Expression))]
    public class ExpressionEditor : Editor
    {
        /// <summary>
        /// Preview(Inspectorの下方)を描画するクラス
        /// 
        /// * PreviewRenderUtility.m_cameraのUnityVersionによる切り分け
        /// 
        /// </summary>
        PreviewFaceRenderer m_renderer;

        /// <summary>
        /// Previewを描画するのにシーンが必用である。
        /// m_target.Prefabをインスタンス化したシーンを管理する。
        /// 
        /// * ExpressionのBake
        /// * MaterialMorphの適用
        /// * Previewカメラのコントロール
        /// * Previewライティングのコントロール
        /// 
        /// </summary>
        PreviewSceneManager m_scene;

        /// <summary>
        /// Preview シーンに Expression を適用する
        /// </summary>
        void Bake()
        {
            if (m_scene != null)
            {
                //Debug.Log("Bake");
                m_scene.Bake(CurrentExpression(), 1.0f);
            }
        }

        void ClearScene()
        {
            if (m_scene != null)
            {
                //Debug.LogFormat("OnDestroy");
                m_scene.Clean();
                GameObject.DestroyImmediate(m_scene.gameObject);
                m_scene = null;
            }
        }

        void PrefabGUI()
        {
            var prefab = (GameObject)EditorGUILayout.ObjectField("Preview Prefab", m_target.Prefab, typeof(GameObject), false);
            if (prefab == m_target.Prefab)
            {
                return;
            }
            ClearPreview();
            m_target.Prefab = prefab;
            Initialize();
        }

        void OnEnable()
        {
            Initialize();
        }

        void OnDisable()
        {
            ClearPreview();
        }

        void Initialize()
        {
            m_target = (VRM10Expression)target;
            m_renderer = new PreviewFaceRenderer();
        }

        void ClearPreview()
        {
            if (m_renderer != null)
            {
                m_renderer.Dispose();
                m_renderer = null;
            }

            m_serializedEditor = null;
            ClearScene();
        }

        void OnDestroy()
        {
            // 2018/2019 で OnDisable/OnDestroy の呼ばれ方が違う？
            ClearScene();
        }

        static void Separator()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            //GUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
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
                        //Debug.LogFormat("wheel: {0}", current.delta);
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
            //Debug.LogFormat("{0}", previewDir);

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

        SerializedExpressionEditor m_serializedEditor;

        VRM10Expression m_target;
        VRM10Expression CurrentExpression()
        {
            return m_target;
        }

        float m_previewSlider = 1.0f;

        static Texture2D SaveResizedImage(RenderTexture rt, UnityPath path, int size)
        {
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            //TextureScale.Scale(tex, size, size);
            tex = TextureScale.GetResized(tex, size, size);

            byte[] bytes;
            switch (path.Extension.ToLower())
            {
                case ".png":
                    bytes = tex.EncodeToPNG();
                    break;

                case ".jpg":
                    bytes = tex.EncodeToJPG();
                    break;

                default:
                    throw new Exception();
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(tex);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(tex);
            }
            File.WriteAllBytes(path.FullPath, bytes);

            path.ImportAsset();
            return path.LoadAsset<Texture2D>();
        }

        public override void OnInspectorGUI()
        {
            var changed = false;
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            PrefabGUI();
            EditorGUILayout.LabelField("Preview Weight");
            var previewSlider = EditorGUILayout.Slider(m_previewSlider, 0, 1.0f);
            if (m_target.IsBinary)
            {
                previewSlider = Mathf.Round(previewSlider);
            }
            if (previewSlider != m_previewSlider)
            {
                m_previewSlider = previewSlider;
                changed = true;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            Separator();

            if (m_scene == null)
            {
                if (m_target.Prefab != null)
                {
                    m_scene = UniVRM10.PreviewSceneManager.GetOrCreate(m_target.Prefab);
                    if (m_scene != null)
                    {
                        m_scene.gameObject.SetActive(false);
                    }
                    Bake();
                }
            }

            if (m_scene != null)
            {
                if (m_serializedEditor == null)
                {
                    m_serializedEditor = new SerializedExpressionEditor(serializedObject, m_scene);
                }
                if (m_serializedEditor.Draw(out VRM10Expression bakeValue))
                {
                    changed = true;
                }
                if (changed)
                {
                    m_scene.Bake(bakeValue, m_previewSlider);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override string GetInfoString()
        {
            if (m_scene == null) return "";
            return m_scene.hasError ? "An error occurred while previewing. Check the console log for details." : "";
        }
    }
}
