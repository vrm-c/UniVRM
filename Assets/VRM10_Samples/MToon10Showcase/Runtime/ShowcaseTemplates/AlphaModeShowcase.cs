using System;
using UnityEngine;
using VRM10.MToon10;

namespace VRM10.Samples.MToon10Showcase
{
#if VRM_DEVELOP
    [CreateAssetMenu(menuName = "VRM10/Samples/MToon10Showcase/" + nameof(AlphaModeShowcase))]
#endif
    public sealed class AlphaModeShowcase : ScriptableObject
    {
        public Material baseMaterial;
        public AlphaModeEntry[] entries;

        [Serializable]
        public sealed class AlphaModeEntry
        {
            public MToon10AlphaMode alphaMode;
            public MToon10TransparentWithZWriteMode transparentWithZWriteMode;
            public float alphaCutoff;
            public MToon10DoubleSidedMode doubleSidedMode;
        }
    }
}