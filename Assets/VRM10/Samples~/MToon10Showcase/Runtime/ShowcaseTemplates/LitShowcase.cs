using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(LitShowcase))]
#endif
    public sealed class LitShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public LitEntry[] entries;

        [Serializable]
        public sealed class LitEntry
        {
            public Color litColor;
            public Texture2D litTexture;
        }
    }
}