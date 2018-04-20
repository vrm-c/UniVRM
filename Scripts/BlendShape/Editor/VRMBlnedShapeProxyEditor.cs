using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;


namespace VRM
{
    [CustomEditor(typeof(VRMBlendShapeProxy))]
    public class VRMBlnedShapeProxyEditor : Editor
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

            public void Slider()
            {
                if (m_target.BlendShapeAvatar == null)
                {
                    return;
                }

                var oldValue = m_target.GetValue(m_key);
                var newValue = EditorGUILayout.Slider(m_key.ToString(), oldValue, 0, 1.0f);
                if (oldValue != newValue)
                {
                    m_target.SetValue(m_key, newValue);
                }
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

            if (m_sliders != null)
            {
                foreach (var slider in m_sliders)
                {
                    slider.Slider();
                }
            }
        }
    }
}
