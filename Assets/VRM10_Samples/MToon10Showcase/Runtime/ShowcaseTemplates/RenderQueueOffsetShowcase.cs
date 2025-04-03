using System;
using UnityEngine;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(RenderQueueOffsetShowcase))]
#endif
    public sealed class RenderQueueOffsetShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public RenderQueueOffsetEntry[] entries;

        [Serializable]
        public sealed class RenderQueueOffsetEntry
        {
            public Color litColor;
            public int renderQueueOffset;
        }
        
    }
}