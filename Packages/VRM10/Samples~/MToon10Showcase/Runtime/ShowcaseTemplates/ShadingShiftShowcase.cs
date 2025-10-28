using System;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(ShadingShiftShowcase))]
#endif
    public sealed class ShadingShiftShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public ShadingShiftEntry[] entries;

        [Serializable]
        public sealed class ShadingShiftEntry
        {
            public Texture shadingShiftTexture;
            public float shadingShiftTextureScale;
        }
    }
}