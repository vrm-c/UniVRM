using System;
using UnityEngine;

namespace UniVRM10
{
    public static class MToonExtensions
    {
        public static VrmLib.TextureMagFilterType ToVrmLibMagFilter(this FilterMode mode)
        {
            switch (mode)
            {
                case FilterMode.Bilinear:
                case FilterMode.Trilinear:
                    return VrmLib.TextureMagFilterType.LINEAR;
                case FilterMode.Point:
                    return VrmLib.TextureMagFilterType.NEAREST;

                default:
                    throw new NotImplementedException();
            }
        }

        public static VrmLib.TextureMinFilterType ToVrmLibMinFilter(this FilterMode mode)
        {
            switch (mode)
            {
                case FilterMode.Bilinear:
                case FilterMode.Trilinear:
                    return VrmLib.TextureMinFilterType.LINEAR;
                case FilterMode.Point:
                    return VrmLib.TextureMinFilterType.NEAREST;

                default:
                    throw new NotImplementedException();
            }
        }

        public static VrmLib.TextureWrapType ToVrmLib(this TextureWrapMode mode)
        {
            switch (mode)
            {
                case TextureWrapMode.Clamp:
                    return VrmLib.TextureWrapType.CLAMP_TO_EDGE;

                case TextureWrapMode.Repeat:
                    return VrmLib.TextureWrapType.REPEAT;

                case TextureWrapMode.Mirror:
                    return VrmLib.TextureWrapType.MIRRORED_REPEAT;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// MToon.MToonDefinition(Unity) を VrmLib.MToon.MToonDefinition に変換する
        /// <summary>
        public static VrmLib.MToon.MToonDefinition ToVrmLib(this global::MToon.MToonDefinition unity,
            Material material,
            GetOrCreateTextureDelegate getOrCreateTexture)
        {
            return new VrmLib.MToon.MToonDefinition
            {
                Color = new VrmLib.MToon.ColorDefinition
                {
                    CutoutThresholdValue = unity.Color.CutoutThresholdValue,
                    LitColor = unity.Color.LitColor.FromUnitySrgbToLinear(),
                    LitMultiplyTexture = unity.Color.LitMultiplyTexture.ToVrmLib(getOrCreateTexture, material, VrmLib.Texture.ColorSpaceTypes.Srgb),
                    ShadeColor = unity.Color.ShadeColor.FromUnitySrgbToLinear(),
                    ShadeMultiplyTexture = unity.Color.ShadeMultiplyTexture.ToVrmLib(getOrCreateTexture, material, VrmLib.Texture.ColorSpaceTypes.Srgb),
                },
                Emission = new VrmLib.MToon.EmissionDefinition
                {
                    EmissionColor = unity.Emission.EmissionColor.FromUnityLinear(),
                    EmissionMultiplyTexture = unity.Emission.EmissionMultiplyTexture.ToVrmLib(getOrCreateTexture, material, VrmLib.Texture.ColorSpaceTypes.Srgb),
                },
                Lighting = new VrmLib.MToon.LightingDefinition
                {
                    LightingInfluence = new VrmLib.MToon.LightingInfluenceDefinition
                    {
                        GiIntensityValue = unity.Lighting.LightingInfluence.GiIntensityValue,
                        LightColorAttenuationValue = unity.Lighting.LightingInfluence.LightColorAttenuationValue,
                    },
                    LitAndShadeMixing = new VrmLib.MToon.LitAndShadeMixingDefinition
                    {
                        ShadingShiftValue = unity.Lighting.LitAndShadeMixing.ShadingShiftValue,
                        ShadingToonyValue = unity.Lighting.LitAndShadeMixing.ShadingToonyValue,
                    },
                    Normal = new VrmLib.MToon.NormalDefinition
                    {
                        NormalScaleValue = unity.Lighting.Normal.NormalScaleValue,
                        NormalTexture = unity.Lighting.Normal.NormalTexture.ToVrmLib(getOrCreateTexture, material, VrmLib.Texture.ColorSpaceTypes.Linear, VrmLib.Texture.TextureTypes.NormalMap),
                    },
                },
                MatCap = new VrmLib.MToon.MatCapDefinition
                {
                    AdditiveTexture = unity.MatCap.AdditiveTexture.ToVrmLib(getOrCreateTexture, material, VrmLib.Texture.ColorSpaceTypes.Srgb),
                },
                Meta = new VrmLib.MToon.MetaDefinition
                {
                    Implementation = unity.Meta.Implementation,
                    VersionNumber = unity.Meta.VersionNumber,
                },
                Outline = new VrmLib.MToon.OutlineDefinition
                {
                    OutlineColor = unity.Outline.OutlineColor.FromUnitySrgbToLinear(),
                    OutlineColorMode = (VrmLib.MToon.OutlineColorMode)unity.Outline.OutlineColorMode,
                    OutlineLightingMixValue = unity.Outline.OutlineLightingMixValue,
                    OutlineScaledMaxDistanceValue = unity.Outline.OutlineScaledMaxDistanceValue,
                    OutlineWidthMode = (VrmLib.MToon.OutlineWidthMode)unity.Outline.OutlineWidthMode,
                    OutlineWidthMultiplyTexture = unity.Outline.OutlineWidthMultiplyTexture.ToVrmLib(getOrCreateTexture, material, VrmLib.Texture.ColorSpaceTypes.Linear),
                    OutlineWidthValue = unity.Outline.OutlineWidthValue,
                },
                Rendering = new VrmLib.MToon.RenderingDefinition
                {
                    CullMode = (VrmLib.MToon.CullMode)unity.Rendering.CullMode,
                    RenderMode = (VrmLib.MToon.RenderMode)unity.Rendering.RenderMode,
                    RenderQueueOffsetNumber = unity.Rendering.RenderQueueOffsetNumber,
                },
                Rim = new VrmLib.MToon.RimDefinition
                {
                    RimColor = unity.Rim.RimColor.FromUnityLinear(),
                    RimFresnelPowerValue = unity.Rim.RimFresnelPowerValue,
                    RimLiftValue = unity.Rim.RimLiftValue,
                    RimLightingMixValue = unity.Rim.RimLightingMixValue,
                    RimMultiplyTexture = unity.Rim.RimMultiplyTexture.ToVrmLib(getOrCreateTexture, material, VrmLib.Texture.ColorSpaceTypes.Srgb),
                },
                TextureOption = new VrmLib.MToon.TextureUvCoordsDefinition
                {
                    MainTextureLeftBottomOriginOffset = unity.TextureOption.MainTextureLeftBottomOriginOffset.ToNumericsVector2(),
                    MainTextureLeftBottomOriginScale = unity.TextureOption.MainTextureLeftBottomOriginScale.ToNumericsVector2(),
                    UvAnimationMaskTexture = unity.TextureOption.UvAnimationMaskTexture.ToVrmLib(getOrCreateTexture, material, VrmLib.Texture.ColorSpaceTypes.Linear),
                    UvAnimationRotationSpeedValue = unity.TextureOption.UvAnimationRotationSpeedValue,
                    UvAnimationScrollXSpeedValue = unity.TextureOption.UvAnimationScrollXSpeedValue,
                    UvAnimationScrollYSpeedValue = unity.TextureOption.UvAnimationScrollYSpeedValue,
                },
            };
        }

        static VrmLib.TextureInfo ToVrmLib(this Texture2D src, GetOrCreateTextureDelegate map, Material material, VrmLib.Texture.ColorSpaceTypes colorSpace, VrmLib.Texture.TextureTypes textureType = VrmLib.Texture.TextureTypes.Default)
        {
            return map(material, src, colorSpace, textureType);
        }
    }
}
