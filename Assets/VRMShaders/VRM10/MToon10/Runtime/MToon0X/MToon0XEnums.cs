namespace VRMShaders.VRM10.MToon10.Runtime.MToon0X
{
    public enum MToon0XDebugMode
    {
        None = 0,
        Normal = 1,
        LitShadeRate = 2,
    }

    public enum MToon0XOutlineColorMode
    {
        FixedColor = 0,
        MixedLighting = 1,
    }

    public enum MToon0XOutlineWidthMode
    {
        None = 0,
        WorldCoordinates = 1,
        ScreenCoordinates = 2,
    }

    public enum MToon0XRenderMode
    {
        Opaque = 0,
        Cutout = 1,
        Transparent = 2,
        TransparentWithZWrite = 3,
    }

    public enum MToon0XCullMode
    {
        Off = 0,
        Front = 1,
        Back = 2,
    }

    public struct MToon0XRenderQueueRequirement
    {
        public int DefaultValue;
        public int MinValue;
        public int MaxValue;
    }
}