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
        /// PreviewRenderUtilityを管理する。
        /// 
        /// * PreviewRenderUtility.m_cameraのUnityVersionによる切り分け
        /// 
        /// </summary>
        PreviewFaceRenderer m_renderer;

        /// <summary>
        /// Prefabをインスタンス化したシーンを管理する。
        /// 
        /// * ExpressionのBake
        /// * MaterialMorphの適用
        /// * Previewカメラのコントロール
        /// * Previewライティングのコントロール
        /// 
        /// </summary>
        PreviewSceneManager m_scene;
        PreviewSceneManager PreviewSceneManager
        {
            get { return m_scene; }
        }

        /// <summary>
        /// Previewシーンに表示するPrefab
        /// </summary>
        GameObject m_prefab;
        GameObject Prefab
        {
            get { return m_prefab; }
            set
            {
                if (m_prefab == value) return;

                //Debug.LogFormat("Prefab = {0}", value);
                m_prefab = value;

                if (m_scene != null)
                {
                    //Debug.LogFormat("OnDestroy");
                    GameObject.DestroyImmediate(m_scene.gameObject);
                    m_scene = null;
                }

                if (m_prefab != null)
                {
                    m_scene = UniVRM10.PreviewSceneManager.GetOrCreate(m_prefab);
                    if (m_scene != null)
                    {
                        m_scene.gameObject.SetActive(false);
                    }

                    Bake();
                }
            }
        }

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

        void OnEnable()
        {
            m_target = (VRM10Expression)target;
            m_renderer = new PreviewFaceRenderer();
            Prefab = GetPrefab();
        }

        void OnDisable()
        {
            if (m_renderer != null)
            {
                m_renderer.Dispose();
                m_renderer = null;
            }
            ClearScene();
        }

        void OnDestroy()
        {
            // 2018/2019 で OnDisable/OnDestroy の呼ばれ方が違う？
            ClearScene();
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

        public static VRM10Expression CreateExpression(string path)
        {
            //Debug.LogFormat("{0}", path);
            var clip = ScriptableObject.CreateInstance<VRM10Expression>();
            clip.ExpressionName = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.ImportAsset(path);
            return clip;
            //Clips.Add(clip);
            //EditorUtility.SetDirty(this);
            //AssetDatabase.SaveAssets();
        }

        SerializedExpressionEditor m_serializedEditor;

        VRM10Expression m_target;
        VRM10Expression CurrentExpression()
        {
            return m_target;
        }

        GameObject GetPrefab()
        {
            return m_target.Prefab;
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
            if (PreviewSceneManager == null)
            {
                return;
            }
            serializedObject.Update();

            if (m_serializedEditor == null)
            {
                m_serializedEditor = new SerializedExpressionEditor(serializedObject, PreviewSceneManager);
            }

            EditorGUILayout.BeginHorizontal();

            var changed = false;
            EditorGUILayout.BeginVertical();

            Prefab = (GameObject)EditorGUILayout.ObjectField("Preview Prefab", Prefab, typeof(GameObject), false);
            EditorGUILayout.LabelField("Preview Weight");
            var previewSlider = EditorGUILayout.Slider(m_previewSlider, 0, 1.0f);

            EditorGUILayout.EndVertical();

            if (m_serializedEditor.IsBinary)
            {
                previewSlider = Mathf.Round(previewSlider);
            }

            if (previewSlider != m_previewSlider)
            {
                m_previewSlider = previewSlider;
                changed = true;
            }

            EditorGUILayout.EndHorizontal();
            Separator();
            // EditorGUILayout.Space();

            if (m_serializedEditor.Draw(out VRM10Expression bakeValue))
            {
                changed = true;
            }

            if (changed && PreviewSceneManager != null)
            {
                PreviewSceneManager.Bake(bakeValue, m_previewSlider);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
