using System;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace VRM
{
    class BlendShapeClipSelect
    {
        BlendShapeAvatar m_avatar;

        public BlendShapeClip Selected
        {
            get
            {
                if (m_avatar == null || m_avatar.Clips == null)
                {
                    return null;
                }
                if (m_selectedIndex < 0 || m_selectedIndex >= m_avatar.Clips.Count)
                {
                    return null;
                }
                return m_avatar.Clips[m_selectedIndex];
            }
        }

        int m_selectedIndex;
        int SelectedIndex
        {
            get { return m_selectedIndex; }
            set
            {
                if (m_selectedIndex == value) return;
                m_selectedIndex = value;
                if (m_onSelected != null)
                {
                    m_onSelected(Selected);
                }
            }
        }

        Action<BlendShapeClip> m_onSelected;

        public BlendShapeClipSelect(BlendShapeAvatar avatar, Action<BlendShapeClip> onSelected)
        {
            avatar.RemoveNullClip();

            m_avatar = avatar;
            m_onSelected = onSelected;

            onSelected(Selected);
        }

        public void SelectGUI()
        {
            if (m_avatar != null && m_avatar.Clips != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Select BlendShapeClip", EditorStyles.boldLabel);
                var array = m_avatar.Clips
                    .Select(x => x != null
                        ? BlendShapeKey.CreateFrom(x).ToString()
                        : "null"
                        ).ToArray();
                SelectedIndex = GUILayout.SelectionGrid(SelectedIndex, array, 4);
            }

            if (GUILayout.Button("Add BlendShapeClip"))
            {
                m_avatar.AddBlendShapeClip();
            }
        }

        public void DuplicateWarn()
        {
            var key = BlendShapeKey.CreateFrom(Selected);
            if (m_avatar.Clips.Where(x => key.Match(x)).Count() > 1)
            {
                EditorGUILayout.HelpBox("duplicate clip: " + key, MessageType.Error);
            }
        }
    }

}