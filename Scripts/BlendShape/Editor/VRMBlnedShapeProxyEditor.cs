using UnityEditor;
using UnityEngine;


namespace VRM
{
    [CustomEditor(typeof(VRMBlendShapeProxy))]
    public class VRMBlnedShapeProxyEditor : Editor
    {
        VRMBlendShapeProxy m_target;
        SkinnedMeshRenderer[] m_renderers;

        void OnEnable()
        {
            m_target = (VRMBlendShapeProxy)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_target.Sliders != null)
            {
                foreach (var slider in m_target.Sliders)
                {
                    slider.Slider();
                }
            }
        }
    }
}
