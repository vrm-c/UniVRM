using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMBlendShapeProxy))]
    public class VRMBlnedShapeProxyEditor : Editor
    {
        VRMBlendShapeProxy m_target;
        SkinnedMeshRenderer[] m_renderers;
        List<BlendShapeSlider> m_sliders;

        void OnEnable()
        {
            m_target = (VRMBlendShapeProxy)target;

            if (m_target.BlendShapeAvatar != null)
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
