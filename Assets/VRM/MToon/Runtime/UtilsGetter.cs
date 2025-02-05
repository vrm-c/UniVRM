using System;
using UnityEngine;
using UniGLTF;

namespace MToon
{
    public static partial class Utils
    {
        public static MToonDefinition GetMToonParametersFromMaterial(Material material)
        {
            return new MToonDefinition
            {
                Meta = new MetaDefinition
                {
                    Implementation = Implementation,
                    VersionNumber = material.GetInt(PropVersion),
                },
                Rendering = new RenderingDefinition
                {
                    RenderMode = GetBlendMode(material),
                    CullMode = GetCullMode(material),
                    RenderQueueOffsetNumber = GetRenderQueueOffset(material, GetRenderQueueOriginMode(material)),
                },
                Color = new ColorDefinition
                {
                    LitColor = GetColor(material, PropColor),
                    LitMultiplyTexture = GetTexture(material, PropMainTex),
                    ShadeColor = GetColor(material, PropShadeColor),
                    ShadeMultiplyTexture = GetTexture(material, PropShadeTexture),
                    CutoutThresholdValue = GetValue(material, PropCutoff),
                },
                Lighting = new LightingDefinition
                {
                    LitAndShadeMixing = new LitAndShadeMixingDefinition
                    {
                        ShadingShiftValue = GetValue(material, PropShadeShift),
                        ShadingToonyValue = GetValue(material, PropShadeToony),
                        ShadowReceiveMultiplierValue = GetValue(material, PropReceiveShadowRate),
                        ShadowReceiveMultiplierMultiplyTexture = GetTexture(material, PropReceiveShadowTexture),
                        LitAndShadeMixingMultiplierValue = GetValue(material, PropShadingGradeRate),
                        LitAndShadeMixingMultiplierMultiplyTexture = GetTexture(material, PropShadingGradeTexture),
                    },
                    LightingInfluence = new LightingInfluenceDefinition
                    {
                        LightColorAttenuationValue = GetValue(material, PropLightColorAttenuation),
                        GiIntensityValue = GetValue(material, PropIndirectLightIntensity),
                    },
                    Normal = new NormalDefinition
                    {
                        NormalTexture = GetTexture(material, PropBumpMap),
                        NormalScaleValue = GetValue(material, PropBumpScale),
                    },
                },
                Emission = new EmissionDefinition
                {
                    EmissionColor = GetColor(material, PropEmissionColor),
                    EmissionMultiplyTexture = GetTexture(material, PropEmissionMap),
                },
                MatCap = new MatCapDefinition
                {
                    AdditiveTexture = GetTexture(material, PropSphereAdd),
                },
                Rim = new RimDefinition
                {
                    RimColor = GetColor(material, PropRimColor),
                    RimMultiplyTexture = GetTexture(material, PropRimTexture),
                    RimLightingMixValue = GetValue(material, PropRimLightingMix),
                    RimFresnelPowerValue = GetValue(material, PropRimFresnelPower),
                    RimLiftValue = GetValue(material, PropRimLift),
                },
                Outline = new OutlineDefinition
                {
                    OutlineWidthMode = GetOutlineWidthMode(material),
                    OutlineWidthValue = GetValue(material, PropOutlineWidth),
                    OutlineWidthMultiplyTexture = GetTexture(material, PropOutlineWidthTexture),
                    OutlineScaledMaxDistanceValue = GetValue(material, PropOutlineScaledMaxDistance),
                    OutlineColorMode = GetOutlineColorMode(material),
                    OutlineColor = GetColor(material, PropOutlineColor),
                    OutlineLightingMixValue = GetValue(material, PropOutlineLightingMix),
                },
                TextureOption = new TextureUvCoordsDefinition
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

        private static RenderMode GetBlendMode(Material material)
        {
            if (material.IsKeywordEnabled(KeyAlphaTestOn))
            {
                return RenderMode.Cutout;
            }
            else if (material.IsKeywordEnabled(KeyAlphaBlendOn))
            {
                switch (material.GetInt(PropZWrite))
                {
                    case EnabledIntValue:
                        return RenderMode.TransparentWithZWrite;
                    case DisabledIntValue:
                        return RenderMode.Transparent;
                    default:
                        UniGLTFLogger.Warning("Invalid ZWrite Int Value.");
                        return RenderMode.Transparent;
                }
            }
            else
            {
                return RenderMode.Opaque;
            }
        }

        private static CullMode GetCullMode(Material material)
        {
            switch ((CullMode) material.GetInt(PropCullMode))
            {
                case CullMode.Off:
                    return CullMode.Off;
                case CullMode.Front:
                    return CullMode.Front;
                case CullMode.Back:
                    return CullMode.Back;
                default:
                    UniGLTFLogger.Warning("Invalid CullMode.");
                    return CullMode.Back;
            }
        }

        private static OutlineWidthMode GetOutlineWidthMode(Material material)
        {
            if (material.IsKeywordEnabled(KeyOutlineWidthWorld)) return OutlineWidthMode.WorldCoordinates;
            if (material.IsKeywordEnabled(KeyOutlineWidthScreen)) return OutlineWidthMode.ScreenCoordinates;
            
            return OutlineWidthMode.None;
        }

        private static OutlineColorMode GetOutlineColorMode(Material material)
        {
            if (material.IsKeywordEnabled(KeyOutlineColorFixed)) return OutlineColorMode.FixedColor;
            if (material.IsKeywordEnabled(KeyOutlineColorMixed)) return OutlineColorMode.MixedLighting;
            
            return OutlineColorMode.FixedColor;
        }

        private static RenderMode GetRenderQueueOriginMode(Material material)
        {
            return GetBlendMode(material);
        }

        private static int GetRenderQueueOffset(Material material, RenderMode originMode)
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