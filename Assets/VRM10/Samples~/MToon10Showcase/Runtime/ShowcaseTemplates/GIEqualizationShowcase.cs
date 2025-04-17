using System;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(GIEqualizationShowcase))]
#endif
    public sealed class GIEqualizationShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public GIEqualizationEntry[] entries;

        [Serializable]
        public sealed class GIEqualizationEntry
        {
            public float giEqualizationFactor;
        }
    }
}