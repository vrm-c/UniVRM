using UniGLTF;
using UnityEngine;

namespace VRM10.MToon10.MToon0X
{
    public static partial class MToon0XUtils
    {
        public static MToon0XDefinition GetMToonParametersFromMaterial(Material material)
        {
            return new MToon0XDefinition
            {
                Meta = new MToon0XMetaDefinition
                {
                    Implementation = MToon0XUtils.Implementation,
                    VersionNumber = material.GetInt(PropVersion),
                },
                Rendering = new MToon0XRenderingDefinition
                {
                    RenderMode = GetBlendMode(material),
                    CullMode = GetCullMode(material),
                    RenderQueueOffsetNumber = GetRenderQueueOffset(material, GetRenderQueueOriginMode(material)),
                },
                Color = new MToon0XColorDefinition
                {
                    LitColor = GetColor(material, PropColor),
                    LitMultiplyTexture = GetTexture(material, PropMainTex),
                    ShadeColor = GetColor(material, PropShadeColor),
                    ShadeMultiplyTexture = GetTexture(material, PropShadeTexture),
                    CutoutThresholdValue = GetValue(material, PropCutoff),
                },
                Lighting = new MToon0XLightingDefinition
                {
                    LitAndShadeMixing = new MToon0XLitAndShadeMixingDefinition
                    {
                        ShadingShiftValue = GetValue(material, PropShadeShift),
                        ShadingToonyValue = GetValue(material, PropShadeToony),
                        ShadowReceiveMultiplierValue = GetValue(material, PropReceiveShadowRate),
                        ShadowReceiveMultiplierMultiplyTexture = GetTexture(material, PropReceiveShadowTexture),
                        LitAndShadeMixingMultiplierValue = GetValue(material, PropShadingGradeRate),
                        LitAndShadeMixingMultiplierMultiplyTexture = GetTexture(material, PropShadingGradeTexture),
                    },
                    LightingInfluence = new MToon0XLightingInfluenceDefinition
                    {
                        LightColorAttenuationValue = GetValue(material, PropLightColorAttenuation),
                        GiIntensityValue = GetValue(material, PropIndirectLightIntensity),
                    },
                    Normal = new MToon0XNormalDefinition
                    {
                        NormalTexture = GetTexture(material, PropBumpMap),
                        NormalScaleValue = GetValue(material, PropBumpScale),
                    },
                },
                Emission = new MToon0XEmissionDefinition
                {
                    EmissionColor = GetColor(material, PropEmissionColor),
                    EmissionMultiplyTexture = GetTexture(material, PropEmissionMap),
                },
                MatCap = new MToon0XMatCapDefinition
                {
                    AdditiveTexture = GetTexture(material, PropSphereAdd),
                },
                Rim = new MToon0XRimDefinition
                {
                    RimColor = GetColor(material, PropRimColor),
                    RimMultiplyTexture = GetTexture(material, PropRimTexture),
                    RimLightingMixValue = GetValue(material, PropRimLightingMix),
                    RimFresnelPowerValue = GetValue(material, PropRimFresnelPower),
                    RimLiftValue = GetValue(material, PropRimLift),
                },
                Outline = new MToon0XOutlineDefinition
                {
                    OutlineWidthMode = GetOutlineWidthMode(material),
                    OutlineWidthValue = GetValue(material, PropOutlineWidth),
                    OutlineWidthMultiplyTexture = GetTexture(material, PropOutlineWidthTexture),
                    OutlineScaledMaxDistanceValue = GetValue(material, PropOutlineScaledMaxDistance),
                    OutlineColorMode = GetOutlineColorMode(material),
                    OutlineColor = GetColor(material, PropOutlineColor),
                    OutlineLightingMixValue = GetValue(material, PropOutlineLightingMix),
                },
                TextureOption = new MToon0XTextureUvCoordsDefinition
                {
                    MainTextureLeftBottomOriginScale = material.GetTextureScale(PropMainTex),
                    MainTextureLeftBottomOriginOffset = material.GetTextureOffset(PropMainTex),
                    UvAnimationMaskTexture = GetTexture(material, PropUvAnimMaskTexture),
                    UvAnimationScrollXSpeedValue = GetValue(material, PropUvAnimScrollX),
                    UvAnimationScrollYSpeedValue = GetValue(material, PropUvAnimScrollY),
                    UvAnimationRotationSpeedValue = GetValue(material, PropUvAnimRotation),
                },
            };
        }

        private static float GetValue(Material material, string propertyName)
        {
            return material.GetFloat(propertyName);
        }

        private static Color GetColor(Material material, string propertyName)
        {
            return material.GetColor(propertyName);
        }

        private static Texture2D GetTexture(Material material, string propertyName)
        {
            return (Texture2D) material.GetTexture(propertyName);
        }

        private static MToon0XRenderMode GetBlendMode(Material material)
        {
            if (material.IsKeywordEnabled(KeyAlphaTestOn))
            {
                return MToon0XRenderMode.Cutout;
            }
            else if (material.IsKeywordEnabled(KeyAlphaBlendOn))
            {
                switch (material.GetInt(PropZWrite))
                {
                    case EnabledIntValue:
                        return MToon0XRenderMode.TransparentWithZWrite;
                    case DisabledIntValue:
                        return MToon0XRenderMode.Transparent;
                    default:
                        UniGLTFLogger.Warning("Invalid ZWrite Int Value.");
                        return MToon0XRenderMode.Transparent;
                }
            }
            else
            {
                return MToon0XRenderMode.Opaque;
            }
        }

        private static MToon0XCullMode GetCullMode(Material material)
        {
            switch ((MToon0XCullMode) material.GetInt(PropCullMode))
            {
                case MToon0XCullMode.Off:
                    return MToon0XCullMode.Off;
                case MToon0XCullMode.Front:
                    return MToon0XCullMode.Front;
                case MToon0XCullMode.Back:
                    return MToon0XCullMode.Back;
                default:
                    UniGLTFLogger.Warning("Invalid CullMode.");
                    return MToon0XCullMode.Back;
            }
        }

        private static MToon0XOutlineWidthMode GetOutlineWidthMode(Material material)
        {
            if (material.IsKeywordEnabled(KeyOutlineWidthWorld)) return MToon0XOutlineWidthMode.WorldCoordinates;
            if (material.IsKeywordEnabled(KeyOutlineWidthScreen)) return MToon0XOutlineWidthMode.ScreenCoordinates;
            
            return MToon0XOutlineWidthMode.None;
        }

        private static MToon0XOutlineColorMode GetOutlineColorMode(Material material)
        {
            if (material.IsKeywordEnabled(KeyOutlineColorFixed)) return MToon0XOutlineColorMode.FixedColor;
            if (material.IsKeywordEnabled(KeyOutlineColorMixed)) return MToon0XOutlineColorMode.MixedLighting;
            
            return MToon0XOutlineColorMode.FixedColor;
        }

        private static MToon0XRenderMode GetRenderQueueOriginMode(Material material)
        {
            return GetBlendMode(material);
        }

        private static int GetRenderQueueOffset(Material material, MToon0XRenderMode originMode)
        {
            var rawValue = material.renderQueue;
            var requirement = GetRenderQueueRequirement(originMode);
            if (rawValue < requirement.MinValue || rawValue > requirement.MaxValue)
            {
                return 0;
            }
            return rawValue - requirement.DefaultValue;
        }
    }
}