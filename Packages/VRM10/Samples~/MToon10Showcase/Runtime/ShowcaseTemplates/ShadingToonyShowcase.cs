using System;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(ShadingToonyShowcase))]
#endif
    public sealed class ShadingToonyShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public ShadingToonyEntry[] entries;

        [Serializable]
        public sealed class ShadingToonyEntry
        {
            public float shadingToonyFactor;
            public float shadingShiftFactor;
        }
    }
}