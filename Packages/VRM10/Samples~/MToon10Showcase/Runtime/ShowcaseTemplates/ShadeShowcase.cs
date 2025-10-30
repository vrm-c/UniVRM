using System;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(ShadeShowcase))]
#endif
    public sealed class ShadeShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public ShadeEntry[] entries;

        [Serializable]
        public sealed class ShadeEntry
        {
            public Color shadeColor;
            public Texture2D shadeTexture;
        }
    }
}