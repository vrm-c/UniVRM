using System;

namespace VRM10.MToon10
{
    public static class MToon10Meta
    {
        public static readonly string UnityShaderName = "VRM10/MToon10";
        public static readonly string UnityUrpShaderName = "VRM10/Universal Render Pipeline/MToon10";

        [Obsolete("Use UnityUrpShaderName instead")]
        public static readonly string URPUnityShaderName = UnityUrpShaderName;
    }
}