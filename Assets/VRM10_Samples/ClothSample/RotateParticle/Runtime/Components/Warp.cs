using System;
using System.Collections.Generic;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;
using UniVRM10;


namespace RotateParticle.Components
{
    [AddComponentMenu("RotateParticle/Warp")]
    [DisallowMultipleComponent]
    public class Warp : MonoBehaviour
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

        /// <summary>
        /// VRM10SpringBoneJoint に相当する
        /// </summary>
        [Serializable]
        public class Particle
        {
            public bool useInheritSettings = true;
            public BlittableJointMutable OverrideSettings = DefaultSetting();
            public Transform Transform;

            public BlittableJointMutable GetSettings(BlittableJointMutable baseSettings)
            {
                return useInheritSettings ? baseSettings : OverrideSettings;
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
            Gizmos.DrawSphere(transform.position, BaseSettings.radius);

            Transform prev = transform;
            foreach (var p in Particles)
            {
                if (p != null && p.Transform != null)
                {
                    Gizmos.DrawWireSphere(p.Transform.position, p.GetSettings(BaseSettings).radius);

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