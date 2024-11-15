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
        public class ParticleSettings
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
        }

        /// <summary>
        /// VRM10SpringBoneJoint に相当する
        /// </summary>
        [Serializable]
        public class Particle
        {
            public bool useInheritSettings;
            public ParticleSettings OverrideSettings;
        }

        [SerializeField]
        public ParticleSettings BaseSettings;

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

        void OnValidate()
        {
            // TODO: 枝分かれを削除
            // Debug.Log("Warp.OnValidate");
        }

        internal List<Warp> ToList()
        {
            throw new NotImplementedException();
        }
    }
}
