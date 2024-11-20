using UnityEditor;
using UnityEngine;
using UniVRM10;

namespace RotateParticle.Components
{
    [CustomEditor(typeof(Warp))]
    class WarpEditor : Editor
    {
        private Warp m_target;
        private Vrm10Instance m_vrm;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_target = (Warp)target;
            m_vrm = m_target.GetComponentInParent<Vrm10Instance>();
        }

        public override void OnInspectorGUI()
        {
            var n = EditorUtility.GetDirtyCount(m_target.GetInstanceID());
            base.OnInspectorGUI();
            if (n != EditorUtility.GetDirtyCount(m_target.GetInstanceID()))
            {
                if (m_vrm != null)
                {
                    if (Application.isPlaying)
                    {
                        m_vrm.Runtime.SpringBone.SetJointLevel(m_target.transform, m_target.BaseSettings);
                        foreach (var p in m_target.Particles)
                        {
                            m_vrm.Runtime.SpringBone.SetJointLevel(p.Transform, p.GetSettings(m_target.BaseSettings));
                        }
                    }
                }
            }
        }
    }
}