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
    [CustomEditor(typeof(Warp))]
    class WarpEditor : Editor
    {
        private Warp m_target;
        private Vrm10Instance m_vrm;
        private Transform[] m_items;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            m_target = (Warp)target;
            m_vrm = m_target.GetComponentInParent<Vrm10Instance>();
            m_items = m_target.GetComponentsInChildren<Transform>().Skip(1).ToArray();
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
            root.Add(new PropertyField { bindingPath = nameof(Warp.BaseSettings) });
            root.Add(new PropertyField { bindingPath = nameof(Warp.Center) });

            root.Add(new PropertyField { bindingPath = nameof(Warp.Particles) });
            {

                var listview = new MultiColumnListView();
                listview.columns.Add(new Column
                {
                    title = "Transform",
                    width = 80,
                    makeCell = () => new ObjectField(),
                    bindCell = (v, i) =>
                    {
                        v.SetEnabled(false);
                        (v as ObjectField).value = m_items[i];
                    },
                });
                listview.columns.Add(new Column
                {
                    title = "Mode",
                    width = 60,
                    makeCell = () => new EnumField(default(Warp.ParticleMode)),
                });
                listview.columns.Add(new Column
                {
                    title = "stiffnessForce",
                    width = 20,
                    makeCell = () => new FloatField(),
                });
                listview.columns.Add(new Column
                {
                    title = "gravityPower",
                    width = 20,
                    makeCell = () => new FloatField(),
                });
                listview.columns.Add(new Column
                {
                    title = "gravityDir",
                    width = 20,
                    makeCell = () => new Vector3Field(),
                });
                listview.columns.Add(new Column
                {
                    title = "dragForce",
                    width = 20,
                    makeCell = () => new FloatField(),
                });
                listview.columns.Add(new Column
                {
                    title = "radius",
                    width = 20,
                    makeCell = () => new FloatField(),
                });

                listview.itemsSource = m_items;
                root.Add(listview);
            }

            root.Add(new PropertyField { bindingPath = nameof(Warp.ColliderGroups) });

            return root;
        }
    }
}