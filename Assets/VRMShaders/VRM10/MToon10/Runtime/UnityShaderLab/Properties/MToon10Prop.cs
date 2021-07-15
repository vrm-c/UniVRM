namespace VRMShaders.VRM10.MToon10.Runtime
{
    public enum MToon10Prop
    {
        // Rendering
        AlphaMode,
        TransparentWithZWrite,
        AlphaCutoff,
        RenderQueueOffsetNumber,
        DoubleSided,

        // Lighting
        BaseColorFactor,
        BaseColorTexture,
        ShadeColorFactor,
        ShadeColorTexture,
        NormalTexture,
        NormalTextureScale,
        ShadingShiftFactor,
        ShadingShiftTexture,
        ShadingShiftTextureScale,
        ShadingToonyFactor,

        // GI
        GiEqualizationFactor,

        // Emission
        EmissiveFactor,
        EmissiveTexture,

        // Rim Lighting
        MatcapTexture,
        ParametricRimColorFactor,
        ParametricRimFresnelPowerFactor,
        ParametricRimLiftFactor,
        RimMultiplyTexture,
        RimLightingMixFactor,

        // Outline
        OutlineWidthMode,
        OutlineWidthFactor,
        OutlineWidthMultiplyTexture,
        OutlineColorFactor,
        OutlineLightingMixFactor,

        // UV Animation
        UvAnimationMaskTexture,
        UvAnimationScrollXSpeedFactor,
        UvAnimationScrollYSpeedFactor,
        UvAnimationRotationSpeedFactor,

        // Unity Required
        UnityCullMode,
        UnitySrcBlend,
        UnityDstBlend,
        UnityZWrite,
        UnityAlphaToMask,

        // for Editor
        EditorEditMode,
    }
}