using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UniVRM10
{
    /// <summary>
    /// ExpressionAvatarEditorの部品
    /// </summary>
    class ExpressionClipSelector
    {
        VRM10ObjectExpression m_avatar;

        int m_mode;
        static readonly string[] MODES = new string[]{
            "Button",
            "List"
        };

        ReorderableExpressionList m_clipList;

        public VRM10Expression GetSelected()
        {
            var clips = m_avatar.Clips.ToArray();
            if (m_avatar == null || clips == null || clips.Length == 0)
            {
                return null;
            }
            if (m_selectedIndex < 0 || m_selectedIndex >= clips.Length)
            {
                return null;
            }
            return clips[m_selectedIndex];
        }

        public event Action<VRM10Expression> Selected;
        void RaiseSelected(int index)
        {
            m_clipList.Select(index);
            var clip = GetSelected();
            var handle = Selected;
            if (handle == null)
            {
                return;
            }
            handle(clip);
        }

        int m_selectedIndex;
        int SelectedIndex
        {
            get { return m_selectedIndex; }
            set
            {
                // これで更新するべし
                if (m_selectedIndex == value) return;
                m_selectedIndex = value;
                RaiseSelected(value);
            }
        }

        public ExpressionClipSelector(VRM10ObjectExpression avatar, string dir, SerializedObject serializedObject)
        {
            avatar.RemoveNullClip();

            m_avatar = avatar;

            var prop = serializedObject.FindProperty("Clips");
            m_clipList = new ReorderableExpressionList(serializedObject, prop, dir);

            m_clipList.Selected += (selected) =>
            {
                var clips = avatar.Clips.ToArray();
                SelectedIndex = Array.IndexOf(clips, selected);
            };
        }

        public void DrawGUI(string dir)
        {
            var backup = GUI.enabled;
            try
            {
                GUI.enabled = true;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Select Expression", EditorStyles.boldLabel);

                m_mode = GUILayout.Toolbar(m_mode, MODES);
                switch (m_mode)
                {
                    case 0:
                        SelectGUI(dir);
                        break;

                    case 1:
                        m_clipList.GUI();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            finally
            {
                GUI.enabled = backup;
            }
        }

        void SelectGUI(string dir)
        {
            if (m_avatar != null && m_avatar.Clips != null)
            {
                var array = m_avatar.Clips
                    .Select(x => x != null
                        ? ExpressionKey.CreateFromClip(x).ToString()
                        : "null"
                        ).ToArray();
                SelectedIndex = GUILayout.SelectionGrid(SelectedIndex, array, 4);
            }

            // if (GUILayout.Button("Add Expression"))
            // {
            //     var path = EditorUtility.SaveFilePanel(
            //                    "Create Expression",
            //                    dir,
            //                    string.Format("Expression#{0}.asset", m_avatar.Clips.Count),
            //                    "asset");
            //     if (!string.IsNullOrEmpty(path))
            //     {
            //         var clip = ExpressionEditorBase.CreateExpression(path.ToUnityRelativePath());
            //         //clip.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(target));

            //         m_avatar.Clips.Add(clip);
            //     }
            // }
        }

        public void DuplicateWarn()
        {
            var key = ExpressionKey.CreateFromClip(GetSelected());
            if (m_avatar.Clips.Where(x => key.Match(x)).Count() > 1)
            {
                EditorGUILayout.HelpBox("duplicate clip: " + key, MessageType.Error);
            }
        }
    }
}
