using System;
using UnityEngine;
using UnityEngine.Serialization;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(OutlineShowcase))]
#endif
    public sealed class OutlineShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public OutlineEntry[] entries;

        [Serializable]
        public sealed class OutlineEntry
        {
            public MToon10OutlineMode outlineWidthMode;
            public float outlineWidthFactor;
            public Color outlineColorFactor;
            public Texture outlineWidthMultiplyTexture;
            public float outlineLightingMixFactor;
        }
    }
}