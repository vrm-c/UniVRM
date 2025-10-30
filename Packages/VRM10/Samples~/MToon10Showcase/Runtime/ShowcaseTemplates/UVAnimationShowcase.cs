using System;
using UnityEngine;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(UVAnimationShowcase))]
#endif
    public sealed class UVAnimationShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public UVAnimationEntry[] entries;

        [Serializable]
        public sealed class UVAnimationEntry
        {
            public Texture uvAnimationMaskTexture;
            public float uvAnimationScrollXSpeedFactor;
            public float uvAnimationScrollYSpeedFactor;
            public float uvAnimationRotationSpeedFactor;
        }
    }
}