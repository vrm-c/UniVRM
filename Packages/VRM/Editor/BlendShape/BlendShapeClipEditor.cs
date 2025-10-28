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
        protected override PreviewSceneManager.BakeValue GetBakeValue()
        {
            return new PreviewSceneManager.BakeValue
            {
                BlendShapeBindings = m_target.Values,
                MaterialValueBindings = m_target.MaterialValues,
                Weight = 1.0f,
            };
        }

        //SerializedProperty m_thumbnailProp;
        SerializedProperty m_isBinaryProp;

        protected override GameObject GetPrefab()
        {
            return m_target.Prefab;
        }

        protected override void OnEnable()
        {
            m_target = (BlendShapeClip)target;

            base.OnEnable();
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
                m_serializedEditor = new SerializedBlendShapeEditor(serializedObject, PreviewSceneManager);
                //m_thumbnailProp = serializedObject.FindProperty("Thumbnail");
                m_isBinaryProp = serializedObject.FindProperty("IsBinary");
            }

            EditorGUILayout.BeginHorizontal();

            /*
            int thumbnailSize = 96;
            var objectReferenceValue = EditorGUILayout.ObjectField(m_thumbnailProp.objectReferenceValue, typeof(Texture), false,
                GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize));
            if (m_thumbnailProp.objectReferenceValue != objectReferenceValue)
            {
                m_thumbnailProp.objectReferenceValue = objectReferenceValue;
                serializedObject.ApplyModifiedProperties();
            }
            */

            var changed = false;
            EditorGUILayout.BeginVertical();
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("Preview Weight");
            var previewSlider = EditorGUILayout.Slider(m_previewSlider, 0, 1.0f);
            GUI.enabled = PreviewTexture != null;
            /*
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
            */
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
                PreviewSceneManager.Bake(new PreviewSceneManager.BakeValue
                {
                    BlendShapeBindings = result.BlendShapeBindings,
                    MaterialValueBindings = result.MaterialValueBindings,
                    Weight = m_previewSlider
                });
            }
        }

        public override string GetInfoString()
        {
            return BlendShapeKey.CreateFromClip((BlendShapeClip)target).ToString();
        }
    }
}
