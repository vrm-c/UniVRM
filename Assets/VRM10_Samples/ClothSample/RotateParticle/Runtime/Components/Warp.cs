using System;
using System.Collections.Generic;
using UnityEngine;
using UniVRM10;


namespace RotateParticle.Components
{
    [AddComponentMenu("RotateParticle/Warp")]
    [DisallowMultipleComponent]
    public class Warp : MonoBehaviour
    {
        [Serializable]
        public struct ParticleSettings
        {
            [SerializeField]
            public float Stiffness;

            [SerializeField]
            public float GravityPower;

            [SerializeField]
            public Vector3 GravityDir;

            [SerializeField, Range(0, 1)]
            public float DragForce;

            [SerializeField]
            public float HitRadius;

            public static ParticleSettings Default => new ParticleSettings
            {
                Stiffness = 1.0f,
                GravityPower = 0,
                GravityDir = new Vector3(0, -1.0f, 0),
                DragForce = 0.4f,
                HitRadius = 0.02f,
            };
        }

        /// <summary>
        /// VRM10SpringBoneJoint に相当する
        /// </summary>
        [Serializable]
        public class Particle
        {
            public bool useInheritSettings = true;
            public ParticleSettings OverrideSettings = ParticleSettings.Default;
            public Transform Transform;

            public ParticleSettings GetSettings(ParticleSettings baseSettings)
            {
                return useInheritSettings ? baseSettings : OverrideSettings;
            }
        }

        [SerializeField]
        public ParticleSettings BaseSettings = ParticleSettings.Default;

        /// <summary>
        /// null のときは world root ではなく model root で処理
        /// </summary>
        [SerializeField]
        public Transform Center;

        /// <summary>
        /// 枝分かれ不可
        /// </summary>
        [SerializeField]
        public List<Particle> Particles = new();

        [SerializeField]
        public List<VRM10SpringBoneColliderGroup> ColliderGroups = new();

        void Reset()
        {
            // Debug.Log("Warp.Reset");            
        }

        public void AddParticleRecursive()
        {
            if (transform.childCount > 0)
            {
                AddParticleRecursive(transform.GetChild(0));
            }
        }

        public void AddParticleRecursive(Transform t)
        {
            Particles.Add(new Particle
            {
                Transform = t,
            });
            if (t.childCount > 0)
            {
                AddParticleRecursive(t.GetChild(0));
            }
        }

        void OnValidate()
        {
            // TODO: 枝分かれを削除
            // Debug.Log("Warp.OnValidate");
        }

        internal List<Warp> ToList()
        {
            throw new NotImplementedException();
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(transform.position, BaseSettings.HitRadius);

            Transform prev = transform;
            foreach (var p in Particles)
            {
                if (p != null && p.Transform != null)
                {
                    Gizmos.DrawWireSphere(p.Transform.position, p.GetSettings(BaseSettings).HitRadius);

                    if (prev != null)
                    {
                        Gizmos.DrawLine(prev.position, p.Transform.position);
                    }
                }
                prev = p.Transform;
            }
        }
    }
}