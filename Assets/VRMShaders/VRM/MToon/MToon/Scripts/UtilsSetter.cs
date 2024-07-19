using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MToon
{
    public static partial class Utils
    {
        public static void SetMToonParametersToMaterial(Material material, MToonDefinition parameters)
        {
            {
                var meta = parameters.Meta;
                SetValue(material, PropVersion, meta.VersionNumber);
            }
            {
                var rendering = parameters.Rendering;
                SetRenderMode(material, rendering.RenderMode, rendering.RenderQueueOffsetNumber,
                    useDefaultRenderQueue: false);
                SetCullMode(material, rendering.CullMode);
            }
            {
                var color = parameters.Color;
                SetColor(material, PropColor, color.LitColor);
                SetTexture(material, PropMainTex, color.LitMultiplyTexture);
                SetColor(material, PropShadeColor, color.ShadeColor);
                SetTexture(material, PropShadeTexture, color.ShadeMultiplyTexture);
                SetValue(material, PropCutoff, color.CutoutThresholdValue);
            }
            {
                var lighting = parameters.Lighting;
                {
                    var prop = lighting.LitAndShadeMixing;
                    SetValue(material, PropShadeShift, prop.ShadingShiftValue);
                    SetValue(material, PropShadeToony, prop.ShadingToonyValue);
                    SetValue(material, PropReceiveShadowRate, prop.ShadowReceiveMultiplierValue);
                    SetTexture(material, PropReceiveShadowTexture, prop.ShadowReceiveMultiplierMultiplyTexture);
                    SetValue(material, PropShadingGradeRate, prop.LitAndShadeMixingMultiplierValue);
                    SetTexture(material, PropShadingGradeTexture, prop.LitAndShadeMixingMultiplierMultiplyTexture);
                }
                {
                    var prop = lighting.LightingInfluence;
                    SetValue(material, PropLightColorAttenuation, prop.LightColorAttenuationValue);
                    SetValue(material, PropIndirectLightIntensity, prop.GiIntensityValue);
                }
                {
                    var prop = lighting.Normal;
                    SetNormalMapping(material, prop.NormalTexture, prop.NormalScaleValue);
                }
            }
            {
                var emission = parameters.Emission;
                SetColor(material, PropEmissionColor, emission.EmissionColor);
                SetTexture(material, PropEmissionMap, emission.EmissionMultiplyTexture);
            }
            {
                var matcap = parameters.MatCap;
                SetTexture(material, PropSphereAdd, matcap.AdditiveTexture);
            }
            {
                var rim = parameters.Rim;
                SetColor(material, PropRimColor, rim.RimColor);
                SetTexture(material, PropRimTexture, rim.RimMultiplyTexture);
                SetValue(material, PropRimLightingMix, rim.RimLightingMixValue);
                SetValue(material, PropRimFresnelPower, rim.RimFresnelPowerValue);
                SetValue(material, PropRimLift, rim.RimLiftValue);
            }
            {
                var outline = parameters.Outline;
                SetValue(material, PropOutlineWidth, outline.OutlineWidthValue);
                SetTexture(material, PropOutlineWidthTexture, outline.OutlineWidthMultiplyTexture);
                SetValue(material, PropOutlineScaledMaxDistance, outline.OutlineScaledMaxDistanceValue);
                SetColor(material, PropOutlineColor, outline.OutlineColor);
                SetValue(material, PropOutlineLightingMix, outline.OutlineLightingMixValue);
                SetOutlineMode(material, outline.OutlineWidthMode, outline.OutlineColorMode);
            }
            {
                var textureOptions = parameters.TextureOption;
                material.SetTextureScale(PropMainTex, textureOptions.MainTextureLeftBottomOriginScale);
                material.SetTextureOffset(PropMainTex, textureOptions.MainTextureLeftBottomOriginOffset);
                material.SetTexture(PropUvAnimMaskTexture, textureOptions.UvAnimationMaskTexture);
                material.SetFloat(PropUvAnimScrollX, textureOptions.UvAnimationScrollXSpeedValue);
                material.SetFloat(PropUvAnimScrollY, textureOptions.UvAnimationScrollYSpeedValue);
                material.SetFloat(PropUvAnimRotation, textureOptions.UvAnimationRotationSpeedValue);
            }
            
            ValidateProperties(material, isBlendModeChangedByUser: false);
        }

        /// <summary>
        /// Validate properties and Set hidden properties, keywords.
        /// if isBlendModeChangedByUser is true, renderQueue will set specified render mode's default value.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="isBlendModeChangedByUser"></param>
        public static void ValidateProperties(Material material, bool isBlendModeChangedByUser = false)
        {
            SetRenderMode(material,
                (RenderMode) material.GetFloat(PropBlendMode),
                material.renderQueue - GetRenderQueueRequirement((RenderMode) material.GetFloat(PropBlendMode)).DefaultValue,
                useDefaultRenderQueue: isBlendModeChangedByUser);
            SetNormalMapping(material, material.GetTexture(PropBumpMap), material.GetFloat(PropBumpScale));
            SetOutlineMode(material,
                (OutlineWidthMode) material.GetFloat(PropOutlineWidthMode),
                (OutlineColorMode) material.GetFloat(PropOutlineColorMode));
            SetDebugMode(material, (DebugMode) material.GetFloat(PropDebugMode));
            SetCullMode(material, (CullMode) material.GetFloat(PropCullMode));

            var mainTex = material.GetTexture(PropMainTex);
            var shadeTex = material.GetTexture(PropShadeTexture);
            if (mainTex != null && shadeTex == null)
            {
                material.SetTexture(PropShadeTexture, mainTex);
            }
        }

        private static void SetDebugMode(Material material, DebugMode debugMode)
        {
            SetValue(material, PropDebugMode, (int) debugMode);
            
            switch (debugMode)
            {
                case DebugMode.None:
                    SetKeyword(material, KeyDebugNormal, false);
                    SetKeyword(material, KeyDebugLitShadeRate, false);
                    break;
                case DebugMode.Normal:
                    SetKeyword(material, KeyDebugNormal, true);
                    SetKeyword(material, KeyDebugLitShadeRate, false);
                    break;
                case DebugMode.LitShadeRate:
                    SetKeyword(material, KeyDebugNormal, false);
                    SetKeyword(material, KeyDebugLitShadeRate, true);
                    break;
            }
        }

        private static void SetRenderMode(Material material, RenderMode renderMode, int renderQueueOffset,
            bool useDefaultRenderQueue)
        {
            SetValue(material, PropBlendMode, (int) renderMode);
            
            switch (renderMode)
            {
                case RenderMode.Opaque:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueOpaque);
                    material.SetInt(PropSrcBlend, (int) BlendMode.One);
                    material.SetInt(PropDstBlend, (int) BlendMode.Zero);
                    material.SetInt(PropZWrite, EnabledIntValue);
                    material.SetInt(PropAlphaToMask, DisabledIntValue);
                    SetKeyword(material, KeyAlphaTestOn, false);
                    SetKeyword(material, KeyAlphaBlendOn, false);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.Cutout:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparentCutout);
                    material.SetInt(PropSrcBlend, (int) BlendMode.One);
                    material.SetInt(PropDstBlend, (int) BlendMode.Zero);
                    material.SetInt(PropZWrite, EnabledIntValue);
                    material.SetInt(PropAlphaToMask, EnabledIntValue);
                    SetKeyword(material, KeyAlphaTestOn, true);
                    SetKeyword(material, KeyAlphaBlendOn, false);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.Transparent:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparent);
                    material.SetInt(PropSrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(PropDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(PropZWrite, DisabledIntValue);
                    material.SetInt(PropAlphaToMask, DisabledIntValue);
                    SetKeyword(material, KeyAlphaTestOn, false);
                    SetKeyword(material, KeyAlphaBlendOn, true);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.TransparentWithZWrite:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparent);
                    material.SetInt(PropSrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(PropDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(PropZWrite, EnabledIntValue);
                    material.SetInt(PropAlphaToMask, DisabledIntValue);
                    SetKeyword(material, KeyAlphaTestOn, false);
                    SetKeyword(material, KeyAlphaBlendOn, true);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
            }

            if (useDefaultRenderQueue)
            {
                var requirement = GetRenderQueueRequirement(renderMode);
                material.renderQueue = requirement.DefaultValue;
            }
            else
            {
                var requirement = GetRenderQueueRequirement(renderMode);
                material.renderQueue = Mathf.Clamp(
                    requirement.DefaultValue + renderQueueOffset, requirement.MinValue, requirement.MaxValue);
            }
        }

        private static void SetOutlineMode(Material material, OutlineWidthMode outlineWidthMode,
            OutlineColorMode outlineColorMode)
        {
            SetValue(material, PropOutlineWidthMode, (int) outlineWidthMode);
            SetValue(material, PropOutlineColorMode, (int) outlineColorMode);
            
            var isFixed = outlineColorMode == OutlineColorMode.FixedColor;
            var isMixed = outlineColorMode == OutlineColorMode.MixedLighting;
            
            switch (outlineWidthMode)
            {
                case OutlineWidthMode.None:
                    SetKeyword(material, KeyOutlineWidthWorld, false);
                    SetKeyword(material, KeyOutlineWidthScreen, false);
                    SetKeyword(material, KeyOutlineColorFixed, false);
                    SetKeyword(material, KeyOutlineColorMixed, false);
                    break;
                case OutlineWidthMode.WorldCoordinates:
                    SetKeyword(material, KeyOutlineWidthWorld, true);
                    SetKeyword(material, KeyOutlineWidthScreen, false);
                    SetKeyword(material, KeyOutlineColorFixed, isFixed);
                    SetKeyword(material, KeyOutlineColorMixed, isMixed);
                    break;
                case OutlineWidthMode.ScreenCoordinates:
                    SetKeyword(material, KeyOutlineWidthWorld, false);
                    SetKeyword(material, KeyOutlineWidthScreen, true);
                    SetKeyword(material, KeyOutlineColorFixed, isFixed);
                    SetKeyword(material, KeyOutlineColorMixed, isMixed);
                    break;
            }
        }

        private static void SetNormalMapping(Material material, Texture bumpMap, float bumpScale)
        {
            SetTexture(material, PropBumpMap, bumpMap);
            SetValue(material, PropBumpScale, bumpScale);
            
            SetKeyword(material, KeyNormalMap, bumpMap != null);
        }

        private static void SetCullMode(Material material, CullMode cullMode)
        {
            SetValue(material, PropCullMode, (int) cullMode);
            
            switch (cullMode)
            {
                case CullMode.Back:
                    material.SetInt(PropCullMode, (int) CullMode.Back);
                    material.SetInt(PropOutlineCullMode, (int) CullMode.Front);
                    break;
                case CullMode.Front:
                    material.SetInt(PropCullMode, (int) CullMode.Front);
                    material.SetInt(PropOutlineCullMode, (int) CullMode.Back);
                    break;
                case CullMode.Off:
                    material.SetInt(PropCullMode, (int) CullMode.Off);
                    material.SetInt(PropOutlineCullMode, (int) CullMode.Front);
                    break;
            }
        }

        private static void SetValue(Material material, string propertyName, float val)
        {
            material.SetFloat(propertyName, val);
        }

        private static void SetColor(Material material, string propertyName, Color color)
        {
            material.SetColor(propertyName, color);
        }

        private static void SetTexture(Material material, string propertyName, Texture texture)
        {
            material.SetTexture(propertyName, texture);
        }

        private static void SetKeyword(Material mat, string keyword, bool required)
        {
            if (required)
                mat.EnableKeyword(keyword);
            else
                mat.DisableKeyword(keyword);
        }
    }
}