using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UnityEngine.UIElements;
using UniVRM10;


namespace ClothWarpLib.Components
{
    [AddComponentMenu("ClothWarp/ClothWarp")]
    [DisallowMultipleComponent]
    /// <summary>
    /// Warp の root にアタッチする。
    /// 子孫の Transform がすべて登録される。 
    /// </summary>
    public class ClothWarp : MonoBehaviour
    {
        public static BlittableJointMutable DefaultSetting()
        {
            return new BlittableJointMutable
            {
                stiffnessForce = 1.0f,
                gravityPower = 0,
                gravityDir = new Vector3(0, -1.0f, 0),
                dragForce = 0.4f,
                radius = 0.02f,
            };
        }

        public enum ParticleMode
        {
            /// <summary>
            /// Use BaseSettings 
            /// </summary>
            Base,
            /// <summary>
            /// Use specific settings
            /// </summary>
            Custom,
            /// <summary>
            /// no animation
            /// </summary>
            Disabled,
        }

        /// <summary>
        /// VRM10SpringBoneJoint に相当する
        /// </summary>
        [Serializable]
        public struct Particle
        {
            public Transform Transform;
            public ParticleMode Mode;
            public BlittableJointMutable Settings;

            public Particle(Transform t, ParticleMode mode, BlittableJointMutable settings)
            {
                Transform = t;
                Mode = mode;
                Settings = settings;
            }

            public Particle(Transform t, BlittableJointMutable settings)
            : this(t, ParticleMode.Custom, settings)
            {
            }

            public Particle(Transform t)
            : this(t, ParticleMode.Base, DefaultSetting())
            {
            }
        }

        [SerializeField]
        public BlittableJointMutable BaseSettings = DefaultSetting();

        /// <summary>
        /// null のときは world root ではなく model root で処理
        /// </summary>
        [SerializeField]
        public Transform Center;

        /// <summary>
        /// 枝分かれ不可
        /// </summary>
        [SerializeField]
        private List<Particle> m_particles = new();
        public IReadOnlyList<Particle> Particles => m_particles;

        [SerializeField]
        public List<VRM10SpringBoneColliderGroup> ColliderGroups = new();

        // uitool kit 向け
        public List<TreeViewItemData<Particle>> m_rootitems;

        // 逆引き
        Dictionary<Transform, int> m_map = new();

        void OnValidate()
        {
            m_particles = GetComponentsInChildren<Transform>().Skip(1).Select(x => new Particle(x)).ToList();
            m_map.Clear();
            for (int i = 0; i < m_particles.Count; ++i)
            {
                var p = m_particles[i];
                if (p.Mode == ParticleMode.Base)
                {
                    p.Settings = BaseSettings;
                    m_particles[i] = p;
                }
                m_map.Add(p.Transform, i);
            }

            m_rootitems = MakeTree(-1);
        }

        List<TreeViewItemData<Particle>> MakeTree(int id)
        {
            List<TreeViewItemData<Particle>> items = new();
            foreach (Transform child_transform in id == -1 ? transform : m_particles[id].Transform)
            {
                var child_id = m_map[child_transform];
                var item = (child_transform.childCount > 0)
                    ? new TreeViewItemData<Particle>(child_id, m_particles[child_id], MakeTree(child_id))
                    : new TreeViewItemData<Particle>(child_id, m_particles[child_id])
                    ;
                items.Add(item);
            }
            return items;
        }

        public Particle GetParticleFromId(int id)
        {
            return m_particles[id];
        }

        public Particle GetParticleFromTransform(Transform t)
        {
            foreach (var p in m_particles)
            {
                if (p.Transform == t)
                {
                    return p;
                }
            }
            return default;
        }

        public void UseBaseSettings(Transform t)
        {
            if (t == null) return;
            for (int i = 0; i < m_particles.Count; ++i)
            {
                var p = m_particles[i];
                if (p.Transform == t)
                {
                    p.Mode = ParticleMode.Base;
                    p.Settings = BaseSettings;
                    m_particles[i] = p;
                    break;
                }
            }
        }

        public void SetSettings(Transform t, BlittableJointMutable settings)
        {
            if (t == null) return;
            for (int i = 0; i < m_particles.Count; ++i)
            {
                var p = m_particles[i];
                if (p.Transform == t)
                {
                    p.Mode = ParticleMode.Custom;
                    p.Settings = settings;
                    m_particles[i] = p;
                    break;
                }
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(transform.position, BaseSettings.radius);

            foreach (var p in Particles)
            {
                if (p.Transform == null || p.Mode == ParticleMode.Disabled)
                {
                    continue;
                }
                Gizmos.DrawWireSphere(p.Transform.position, p.Settings.radius);

                if (TryGetClosestParent(p.Transform, out var parent))
                {
                    Gizmos.DrawLine(p.Transform.position, parent.position);
                }
            }
        }

        bool TryGetClosestParent(Transform t, out Transform parent)
        {
            for (var current = t.parent; current != null; current = current.parent)
            {
                if (current == transform)
                {
                    parent = transform;
                    return true;
                }

                var index = m_map[current];
                var p = m_particles[index];
                if (p.Mode != ParticleMode.Disabled)
                {
                    parent = p.Transform;
                    return true;
                }
            }
            parent = default;
            return false;
        }
    }
}