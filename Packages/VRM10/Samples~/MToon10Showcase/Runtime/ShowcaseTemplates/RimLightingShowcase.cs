using System;
using UnityEngine;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(RimLightingShowcase))]
#endif
    public sealed class RimLightingShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public RimLightingMixFactorEntry[] entries;

        [Serializable]
        public sealed class RimLightingMixFactorEntry
        {
            public float rimLightingMixFactor;
            public Texture rimMultiplyTexture;
        }
    }
}