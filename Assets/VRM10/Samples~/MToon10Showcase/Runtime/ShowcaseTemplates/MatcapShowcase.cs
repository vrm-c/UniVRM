using System;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(MatcapShowcase))]
#endif
    public sealed class MatcapShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public MatcapEntry[] entries;

        [Serializable]
        public sealed class MatcapEntry
        {
            public Texture matcapTexture;
            public Color matcapColorFactor;
        }
    }
}