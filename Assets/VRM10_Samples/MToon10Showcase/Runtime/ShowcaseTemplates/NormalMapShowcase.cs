using System;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(NormalMapShowcase))]
#endif
    public sealed class NormalMapShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public NormalMapEntry[] entries;

        [Serializable]
        public sealed class NormalMapEntry
        {
            public Texture2D normalTexture;
            public float normalTextureScale;
        }
    }
}