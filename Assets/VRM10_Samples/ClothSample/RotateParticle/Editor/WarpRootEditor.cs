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

            root.TrackSerializedObjectValue(serializedObject, OnValueChanged);

            root.Bind(serializedObject);

            {
                var s = new PropertyField { bindingPath = "m_Script" };
                s.SetEnabled(false);
                root.Add(s);
            }
            root.Add(new PropertyField { bindingPath = nameof(WarpRoot.BaseSettings) });
            root.Add(new PropertyField { bindingPath = nameof(WarpRoot.Center) });

            root.Add(new PropertyField { bindingPath = "m_particles" });

            {
                m_treeview = new MultiColumnTreeView();
                m_treeview.columns.Add(new Column
                {
                    title = "Transform",
                    width = 160,
                    makeCell = () => new ObjectField
                    {
                    },
                    bindCell = (v, i) =>
                    {
                        v.SetEnabled(false);
                        // var prop = serializedObject.FindProperty($"m_particles.Array.data[{i}].Transform");
                        // Debug.Log($"{i} => {prop}");
                        if (v is ObjectField prop)
                        {
                            // prop.bindingPath = $"m_particles.Array.data[{i}].Transform";
                            prop.BindProperty(serializedObject.FindProperty($"m_particles.Array.data[{i}].Transform"));
                        }
                    },
                });
                m_treeview.columns.Add(new Column
                {
                    title = "Mode",
                    width = 60,
                    // makeCell = () => new EnumField(default(WarpRoot.ParticleMode)),
                    makeCell = () => new EnumField
                    {
                        // bindingPath = $"m_particles.Array.data[0].Mode",
                    },
                    bindCell = (v, i) =>
                    {
                        if (v is EnumField prop)
                        {
                            prop.bindingPath = $"m_particles.Array.data[{i}].Mode";
                            prop.Bind(serializedObject);
                        }
                    },
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
                    bindCell = (v, i) =>
                    {
                        if (v is FloatField prop)
                        {
                            prop.BindProperty(serializedObject.FindProperty($"m_particles.Array.data[{i}].Settings.radius"));
                            prop.SetEnabled(m_target.Particles[i].Mode == WarpRoot.ParticleMode.Custom);
                        }
                    },
                });

                m_treeview.autoExpand = true;
                m_treeview.SetRootItems(m_target.m_rootitems);
                root.Add(m_treeview);
            }

            root.Add(new PropertyField { bindingPath = nameof(WarpRoot.ColliderGroups) });

            return root;
        }

        private void OnValueChanged(SerializedObject so)
        {
            Debug.Log("Name changed: " + so.targetObject.name);
            // var nameProperty = so.FindProperty("m_Name");

            // if (nameProperty.stringValue.Contains(" "))
            //     _textField.style.backgroundColor = Color.red;
            // else
            //     _textField.style.backgroundColor = StyleKeyword.Null;
            m_treeview.RefreshItems();
            // m_treeview.SetRootItems(m_target.m_rootitems);
            Repaint();
        }

        public void OnSceneGUI()
        {
            HandleUtility.Repaint();
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
                p = m_target.GetParticleFromTransform(p.Transform);
                var t = p.Transform;
                Handles.color = Color.green;
                Handles.SphereHandleCap(t.GetInstanceID(), t.position, t.rotation, p.Settings.radius * 2, EventType.Repaint);
            }
        }
    }
}