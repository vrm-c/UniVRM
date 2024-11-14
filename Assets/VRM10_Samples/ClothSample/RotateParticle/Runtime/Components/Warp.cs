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
        ParticleSettings BaseSettings;

        /// <summary>
        /// null のときは world root ではなく model root で処理
        /// </summary>
        [SerializeField]
        Transform Center;

        /// <summary>
        /// 枝分かれ不可
        /// </summary>
        [SerializeField]
        List<Particle> Particles = new List<Particle>();

        void Reset()
        {
            Debug.Log("Warp.Reset");
        }

        void OnValidate()
        {
            // 枝分かれを削除
            Debug.Log("Warp.OnValidate");
        }
    }
}
