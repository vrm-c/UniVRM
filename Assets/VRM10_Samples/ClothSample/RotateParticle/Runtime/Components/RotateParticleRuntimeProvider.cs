using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniVRM10;

namespace RotateParticle.Components
{
    [AddComponentMenu("RotateParticle/RotateParticleRuntimeProvider")]
    [DisallowMultipleComponent]
    public class RotateParticleRuntimeProvider : MonoBehaviour, IVrm10SpringBoneRuntimeProvider
    {
        [SerializeField]
        public List<Warp> Warps = new();

        [SerializeField]
        public List<RectCloth> Cloths = new();

        IVrm10SpringBoneRuntime m_runtime;
        public IVrm10SpringBoneRuntime CreateSpringBoneRuntime()
        {
            m_runtime = new RotateParticleSpringboneRuntime();
            return m_runtime;
        }

        public void Reset()
        {
            Warps = GetComponentsInChildren<Warp>().ToList();
            Cloths = GetComponentsInChildren<RectCloth>().ToList();
        }

        void OnDrawGizmos()
        {
            if (m_runtime == null)
            {
                return;
            }
            m_runtime.DrawGizmos();
        }
    }
}