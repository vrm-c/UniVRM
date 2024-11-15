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

        public IVrm10SpringBoneRuntime CreateSpringBoneRuntime()
        {
            return new RotateParticleSpringboneRuntime();
        }

        public void Reset()
        {
            Warps = GetComponentsInChildren<Warp>().ToList();
            Cloths = GetComponentsInChildren<RectCloth>().ToList();
        }
    }
}