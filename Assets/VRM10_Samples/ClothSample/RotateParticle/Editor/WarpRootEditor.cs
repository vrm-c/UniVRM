using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniVRM10;

namespace RotateParticle.Components
{
    [CustomEditor(typeof(WarpRoot))]
    class WarpRootEditor : Editor
    {
        private WarpRoot m_target;
        private Vrm10Instance m_vrm;
        private MultiColumnTreeView m_treeview;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            m_target = (WarpRoot)target;
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

            root.Bind(serializedObject);

            {
                var s = new PropertyField { bindingPath = "m_Script" };
                s.SetEnabled(false);
                root.Add(s);
            }
            root.Add(new PropertyField { bindingPath = nameof(WarpRoot.BaseSettings) });
            root.Add(new PropertyField { bindingPath = nameof(WarpRoot.Center) });

            root.Add(new PropertyField { bindingPath = nameof(WarpRoot.Particles) });
            {
                m_treeview = new MultiColumnTreeView();
                m_treeview.columns.Add(new Column
                {
                    title = "Transform",
                    width = 160,
                    makeCell = () => new ObjectField(),
                    bindCell = (v, i) =>
                    {
                        v.SetEnabled(false);
                        (v as ObjectField).value = m_target.GetParticle(i).Transform;
                    },
                });
                m_treeview.columns.Add(new Column
                {
                    title = "Mode",
                    width = 60,
                    makeCell = () => new EnumField(default(WarpRoot.ParticleMode)),
                });
                m_treeview.columns.Add(new Column
                {
                    title = "stiffnessForce",
                    width = 20,
                    makeCell = () => new FloatField(),
                });
                m_treeview.columns.Add(new Column
                {
                    title = "gravityPower",
                    width = 20,
                    makeCell = () => new FloatField(),
                });
                m_treeview.columns.Add(new Column
                {
                    title = "gravityDir",
                    width = 20,
                    makeCell = () => new Vector3Field(),
                });
                m_treeview.columns.Add(new Column
                {
                    title = "dragForce",
                    width = 20,
                    makeCell = () => new FloatField(),
                });
                m_treeview.columns.Add(new Column
                {
                    title = "radius",
                    width = 20,
                    makeCell = () => new FloatField(),
                });

                m_treeview.autoExpand = true;
                m_treeview.SetRootItems(m_target.m_rootitems);
                root.Add(m_treeview);
            }

            root.Add(new PropertyField { bindingPath = nameof(WarpRoot.ColliderGroups) });

            return root;
        }

        public void OnSceneGUI()
        {
            if (m_treeview == null)
            {
                return;
            }

            var item = m_treeview.selectedItem;
            if (item == null)
            {
                return;
            }

            if (item is WarpRoot.Particle p)
            {
                var t = p.Transform;
                Handles.color = Color.green;
                Handles.SphereHandleCap(t.GetInstanceID(), t.position, t.rotation, p.Settings.radius * 2, EventType.Repaint);
            }
        }
    }
}