using UnityEngine;
using UniVRM10;

namespace RotateParticle
{
    public class RotateParticleSpringboneRuntimeProvider : MonoBehaviour, IVrm10SpringBoneRuntimeProvider
    {
        public IVrm10SpringBoneRuntime CreateSpringBoneRuntime()
        {
            return new RotateParticleSpringboneRuntime();
        }
    }
}