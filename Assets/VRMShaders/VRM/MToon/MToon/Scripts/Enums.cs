namespace MToon
{
    public enum DebugMode
    {
        None = 0,
        Normal = 1,
        LitShadeRate = 2,
    }

    public enum OutlineColorMode
    {
        FixedColor = 0,
        MixedLighting = 1,
    }

    public enum OutlineWidthMode
    {
        None = 0,
        WorldCoordinates = 1,
        ScreenCoordinates = 2,
    }

    public enum RenderMode
    {
        Opaque = 0,
        Cutout = 1,
        Transparent = 2,
        TransparentWithZWrite = 3,
    }

    public enum CullMode
    {
        Off = 0,
        Front = 1,
        Back = 2,
    }

    public struct RenderQueueRequirement
    {
        public int DefaultValue;
        public int MinValue;
        public int MaxValue;
    }
}