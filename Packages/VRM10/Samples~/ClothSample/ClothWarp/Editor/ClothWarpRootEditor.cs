using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniGLTF;
using System.Linq;


namespace UniVRM10.ClothWarp.Components
{
    [CustomEditor(typeof(ClothWarpRoot))]
    class ClothWarpRootEditor : Editor
    {
        private ClothWarpRoot m_target;
        private Vrm10Instance m_vrm;
        private MultiColumnTreeView m_treeview;
        VisualElement m_body;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            m_target = (ClothWarpRoot)target;
            m_vrm = m_target.GetComponentInParent<Vrm10Instance>();
        }

        void BindColumn<T>(MultiColumnTreeView tree, string title, 
            int width, Func<T> makeVisualELment, 
            Func<int, bool> enableFunc, string subpath) where T : BindableElement
        {
            m_treeview.columns.Add(new Column
            {
                title = title,
                width = width,
                makeCell = makeVisualELment,
                bindCell = (v, index) =>
                {
                    if (v is T prop)
                    {
                        var i = tree.GetIdForIndex(index);
                        var sb = new System.Text.StringBuilder();
                        sb.Append("m_particles.Array.data[");
                        sb.Append(i);
                        sb.Append("].");
                        sb.Append(subpath);
                        var s = sb.ToString();
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

            root.Add(new IMGUIContainer(() =>
            {
                foreach (var v in m_target.Validations)
                {
                    v.DrawGUI();
                }
            }));

            m_body = new VisualElement();
            root.Add(m_body);
            m_body.style.display = m_target.Validations.All(x => x.ErrorLevel < ErrorLevels.Warning)
                ? DisplayStyle.Flex
                : DisplayStyle.None
                ;

            m_body.Add(new PropertyField { bindingPath = nameof(ClothWarpRoot.BaseSettings) });
            m_body.Add(new PropertyField { bindingPath = nameof(ClothWarpRoot.Center) });

            // root.Add(new PropertyField { bindingPath = "m_particles" });
            {
                Func<int, bool> isCustom = (i) =>
                {
                    return m_target.Particles[i].Mode == ClothWarpRoot.ParticleMode.Custom;
                };

                m_treeview = new MultiColumnTreeView();
                BindColumn(m_treeview, "Transform", 120, () => new ObjectField(), (_) => false, "Transform");
                BindColumn(m_treeview, "Mode", 40, () => new EnumField(), (_) => true, "Mode");
                BindColumn(m_treeview, "Stiffness", 40, () => new FloatField(), isCustom, "Settings.Stiffness");
                BindColumn(m_treeview, "Gravity", 120, () => new Vector3Field(), isCustom, "Settings.Gravity");
                BindColumn(m_treeview, "Deceleration", 40, () => new FloatField(), isCustom, "Settings.Deceleration");
                BindColumn(m_treeview, "Radius", 40, () => new FloatField(), isCustom, "Settings.Radius");

                m_treeview.autoExpand = true;
                m_treeview.SetRootItems(m_target.m_rootitems);
                m_body.Add(m_treeview);
            }

            m_body.Add(new PropertyField { bindingPath = nameof(ClothWarpRoot.ColliderGroups) });

            return root;
        }

        private void OnValueChanged(SerializedObject so)
        {
            if (m_vrm != null)
            {
                if (Application.isPlaying)
                {
                    m_vrm.Runtime.SpringBone.SetJointLevel(m_target.transform, m_target.BaseSettings.ToBlittableJointMutable());
                    foreach (var p in m_target.Particles)
                    {
                        m_vrm.Runtime.SpringBone.SetJointLevel(p.Transform, p.Settings.ToBlittableJointMutable());
                    }
                }
            }

            m_treeview.RefreshItems();

            m_body.style.display = m_target.Validations.All(x => x.ErrorLevel < ErrorLevels.Warning)
                ? DisplayStyle.Flex
                : DisplayStyle.None
                ;
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
                Handles.SphereHandleCap(t.GetInstanceID(), t.position, t.rotation, p.Settings.Radius * 2, EventType.Repaint);
            }
        }
    }
}