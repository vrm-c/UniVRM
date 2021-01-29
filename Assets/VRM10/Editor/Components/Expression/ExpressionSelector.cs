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
        VRM10ExpressionAvatar m_avatar;

        int m_mode;
        static readonly string[] MODES = new string[]{
            "Button",
            "List"
        };

        ReorderableExpressionList m_clipList;

        public VRM10Expression GetSelected()
        {
            if (m_avatar == null || m_avatar.Clips == null || m_avatar.Clips.Count == 0)
            {
                return null;
            }
            if (m_selectedIndex < 0 || m_selectedIndex >= m_avatar.Clips.Count)
            {
                return null;
            }
            return m_avatar.Clips[m_selectedIndex];
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

        public ExpressionClipSelector(VRM10ExpressionAvatar avatar, SerializedObject serializedObject)
        {
            avatar.RemoveNullClip();

            m_avatar = avatar;

            var prop = serializedObject.FindProperty("Clips");
            m_clipList = new ReorderableExpressionList(serializedObject, prop, avatar);
            m_clipList.Selected += (selected) =>
            {
                SelectedIndex = avatar.Clips.IndexOf(selected);
            };
        }

        public void DrawGUI()
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
                        SelectGUI();
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

        void SelectGUI()
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

            if (GUILayout.Button("Add Expression"))
            {
                var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(m_avatar));
                var path = EditorUtility.SaveFilePanel(
                               "Create Expression",
                               dir,
                               string.Format("Expression#{0}.asset", m_avatar.Clips.Count),
                               "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    var clip = VRM10ExpressionAvatar.CreateExpression(path.ToUnityRelativePath());
                    //clip.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(target));

                    m_avatar.Clips.Add(clip);
                }
            }
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
