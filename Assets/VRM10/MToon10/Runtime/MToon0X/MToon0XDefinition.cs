using UnityEngine;

namespace VRM10.MToon10.MToon0X
{
    public class MToon0XDefinition
    {
        public MToon0XMetaDefinition Meta;
        public MToon0XRenderingDefinition Rendering;
        public MToon0XColorDefinition Color;
        public MToon0XLightingDefinition Lighting;
        public MToon0XEmissionDefinition Emission;
        public MToon0XMatCapDefinition MatCap;
        public MToon0XRimDefinition Rim;
        public MToon0XOutlineDefinition Outline;
        public MToon0XTextureUvCoordsDefinition TextureOption;
    }

    public class MToon0XMetaDefinition
    {
        public string Implementation;
        public int VersionNumber;
    }

    public class MToon0XRenderingDefinition
    {
        public MToon0XRenderMode RenderMode;
        public MToon0XCullMode CullMode;
        public int RenderQueueOffsetNumber;
    }

    public class MToon0XColorDefinition
    {
        public Color LitColor;
        public Texture2D LitMultiplyTexture;
        public Color ShadeColor;
        public Texture2D ShadeMultiplyTexture;
        public float CutoutThresholdValue;
    }

    public class MToon0XLightingDefinition
    {
        public MToon0XLitAndShadeMixingDefinition LitAndShadeMixing;
        public MToon0XLightingInfluenceDefinition LightingInfluence;
        public MToon0XNormalDefinition Normal;
    }

    public class MToon0XLitAndShadeMixingDefinition
    {
        public float ShadingShiftValue;
        public float ShadingToonyValue;
        public float ShadowReceiveMultiplierValue;
        public Texture2D ShadowReceiveMultiplierMultiplyTexture;
        public float LitAndShadeMixingMultiplierValue;
        public Texture2D LitAndShadeMixingMultiplierMultiplyTexture;
    }

    public class MToon0XLightingInfluenceDefinition
    {
        public float LightColorAttenuationValue;
        public float GiIntensityValue;
    }

    public class MToon0XEmissionDefinition
    {
        public Color EmissionColor;
        public Texture2D EmissionMultiplyTexture;
    }

    public class MToon0XMatCapDefinition
    {
        public Texture2D AdditiveTexture;
    }

    public class MToon0XRimDefinition
    {
        public Color RimColor;
        public Texture2D RimMultiplyTexture;
        public float RimLightingMixValue;
        public float RimFresnelPowerValue;
        public float RimLiftValue;
    }

    public class MToon0XNormalDefinition
    {
        public Texture2D NormalTexture;
        public float NormalScaleValue;
    }

    public class MToon0XOutlineDefinition
    {
        public MToon0XOutlineWidthMode OutlineWidthMode;
        public float OutlineWidthValue;
        public Texture2D OutlineWidthMultiplyTexture;
        public float OutlineScaledMaxDistanceValue;
        public MToon0XOutlineColorMode OutlineColorMode;
        public Color OutlineColor;
        public float OutlineLightingMixValue;
    }

    public class MToon0XTextureUvCoordsDefinition
    {
        public Vector2 MainTextureLeftBottomOriginScale;
        public Vector2 MainTextureLeftBottomOriginOffset;
        public Texture2D UvAnimationMaskTexture;
        public float UvAnimationScrollXSpeedValue;
        public float UvAnimationScrollYSpeedValue;
        public float UvAnimationRotationSpeedValue;
    }
}