
using System;
using UnityEditor;
using UnityEngine;
using VRM.M17N;

namespace VRM
{
    [CustomEditor(typeof(VRMExportMeshes))]
    public class VRMExportMeshesEditor : Editor
    {
        VRMExportMeshes m_target;

        private void OnEnable()
        {
            m_target = target as VRMExportMeshes;
        }

        public override void OnInspectorGUI()
        {
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

            GUI.enabled = false;
            EditorGUI.ObjectField(right, info.Renderer, info.Renderer.GetType(), true);

            right.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.ObjectField(right, info.Mesh, info.Renderer.GetType(), true);
            GUI.enabled = true;

            right.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(right, info.Summary);
        }
    }
}
