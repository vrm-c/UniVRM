using System;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(BlendShapeClip))]
    public class BlendShapeClipEditor : PreviewEditor
    {
        SerializedBlendShapeEditor m_serializedEditor;

        BlendShapeClip m_target;

        SerializedProperty m_thumbnailProp;
        SerializedProperty m_isBinaryProp;

        protected override GameObject GetPrefab()
        {
            return m_target.Prefab;
        }

        void OnPrefabChanged()
        {
            m_target.Prefab = Prefab;
            Bake(m_target.Values, m_target.MaterialValues, 1.0f);
        }

        protected override void OnEnable()
        {
            m_target = (BlendShapeClip)target;
            PrefabChanged += OnPrefabChanged;

            base.OnEnable();

            Bake(m_target.Values, m_target.MaterialValues, 1.0f);

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PrefabChanged -= OnPrefabChanged;
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

            if (m_serializedEditor == null)
            {
                m_serializedEditor = new SerializedBlendShapeEditor(serializedObject, PreviewSceneManager);
                m_thumbnailProp = serializedObject.FindProperty("Thumbnail");
                m_isBinaryProp = serializedObject.FindProperty("IsBinary");
            }

            int thumbnailSize = 96;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(m_thumbnailProp.objectReferenceValue, typeof(Texture), false,
                GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize));

            var changed = false;
            EditorGUILayout.BeginVertical();
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("Preview Weight");
            var previewSlider = EditorGUILayout.Slider(m_previewSlider, 0, 1.0f);
            GUI.enabled = PreviewTexture != null;
            if (GUILayout.Button("save thumbnail"))
            {
                //var ext = "jpg";
                var ext = "png";
                var asset = UnityPath.FromAsset(target);
                var path = EditorUtility.SaveFilePanel(
                               "save thumbnail",
                               asset.Parent.FullPath,
                               string.Format("{0}.{1}", asset.FileNameWithoutExtension, ext),
                               ext);
                if (!string.IsNullOrEmpty(path))
                {
                    var thumbnail = SaveResizedImage(PreviewTexture, UnityPath.FromFullpath(path),
                    BlendShapeClipDrawer.ThumbnailSize);
                    m_thumbnailProp.objectReferenceValue = thumbnail;
                    serializedObject.ApplyModifiedProperties();
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndVertical();

            if (m_isBinaryProp.boolValue)
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
            EditorGUILayout.Space();

            var result = m_serializedEditor.Draw();
            if ((changed || result.Changed) && PreviewSceneManager != null)
            {
                PreviewSceneManager.Bake(result.BlendShapeBindings, result.MaterialValueBindings, m_previewSlider);
            }
        }

        public override string GetInfoString()
        {
            return BlendShapeKey.CreateFrom((BlendShapeClip)target).ToString();
        }
    }
}
