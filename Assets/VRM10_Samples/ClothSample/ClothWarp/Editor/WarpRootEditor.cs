using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniVRM10;

namespace ClothWarpLib.Components
{
    [CustomEditor(typeof(ClothWarpRoot))]
    class WarpRootEditor : Editor
    {
        private ClothWarpRoot m_target;
        private Vrm10Instance m_vrm;
        private MultiColumnTreeView m_treeview;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            m_target = (ClothWarpRoot)target;
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

        void BindColumn<T>(string title, int width, Func<T> makeVisualELmeent, Func<int, bool> enableFunc, string subpath) where T : BindableElement
        {
            m_treeview.columns.Add(new Column
            {
                title = title,
                width = width,
                makeCell = makeVisualELmeent,
                bindCell = (v, i) =>
                {
                    if (v is T prop)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.Append("m_particles.Array.data[");
                        sb.Append(i);
                        sb.Append("].");
                        sb.Append(subpath);
                        var s = sb.ToString();
                        // Debug.Log(s);
                        prop.BindProperty(serializedObject.FindProperty(s));
                        prop.SetEnabled(enableFunc(i));
                    }
                },
            });
        }

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
            root.Add(new PropertyField { bindingPath = nameof(ClothWarpRoot.BaseSettings) });
            root.Add(new PropertyField { bindingPath = nameof(ClothWarpRoot.Center) });

            // root.Add(new PropertyField { bindingPath = "m_particles" });
            {
                Func<int, bool> isCustom = (i) =>
                {
                    return m_target.Particles[i].Mode == ClothWarpRoot.ParticleMode.Custom;
                };

                m_treeview = new MultiColumnTreeView();
                BindColumn("Transform", 120, () => new ObjectField(), (_) => false, "Transform");
                BindColumn("Mode", 40, () => new EnumField(), (_) => true, "Mode");
                BindColumn("stiffnessForce", 40, () => new FloatField(), isCustom, "Settings.stiffnessForce");
                BindColumn("gravityPower", 40, () => new FloatField(), isCustom, "Settings.gravityPower");
                BindColumn("gravityDir", 120, () => new Vector3Field(), isCustom, "Settings.gravityDir");
                BindColumn("dragForce", 40, () => new FloatField(), isCustom, "Settings.dragForce");
                BindColumn("radius", 40, () => new FloatField(), isCustom, "Settings.radius");

                m_treeview.autoExpand = true;
                m_treeview.SetRootItems(m_target.m_rootitems);
                root.Add(m_treeview);
            }

            root.Add(new PropertyField { bindingPath = nameof(ClothWarpRoot.ColliderGroups) });

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

            if (item is ClothWarpRoot.Particle p)
            {
                p = m_target.GetParticleFromTransform(p.Transform);
                var t = p.Transform;
                Handles.color = Color.green;
                Handles.SphereHandleCap(t.GetInstanceID(), t.position, t.rotation, p.Settings.radius * 2, EventType.Repaint);
            }
        }
    }
}