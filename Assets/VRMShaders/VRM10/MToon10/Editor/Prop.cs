namespace VRMShaders.VRM10.MToon10.Editor
{
    public enum Prop
    {
        // Rendering
        AlphaMode,
        TransparentWithZWrite,
        AlphaCutoff,
        RenderQueueOffsetNumber,
        DoubleSided,

        // Lighting
        BaseColorFactor,

        // Unity Required
        UnityCullMode,
        UnitySrcBlend,
        UnityDstBlend,
        UnityZWrite,
        UnityAlphaToMask,
    }
}