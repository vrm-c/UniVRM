using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMBlendShapeProxy))]
    public class VRMBlnedShapeProxyEditor : Editor
    {
        VRMBlendShapeProxy m_target;
        SkinnedMeshRenderer[] m_renderers;

        VRMLookAtHead m_lookAt;

        #region Preview
        class PreviewRenderer
        {
            PreviewRenderUtility m_previewRenderUtility;

            public PreviewRenderer()
            {
                //true にすることでシーン内のゲームオブジェクトを描画できるようになる
                m_previewRenderUtility = new PreviewRenderUtility(true);

#if UNITY_2017_3_OR_NEWER
            //FieldOfView を 30 にするとちょうどいい見た目になる
            m_previewRenderUtility.cameraFieldOfView = 30f;

            //必要に応じて nearClipPlane と farClipPlane を設定
            m_previewRenderUtility.camera.nearClipPlane = 0.3f;
            m_previewRenderUtility.camera.farClipPlane = 1000;
#else
                //FieldOfView を 30 にするとちょうどいい見た目になる
                m_previewRenderUtility.m_CameraFieldOfView = 30f;

                //必要に応じて nearClipPlane と farClipPlane を設定
                m_previewRenderUtility.m_Camera.nearClipPlane = 0.3f;
                m_previewRenderUtility.m_Camera.farClipPlane = 1000;
#endif
            }

            public Camera Camera
            {
                get
                {
#if UNITY_2017_3_OR_NEWER
            var previewCamera = m_previewRenderUtility.camera;
#else
                    var previewCamera = m_previewRenderUtility.m_Camera;
#endif
                    return previewCamera;
                }
            }

            public void Render(Rect r, GUIStyle background)
            {
                var previewCamera = Camera;
                if (previewCamera == null)
                {
                    // 無い時がある
                    return;
                }
                m_previewRenderUtility.BeginPreview(r, background);
                previewCamera.Render();
                m_previewRenderUtility.EndAndDrawPreview(r);
            }
        }
        PreviewRenderer m_preview;
        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("BlendShapeProxy");
        }

        public override void OnPreviewSettings()
        {
            /*
            GUIStyle preLabel = new GUIStyle("preLabel");
            GUIStyle preButton = new GUIStyle("preButton");
            GUILayout.Label("ラベル", preLabel);
            GUILayout.Button("ボタン", preButton);
            */
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (m_lookAt == null) return;
            if (m_lookAt.Head.Transform == null) return;
            m_lookAt.LookFace(m_preview.Camera.transform);
            m_preview.Render(r, background);
            //描画タイミングが少ないことによって
            //カクつきがきになる時は Repaint を呼び出す（高負荷）
            //Repaint ();
        }
        #endregion

        void OnEnable()
        {
            m_target = (VRMBlendShapeProxy)target;

            if (m_target.BlendShapeAvatar == null) return;
            m_currentClip = m_target.BlendShapeAvatar.GetClip(BlendShapePreset.Neutral.ToString());

            m_renderers = m_target.transform
                .Traverse()
                .Select(x => x.GetComponent<SkinnedMeshRenderer>())
                .Where(x => x != null)
                .ToArray()
                ;

            m_sliders = m_target.BlendShapeAvatar.Clips
                .Where(x => x != null)
                .Select(x => new BlendShapeSlider(m_target, BlendShapeKey.CreateFrom(x)))
                .ToList()
                ;

            m_lookAt = m_target.GetComponent<VRMLookAtHead>();
            m_preview = new PreviewRenderer();
        }

        private void OnDisable()
        {
            if (m_mode == EditorMode.Editor)
            {
                ClearBlendShape();
            }
        }

        enum EditorMode
        {
            Runtime,
            Editor,
        }
        EditorMode m_mode;
        static string[] MODES = ((EditorMode[])Enum.GetValues(typeof(EditorMode)))
            .Select(x => x.ToString())
            .ToArray();

        public override void OnInspectorGUI()
        {
            var mode =
                (EditorMode)GUILayout.Toolbar((int)m_mode, MODES);

            switch (mode)
            {
                case EditorMode.Runtime:
                    RuntimeInspector(mode != m_mode);
                    break;

                case EditorMode.Editor:
                    EditorInspector(mode != m_mode);
                    break;

                default:
                    throw new NotImplementedException();
            }

            m_mode = mode;
        }

        #region RuntimeInspector
        List<BlendShapeSlider> m_sliders;

        void RuntimeInspector(bool changed)
        {
            if (changed)
            {
                ClearBlendShape();
                if (Application.isPlaying)
                {
                    m_target.Restore();
                    m_target.Apply();
                }
                else
                {
                    if (m_sliders != null)
                    {
                        foreach (var x in m_sliders)
                        {
                            x.RestoreEditorValue();
                        }
                    }
                }
            }

            base.OnInspectorGUI();

            if (m_sliders != null)
            {
                foreach (var slider in m_sliders)
                {
                    slider.Slider();
                }
            }
        }
        #endregion

        #region EditorInspector
        BlendShapeClip m_currentClip;
        BlendShapeClip CurrentClip
        {
            get { return m_currentClip; }
            set
            {
                if (m_currentClip == value) return;

                m_currentClip = value;
                ClearBlendShape();
                Apply();
            }
        }
        void Apply()
        {
            if (m_currentClip != null)
            {
                m_currentClip.Apply(m_target.transform, 1.0f);
            }
        }

        static String[] Presets = ((BlendShapePreset[])Enum.GetValues(typeof(BlendShapePreset)))
            .Select(x => x.ToString()).ToArray();

        int m_preset;
        void EditorInspector(bool changed)
        {
            if (changed)
            {
                ClearBlendShape();
                Apply();
            }

            m_target.BlendShapeAvatar = (BlendShapeAvatar)EditorGUILayout.ObjectField("BlendShapeAvatar",
                m_target.BlendShapeAvatar, typeof(BlendShapeAvatar), false);
            if (m_target.BlendShapeAvatar == null) return;

            // buttons
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select BlendShapeClip", EditorStyles.boldLabel);
            var preset = GUILayout.SelectionGrid(m_preset, m_target.BlendShapeAvatar.Clips
                .Where(x => x != null)
                .Select(x => BlendShapeKey.CreateFrom(x).ToString()).ToArray(), 4);
            if (preset != m_preset)
            {
                CurrentClip = m_target.BlendShapeAvatar.Clips[preset];
                m_preset = preset;
            }

            // Add
            if (GUILayout.Button("Add BlendShapeClip"))
            {
                AddBlendShapeClip();
            }

            if (CurrentClip != null)
            {
                // clip
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("CurrentClip", EditorStyles.boldLabel);

                /*var loadClip = (BlendShapeClip)*/
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Current clip",
                    CurrentClip, typeof(BlendShapeClip), false);
                GUI.enabled = true;

                CurrentClip.Preset = (BlendShapePreset)EditorGUILayout.Popup("Preset", (int)CurrentClip.Preset, Presets);

                CurrentClip.BlendShapeName = EditorGUILayout.TextField("BlendShapeName", CurrentClip.BlendShapeName);

                var key = BlendShapeKey.CreateFrom(CurrentClip);
                if (m_target.BlendShapeAvatar.Clips.Where(x => key.Match(x)).Count() > 1)
                {
                    EditorGUILayout.HelpBox("duplicate clip", MessageType.Error);
                }

                // sliders
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("BlendShapeValues", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear"))
                {
                    ClearBlendShape();
                }
                if (CurrentClip != null && GUILayout.Button("Apply"))
                {
                    string maxWeightString;
                    CurrentClip.Values = GetBindings(out maxWeightString);
                }
                EditorGUILayout.EndHorizontal();

                foreach (var renderer in m_renderers)
                {
                    var mesh = renderer.sharedMesh;
                    if (mesh != null)
                    {
                        var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(renderer.transform, m_target.transform);
                        EditorGUILayout.LabelField(m_target.name + "/" + relativePath);

                        for (int i = 0; i < mesh.blendShapeCount; ++i)
                        {
                            var src = renderer.GetBlendShapeWeight(i);
                            var dst = EditorGUILayout.Slider(mesh.GetBlendShapeName(i), src, 0, 100.0f);
                            if (dst != src)
                            {
                                renderer.SetBlendShapeWeight(i, dst);

                            }
                        }
                    }
                }
            }
        }

        void AddBlendShapeClip()
        {
            var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(m_target.BlendShapeAvatar));
            var path = EditorUtility.SaveFilePanel(
                           "Create BlendShapeClip",
                           dir,
                           string.Format("BlendShapeClip#{0}.asset", m_target.BlendShapeAvatar.Clips.Count),
                           "asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            path = path.ToUnityRelativePath();
            Debug.LogFormat("{0}", path);
            var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
            m_target.BlendShapeAvatar.Clips.Add(clip);
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.ImportAsset(path);
        }

        BlendShapeBinding[] GetBindings(out string _maxWeightName)
        {
            var maxWeight = 0.0f;
            var maxWeightName = "";
            // weightのついたblendShapeを集める
            var values = m_renderers.SelectMany(x =>
             {
                 var mesh = x.sharedMesh;

                 var relativePath = UniGLTF.UnityExtensions.RelativePathFrom(x.transform, m_target.transform);

                 var list = new List<BlendShapeBinding>();
                 if (mesh != null)
                 {
                     for (int i = 0; i < mesh.blendShapeCount; ++i)
                     {
                         var weight = x.GetBlendShapeWeight(i);
                         if (weight == 0)
                         {
                             continue;
                         }
                         var name = mesh.GetBlendShapeName(i);
                         if (weight > maxWeight)
                         {
                             maxWeightName = name;
                             maxWeight = weight;
                         }
                         list.Add(new BlendShapeBinding
                         {
                             Index = i,
                             RelativePath = relativePath,
                             Weight = weight
                         });
                     }
                 }
                 return list;
             }).ToArray()
            ;
            _maxWeightName = maxWeightName;
            return values;
        }

        private void ClearBlendShape()
        {
            if (m_renderers == null) return;
            foreach (var renderer in m_renderers)
            {
                var mesh = renderer.sharedMesh;
                if (mesh != null)
                {
                    for (int i = 0; i < mesh.blendShapeCount; ++i)
                    {
                        renderer.SetBlendShapeWeight(i, 0);
                    }
                }
            }
        }
        #endregion
    }
}
