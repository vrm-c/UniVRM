using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using UniGLTF;

namespace VRM
{
    class BlendShapeClipSelector
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

        public BlendShapeClipSelector(BlendShapeAvatar avatar, Action<BlendShapeClip> onSelected)
        {
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
                        ? BlendShapeKey.CreateFromClip(x).ToString()
                        : "null"
                        ).ToArray();
                SelectedIndex = GUILayout.SelectionGrid(SelectedIndex, array, 4);
            }

            if (GUILayout.Button("Create BlendShapeClip"))
            {
                var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(m_avatar));
                var path = EditorUtility.SaveFilePanel(
                               "Create BlendShapeClip",
                               dir,
                               string.Format("BlendShapeClip#{0}.asset", m_avatar.Clips.Count),
                               "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    var clip = BlendShapeAvatar.CreateBlendShapeClip(path.ToUnityRelativePath());
                    //clip.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(target));

                    m_avatar.Clips.Add(clip);

                    // save clips
                    EditorUtility.SetDirty(m_avatar);
                }
            }
        }

        public void DuplicateWarn()
        {
            var key = BlendShapeKey.CreateFromClip(Selected);
            if (m_avatar.Clips.Where(x => key.Match(x)).Count() > 1)
            {
                EditorGUILayout.HelpBox("duplicate clip: " + key, MessageType.Error);
            }
        }
    }

}