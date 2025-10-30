using UnityEngine;

namespace UniVRM10
{
    public class Vrm10FastSpringboneRuntimeStandaloneProvider : MonoBehaviour, IVrm10SpringBoneRuntimeProvider
    {
        public IVrm10SpringBoneRuntime CreateSpringBoneRuntime()
        {
            return new Vrm10FastSpringboneRuntimeStandalone();
        }
    }
}