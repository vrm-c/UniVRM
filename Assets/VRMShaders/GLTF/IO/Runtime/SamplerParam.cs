using UnityEngine;


namespace VRMShaders
{
    public struct SamplerParam
    {
        public (SamplerWrapType, TextureWrapMode)[] WrapModes;
        public FilterMode FilterMode;
    }
}
