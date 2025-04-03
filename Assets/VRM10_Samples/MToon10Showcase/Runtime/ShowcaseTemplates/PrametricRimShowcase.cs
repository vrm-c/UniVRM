using System;
using UnityEngine;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(ParametricRimShowcase))]
#endif
    public sealed class ParametricRimShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public ParametricRimEntry[] entries;

        [Serializable]
        public sealed class ParametricRimEntry
        {
            public Color parametricRimColor;
            public float parametricRimFresnelPowerFactor;
            public float parametricRimLiftFactor;
        }
    }
}