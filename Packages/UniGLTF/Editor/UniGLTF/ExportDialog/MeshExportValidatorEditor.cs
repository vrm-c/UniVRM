
using System;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    [CustomEditor(typeof(MeshExportValidator))]
    public class MeshExportValidatorEditor : Editor
    {
        MeshExportValidator m_target;

        private void OnEnable()
        {
            m_target = target as MeshExportValidator;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox($"Mesh size: {m_target.ExpectedExportByteSize / 1000000.0f:0.0} MByte", MessageType.Info);

            for (int i = 0; i < m_target.Meshes.Count; ++i)
            {
                DrawElement(i, m_target.Meshes[i]);
            }
        }

        static (Rect, Rect) LeftRight(float x, float y, float left, float right, float height)
        {
            return (
                new Rect(x, y, left, height),
                new Rect(x + left, y, right, height)
            );
        }

        void DrawElement(int i, UniGLTF.MeshExportInfo info)
        {
            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3 + 20));
            var col0 = 32;
            var (left, right) = LeftRight(r.x, r.y, col0, r.width - col0, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(left, $"{i,3}");

            using (new EnabledScope())
            {
                foreach (var (renderer, _) in info.Renderers)
                {
                    EditorGUI.ObjectField(right, renderer, info.Renderers.GetType(), true);
                }
                right.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.ObjectField(right, info.Mesh, info.Renderers.GetType(), true);
            }

            right.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(right, info.Summary);
        }
    }
}
