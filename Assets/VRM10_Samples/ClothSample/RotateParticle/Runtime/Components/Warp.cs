using System;
using System.Collections.Generic;
using UnityEngine;


namespace RotateParticle.Components
{
    [AddComponentMenu("RotateParticle/Warp")]
    [DisallowMultipleComponent]
    public class Warp : MonoBehaviour
    {
        [Serializable]
        public class ParticleSettings : IEquatable<ParticleSettings>
        {
            [SerializeField]
            public float StiffnessForce = 1.0f;

            [SerializeField]
            public float GravityPower = 0;

            [SerializeField]
            public Vector3 GravityDir = new Vector3(0, -1.0f, 0);

            [SerializeField, Range(0, 1)]
            public float DragForce = 0.4f;

            [SerializeField]
            public float HitRadius = 0.02f;

            public override bool Equals(object obj) // Object.Equals(Object)のオーバーライド
            {
                return Equals(obj as ParticleSettings);
            }

            public bool Equals(ParticleSettings other)
            {
                return
                StiffnessForce == other.StiffnessForce
                && GravityPower == other.GravityPower
                && GravityDir == other.GravityDir
                && DragForce == other.DragForce
                && HitRadius == other.HitRadius
                ;
            }

            public override int GetHashCode()
            {
                return GravityDir.GetHashCode();
            }
        }

        /// <summary>
        /// VRM10SpringBoneJoint に相当する
        /// </summary>
        [Serializable]
        public class Particle
        {
            public bool useInheritSettings = true;
            public ParticleSettings OverrideSettings = new();
            public Transform Transform;

            public ParticleSettings GetSettings(ParticleSettings baseSettings)
            {
                return useInheritSettings ? baseSettings : OverrideSettings;
            }
        }

        [SerializeField]
        public ParticleSettings BaseSettings = new();

        /// <summary>
        /// null のときは world root ではなく model root で処理
        /// </summary>
        [SerializeField]
        public Transform Center;

        /// <summary>
        /// 枝分かれ不可
        /// </summary>
        [SerializeField]
        public List<Particle> Particles = new List<Particle>();

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