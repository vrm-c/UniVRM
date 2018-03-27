using System;
using System.Collections.Generic;
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
        VRMLookAt m_lookAt;

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
            m_loadClip = m_target.BlendShapeAvatar.GetClip(BlendShapePreset.Neutral.ToString());

            m_renderers = m_target.transform
                .Traverse()
                .Select(x => x.GetComponent<SkinnedMeshRenderer>())
                .Where(x => x != null)
                .ToArray()
                ;

            m_sliders = m_target.BlendShapeAvatar.Clips
                .Where(x => x!=null)
                .Select(x => new BlendShapeSlider(m_target, BlendShapeKey.CreateFrom(x)))
                .ToList()
                ;

            m_lookAt = m_target.GetComponent<VRMLookAt>();
            m_preview = new PreviewRenderer();
        }

        private void OnDisable()
        {
            if(m_mode==EditorMode.Editor)
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
                    m_target.Reload();
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
        BlendShapeClip m_loadClip;
        BlendShapeClip LoadClip
        {
            get { return m_loadClip; }
            set
            {
                if (m_loadClip == value) return;

                m_loadClip = value;
                ClearBlendShape();
                Apply();
            }
        }
        void Apply()
        {
            if (m_loadClip != null)
            {
                m_loadClip.Apply(m_target.transform, 1.0f);
            }
        }

        int m_preset;
        void EditorInspector(bool changed)
        {
            if (changed)
            {
                ClearBlendShape();
                Apply();
            }   
            
            if (m_target.BlendShapeAvatar == null) return;

            if (GUILayout.Button("Clear"))
            {
                ClearBlendShape();
            }

            /*
            if (GUILayout.Button("Create BlendShapeClip"))
            {
                CreateBlendShapeClip();
            }
            */

            GUILayout.BeginHorizontal();
            if (m_loadClip != null && GUILayout.Button("Apply"))
            {
                /*var loadClip = (BlendShapeClip)*/
                EditorGUILayout.ObjectField("Load clip",
                    LoadClip, typeof(BlendShapeClip), false);
                string maxWeightString;
                m_loadClip.Values = GetBindings(out maxWeightString);
            }
            GUILayout.EndHorizontal();

            // buttons
            var preset = GUILayout.SelectionGrid(m_preset, m_target.BlendShapeAvatar.Clips
                .Where(x => x!=null)
                .Select(x => BlendShapeKey.CreateFrom(x).ToString()).ToArray(), 4);
            if (preset != m_preset)
            {
                LoadClip = m_target.BlendShapeAvatar.Clips[preset];
                m_preset = preset;
            }

            EditorGUILayout.Space();

            // sliders
            if (m_loadClip != null)
            {
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

        BlendShapeBinding[] GetBindings(out string _maxWeightName)
        {
            var maxWeight = 0.0f;
            var maxWeightName = "";
            // weightのついたblendShapeを集める
            var values= m_renderers.SelectMany(x =>
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
