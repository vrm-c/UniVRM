using UnityEditor;
using UnityEngine;

namespace VRM
{
    class BlendShapeSlider
    {
        VRMBlendShapeProxy m_target;
        BlendShapeKey m_key;
        float? m_editorValue;

        public BlendShapeSlider(VRMBlendShapeProxy target, BlendShapeKey key)
        {
            m_target = target;
            m_key = key;
        }

        public void Slider()
        {
            if (m_target.BlendShapeAvatar == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                var oldValue = m_target.GetValue(m_key);
                var newValue = EditorGUILayout.Slider(m_key.ToString(), oldValue, 0, 1.0f);
                if (oldValue != newValue)
                {
                    m_target.SetValue(m_key, newValue);
                }
            }
            else
            {
                var oldValue = m_editorValue.HasValue ? m_editorValue.Value : 0.0f;
                var newValue = EditorGUILayout.Slider(m_key.ToString(), oldValue, 0, 1.0f);
                if (oldValue != newValue)
                {
                    var clip = m_target.BlendShapeAvatar.GetClip(m_key);
                    if (clip != null)
                    {
                        clip.Apply(m_target.transform, newValue);
                        m_editorValue = newValue;
                    }
                }
            }
        }

        public void RestoreEditorValue()
        {
            if (m_editorValue.HasValue)
            {
                var clip = m_target.BlendShapeAvatar.GetClip(m_key);
                if (clip != null)
                {
                    clip.Apply(m_target.transform, m_editorValue.Value);
                }
            }
        }
    }
}
