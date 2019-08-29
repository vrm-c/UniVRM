using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;


namespace VRM
{
    [CustomEditor(typeof(VRMBlendShapeProxy))]
    public class VRMBlendShapeProxyEditor : Editor
    {
        VRMBlendShapeProxy m_target;
        SkinnedMeshRenderer[] m_renderers;

        public class BlendShapeSlider
        {
            VRMBlendShapeProxy m_target;
            BlendShapeKey m_key;

            public BlendShapeSlider(VRMBlendShapeProxy target, BlendShapeKey key)
            {
                m_target = target;
                m_key = key;
            }

            public KeyValuePair<BlendShapeKey, float> Slider()
            {
                var oldValue = m_target.GetValue(m_key);
                var enable = GUI.enabled;
                GUI.enabled = Application.isPlaying;
                var newValue = EditorGUILayout.Slider(m_key.ToString(), oldValue, 0, 1.0f);
                GUI.enabled = enable;
                return new KeyValuePair<BlendShapeKey, float>(m_key, newValue);
            }
        }
        List<BlendShapeSlider> m_sliders;

        void OnEnable()
        {
            m_target = (VRMBlendShapeProxy)target;
            if (m_target.BlendShapeAvatar != null && m_target.BlendShapeAvatar.Clips != null)
            {
                m_sliders = m_target.BlendShapeAvatar.Clips
                    .Where(x => x != null)
                    .Select(x => new BlendShapeSlider(m_target, BlendShapeKey.CreateFrom(x)))
                    .ToList()
                    ;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enable when playing", MessageType.Info);
            }

            if (m_target.BlendShapeAvatar == null)
            {
                return;
            }

            if (m_sliders != null)
            {
                m_target.SetValues(m_sliders.Select(x => x.Slider()));
            }
        }
    }
}
