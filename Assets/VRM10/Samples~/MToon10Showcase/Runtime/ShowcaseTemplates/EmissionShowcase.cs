using System;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(EmissionShowcase))]
#endif
    public sealed class EmissionShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public EmissionEntry[] entries;

        [Serializable]
        public sealed class EmissionEntry
        {
            public Texture emissiveTexture;
            [ColorUsage(true, true)]
            public Color emissiveFactorLinear;
        }
    }
}