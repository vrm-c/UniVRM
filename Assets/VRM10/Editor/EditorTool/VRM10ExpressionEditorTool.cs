using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UniGLTF;

#if UNITY_2021_OR_NEWER
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UniVRM10
{
    [EditorTool("vrm-1.0/Expression", typeof(UniVRM10.Vrm10Instance))]
    class VRM10ExpressionEditorTool : EditorTool
    {
        static GUIContent s_cachedIcon;
        public override GUIContent toolbarIcon
        {
            get
            {
                if (s_cachedIcon == null)
                {
                    s_cachedIcon = EditorGUIUtility.IconContent("d_Audio Mixer@2x", "|vrm-1.0 Expression");
                }
                return s_cachedIcon;
            }
        }

        void OnEnable()
        {
            ToolManager.activeToolChanged += ActiveToolDidChange;
        }

        void OnDisable()
        {
            ToolManager.activeToolChanged -= ActiveToolDidChange;
        }

        void ActiveToolDidChange()
        {
            if (!ToolManager.IsActiveTool(this))
            {
                return;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (Selection.activeTransform == null)
            {
                return;
            }
            var root = Selection.activeTransform.GetComponentOrNull<Vrm10Instance>();
            if (root == null)
            {
                return;
            }

            Handles.BeginGUI();
            if (Application.isPlaying)
            {
                ExpressionPreviewInPlay(root?.Vrm?.Expression, root?.Runtime.Expression);
            }
            else
            {
                EditorGUILayout.HelpBox("expression preview in play mode", MessageType.Warning);
            }
            Handles.EndGUI();
        }

        void ExpressionPreviewInPlay(VRM10ObjectExpression expression, Vrm10RuntimeExpression runtime)
        {
            if (expression == null)
            {
                EditorGUILayout.HelpBox("no expression settings", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                // 右よせ
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();

                    m_map.Clear();
                    foreach (var kv in runtime.GetWeights())
                    {
                        var key = kv.Key;
                        var value = ExpressionSlider(key, kv.Value);
                        m_map[key] = value;
                    }
                    GUILayout.FlexibleSpace();

                    runtime.SetWeights(m_map);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        Dictionary<ExpressionKey, float> m_map = new Dictionary<ExpressionKey, float>();
        static GUIStyle s_style;
        static GUIStyle Style
        {
            get
            {
                if (s_style == null)
                {
                    s_style = GUI.skin.GetStyle("box");
                }
                return s_style;
            }
        }

        float ExpressionSlider(ExpressionKey key, float value)
        {
            EditorGUILayout.BeginHorizontal(Style);
            EditorGUILayout.LabelField(key.ToString());
            value = EditorGUILayout.Slider(value, 0, 1.0f);
            EditorGUILayout.EndHorizontal();
            return value;
        }
    }
}
