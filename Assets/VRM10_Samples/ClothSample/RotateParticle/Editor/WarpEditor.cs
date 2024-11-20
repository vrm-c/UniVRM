using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
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

        // public override void OnInspectorGUI()
        // {
        //     var n = EditorUtility.GetDirtyCount(m_target.GetInstanceID());
        //     base.OnInspectorGUI();
        //     if (n != EditorUtility.GetDirtyCount(m_target.GetInstanceID()))
        //     {
        //         if (m_vrm != null)
        //         {
        //             if (Application.isPlaying)
        //             {
        //                 m_vrm.Runtime.SpringBone.SetJointLevel(m_target.transform, m_target.BaseSettings);
        //                 foreach (var p in m_target.Particles)
        //                 {
        //                     m_vrm.Runtime.SpringBone.SetJointLevel(p.Transform, p.GetSettings(m_target.BaseSettings));
        //                 }
        //             }
        //         }
        //     }
        // }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // var container = new IMGUIContainer(OnInspectorGUI);
            // root.Add(container);

            root.Bind(serializedObject);

            var s = new PropertyField { bindingPath = "m_Script" };
            s.SetEnabled(false);
            root.Add(s);
            root.Add(new PropertyField { bindingPath = nameof(Warp.BaseSettings) });
            root.Add(new PropertyField { bindingPath = nameof(Warp.Center) });
            root.Add(new PropertyField { bindingPath = nameof(Warp.Particles) });
            root.Add(new PropertyField { bindingPath = nameof(Warp.ColliderGroups) });

            return root;
        }
    }
}