using System;
using System.IO;
using MeshUtility;
using UnityEditor;
using UnityEngine;


namespace UniVRM10
{
    [CustomEditor(typeof(VRM10Expression))]
    public class ExpressionEditor : ExpressionEditorBase
    {
        SerializedExpressionEditor m_serializedEditor;

        VRM10Expression m_target;
        protected override VRM10Expression CurrentExpression()
        {
            return m_target;
        }

        protected override GameObject GetPrefab()
        {
            return m_target.Prefab;
        }

        protected override void OnEnable()
        {
            m_target = (VRM10Expression)target;

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
                m_serializedEditor = new SerializedExpressionEditor(serializedObject, PreviewSceneManager);
            }

            EditorGUILayout.BeginHorizontal();

            var changed = false;
            EditorGUILayout.BeginVertical();
            base.OnInspectorGUI();
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
        }
    }
}
