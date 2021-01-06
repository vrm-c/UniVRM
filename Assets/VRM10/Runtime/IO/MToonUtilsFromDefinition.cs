using Color = System.Numerics.Vector4;
using Material = UniGLTF.glTFMaterial;
using System.Collections.Generic;
using VrmLib;
using VrmLib.MToon;
using System;

namespace UniVRM10
{
    public static partial class Utils
    {
        public static void ToProtobuf(this LinearColor color, Action<float> add, bool hasAlpha)
        {
            add(color.RGBA.X);
            add(color.RGBA.Y);
            add(color.RGBA.Z);
            if (hasAlpha)
            {
                add(color.RGBA.W);
            }
        }

        // public static void SetMToonParametersToMaterial(Material material, MToonDefinition parameters, List<Texture> textures)
        // {
        //     var mtoon = material.Extensions.VRMCMaterialsMtoon;

        //     {
        //         var meta = parameters.Meta;
        //         mtoon.Version = meta.VersionNumber.ToString();
        //     }
        //     // TODO:
        //     // {
        //     //     var rendering = parameters.Rendering;
        //     //     ValidateBlendMode(material, rendering.RenderMode, isChangedByUser: true);
        //     //     ValidateCullMode(material, rendering.CullMode);
        //     //     ValidateRenderQueue(material, offset: rendering.RenderQueueOffsetNumber);
        //     // }
        //     {
        //         var color = parameters.Color;

        //         // SetColor(material, MToonUtils.PropColor, color.LitColor);
        //         color.LitColor.ToProtobuf(mtoon.LitFactor.Add, true);

        //         // SetTexture(material, MToonUtils.PropMainTex, color.LitMultiplyTexture, textures);
        //         mtoon.LitMultiplyTexture = textures.IndexOfNullable(color.LitMultiplyTexture.Texture);

        //         // SetColor(material, MToonUtils.PropShadeColor, color.ShadeColor);
        //         color.ShadeColor.ToProtobuf(mtoon.ShadeFactor.Add, false);

        //         // SetTexture(material, MToonUtils.PropShadeTexture, color.ShadeMultiplyTexture, textures);
        //         mtoon.ShadeMultiplyTexture = textures.IndexOfNullable(color.ShadeMultiplyTexture.Texture);

        //         // SetValue(material, MToonUtils.PropCutoff, color.CutoutThresholdValue);
        //         mtoon.CutoutThresholdFactor = color.CutoutThresholdValue;
        //     }
        //     {
        //         var lighting = parameters.Lighting;
        //         {
        //             var prop = lighting.LitAndShadeMixing;
        //             // SetValue(material, MToonUtils.PropShadeShift, prop.ShadingShiftValue);
        //             mtoon.ShadingShiftFactor = prop.ShadingShiftValue;

        //             // SetValue(material, MToonUtils.PropShadeToony, prop.ShadingToonyValue);
        //             mtoon.ShadingToonyFactor = prop.ShadingToonyValue;
        //         }
        //         {
        //             var prop = lighting.LightingInfluence;
        //             // SetValue(material, MToonUtils.PropLightColorAttenuation, prop.LightColorAttenuationValue);
        //             mtoon.LightColorAttenuationFactor = prop.LightColorAttenuationValue;

        //             // SetValue(material, MToonUtils.PropIndirectLightIntensity, prop.GiIntensityValue);
        //             mtoon.GiIntensityFactor = prop.GiIntensityValue;
        //         }
        //         {
        //             var prop = lighting.Normal;
        //             // SetTexture(material, MToonUtils.PropBumpMap, prop.NormalTexture, textures);
        //             mtoon.NormalTexture = textures.IndexOfNullable(prop.NormalTexture.Texture);

        //             // SetValue(material, MToonUtils.PropBumpScale, prop.NormalScaleValue);
        //             mtoon.NormalScaleFactor = prop.NormalScaleValue;
        //         }
        //     }
        //     {
        //         var emission = parameters.Emission;
        //         // SetColor(material, MToonUtils.PropEmissionColor, emission.EmissionColor);
        //         emission.EmissionColor.ToProtobuf(mtoon.EmissionFactor.Add, false);

        //         // SetTexture(material, MToonUtils.PropEmissionMap, emission.EmissionMultiplyTexture, textures);
        //         mtoon.EmissionMultiplyTexture = textures.IndexOfNullable(emission.EmissionMultiplyTexture.Texture);
        //     }
        //     {
        //         var matcap = parameters.MatCap;
        //         // SetTexture(material, MToonUtils.PropSphereAdd, matcap.AdditiveTexture, textures);
        //         mtoon.AdditiveTexture = textures.IndexOfNullable(matcap.AdditiveTexture.Texture);
        //     }
        //     {
        //         var rim = parameters.Rim;
        //         // SetColor(material, MToonUtils.PropRimColor, rim.RimColor);
        //         rim.RimColor.ToProtobuf(mtoon.RimFactor.Add, false);

        //         // SetTexture(material, MToonUtils.PropRimTexture, rim.RimMultiplyTexture, textures);
        //         mtoon.RimMultiplyTexture = textures.IndexOfNullable(rim.RimMultiplyTexture.Texture);

        //         // SetValue(material, MToonUtils.PropRimLightingMix, rim.RimLightingMixValue);
        //         mtoon.RimLightingMixFactor = rim.RimLightingMixValue;

        //         // SetValue(material, MToonUtils.PropRimFresnelPower, rim.RimFresnelPowerValue);
        //         mtoon.RimFresnelPowerFactor = rim.RimFresnelPowerValue;

        //         // SetValue(material, MToonUtils.PropRimLift, rim.RimLiftValue);
        //         mtoon.RimLiftFactor = rim.RimLiftValue;
        //     }
        //     {
        //         var outline = parameters.Outline;
        //         // SetValue(material, MToonUtils.PropOutlineWidth, outline.OutlineWidthValue);
        //         mtoon.OutlineWidthFactor = outline.OutlineWidthValue;

        //         // SetTexture(material, MToonUtils.PropOutlineWidthTexture, outline.OutlineWidthMultiplyTexture, textures);
        //         mtoon.OutlineWidthMultiplyTexture = textures.IndexOfNullable(outline.OutlineWidthMultiplyTexture.Texture);

        //         // SetValue(material, MToonUtils.PropOutlineScaledMaxDistance, outline.OutlineScaledMaxDistanceValue);
        //         mtoon.OutlineScaledMaxDistanceFactor = outline.OutlineScaledMaxDistanceValue;

        //         // SetColor(material, MToonUtils.PropOutlineColor, outline.OutlineColor);
        //         outline.OutlineColor.ToProtobuf(mtoon.OutlineFactor.Add, false);

        //         // SetValue(material, MToonUtils.PropOutlineLightingMix, outline.OutlineLightingMixValue);
        //         mtoon.OutlineLightingMixFactor = outline.OutlineLightingMixValue;

        //         // ValidateOutlineMode(material, outline.OutlineWidthMode, outline.OutlineColorMode);
        //     }
        //     {
        //         var textureOptions = parameters.TextureOption;
        //         // TODO:
        //         // material.SetTextureScale(MToonUtils.PropMainTex, textureOptions.MainTextureLeftBottomOriginScale);
        //         // material.SetTextureOffset(MToonUtils.PropMainTex, textureOptions.MainTextureLeftBottomOriginOffset);

        //         // material.SetTexture(MToonUtils.PropUvAnimMaskTexture, textureOptions.UvAnimationMaskTexture, textures);
        //         mtoon.UvAnimationMaskTexture = textures.IndexOfNullable(textureOptions.UvAnimationMaskTexture.Texture);

        //         // material.SetFloat(MToonUtils.PropUvAnimScrollX, textureOptions.UvAnimationScrollXSpeedValue);
        //         mtoon.UvAnimationScrollXSpeedFactor = textureOptions.UvAnimationScrollXSpeedValue;

        //         // material.SetFloat(MToonUtils.PropUvAnimScrollY, textureOptions.UvAnimationScrollYSpeedValue);
        //         mtoon.UvAnimationScrollYSpeedFactor = textureOptions.UvAnimationScrollYSpeedValue;

        //         // material.SetFloat(MToonUtils.PropUvAnimRotation, textureOptions.UvAnimationRotationSpeedValue);
        //         mtoon.UvAnimationRotationSpeedFactor = textureOptions.UvAnimationRotationSpeedValue;
        //     }
        // }

        // /// <summary>
        // /// Validate properties and Set hidden properties, keywords.
        // /// if isBlendModeChangedByUser is true, renderQueue will set specified render mode's default value.
        // /// </summary>
        // /// <param name="material"></param>
        // /// <param name="isBlendModeChangedByUser"></param>
        // public static void ValidateProperties(Material material, List<Texture> textures, bool isBlendModeChangedByUser = false)
        // {
        //     ValidateBlendMode(material, (RenderMode)material.GetFloat(MToonUtils.PropBlendMode), isBlendModeChangedByUser);
        //     ValidateNormalMode(material, material.GetTexture(MToonUtils.PropBumpMap, textures) != null);
        //     ValidateOutlineMode(material,
        //         (OutlineWidthMode)material.GetFloat(MToonUtils.PropOutlineWidthMode),
        //         (OutlineColorMode)material.GetFloat(MToonUtils.PropOutlineColorMode));
        //     ValidateDebugMode(material, (DebugMode)material.GetFloat(MToonUtils.PropDebugMode));
        //     ValidateCullMode(material, (CullMode)material.GetFloat(MToonUtils.PropCullMode));

        //     var mainTex = material.GetTexture(MToonUtils.PropMainTex, textures);
        //     var shadeTex = material.GetTexture(MToonUtils.PropShadeTexture, textures);
        //     if (mainTex != null && shadeTex == null)
        //     {
        //         material.SetTexture(MToonUtils.PropShadeTexture, mainTex, textures);
        //     }
        // }

        // private static void ValidateDebugMode(Material material, DebugMode debugMode)
        // {
        //     switch (debugMode)
        //     {
        //         case DebugMode.None:
        //             SetKeyword(material, MToonUtils.KeyDebugNormal, false);
        //             SetKeyword(material, MToonUtils.KeyDebugLitShadeRate, false);
        //             break;
        //         case DebugMode.Normal:
        //             SetKeyword(material, MToonUtils.KeyDebugNormal, true);
        //             SetKeyword(material, MToonUtils.KeyDebugLitShadeRate, false);
        //             break;
        //         case DebugMode.LitShadeRate:
        //             SetKeyword(material, MToonUtils.KeyDebugNormal, false);
        //             SetKeyword(material, MToonUtils.KeyDebugLitShadeRate, true);
        //             break;
        //     }
        // }

        // public static void ValidateBlendMode(Material material, RenderMode renderMode, bool isChangedByUser)
        // {
        //     switch (renderMode)
        //     {
        //         case RenderMode.Opaque:
        //             material.SetOverrideTag(MToonUtils.TagRenderTypeKey, MToonUtils.TagRenderTypeValueOpaque);
        //             material.SetInt(MToonUtils.PropSrcBlend, BlendMode.One);
        //             material.SetInt(MToonUtils.PropDstBlend, BlendMode.Zero);
        //             material.SetInt(MToonUtils.PropZWrite, MToonUtils.EnabledIntValue);
        //             material.SetInt(MToonUtils.PropAlphaToMask, MToonUtils.DisabledIntValue);
        //             SetKeyword(material, MToonUtils.KeyAlphaTestOn, false);
        //             SetKeyword(material, MToonUtils.KeyAlphaBlendOn, false);
        //             SetKeyword(material, MToonUtils.KeyAlphaPremultiplyOn, false);
        //             break;
        //         case RenderMode.Cutout:
        //             material.SetOverrideTag(MToonUtils.TagRenderTypeKey, MToonUtils.TagRenderTypeValueTransparentCutout);
        //             material.SetInt(MToonUtils.PropSrcBlend, BlendMode.One);
        //             material.SetInt(MToonUtils.PropDstBlend, BlendMode.Zero);
        //             material.SetInt(MToonUtils.PropZWrite, MToonUtils.EnabledIntValue);
        //             material.SetInt(MToonUtils.PropAlphaToMask, MToonUtils.EnabledIntValue);
        //             SetKeyword(material, MToonUtils.KeyAlphaTestOn, true);
        //             SetKeyword(material, MToonUtils.KeyAlphaBlendOn, false);
        //             SetKeyword(material, MToonUtils.KeyAlphaPremultiplyOn, false);
        //             break;
        //         case RenderMode.Transparent:
        //             material.SetOverrideTag(MToonUtils.TagRenderTypeKey, MToonUtils.TagRenderTypeValueTransparent);
        //             material.SetInt(MToonUtils.PropSrcBlend, BlendMode.SrcAlpha);
        //             material.SetInt(MToonUtils.PropDstBlend, BlendMode.OneMinusSrcAlpha);
        //             material.SetInt(MToonUtils.PropZWrite, MToonUtils.DisabledIntValue);
        //             material.SetInt(MToonUtils.PropAlphaToMask, MToonUtils.DisabledIntValue);
        //             SetKeyword(material, MToonUtils.KeyAlphaTestOn, false);
        //             SetKeyword(material, MToonUtils.KeyAlphaBlendOn, true);
        //             SetKeyword(material, MToonUtils.KeyAlphaPremultiplyOn, false);
        //             break;
        //         case RenderMode.TransparentWithZWrite:
        //             material.SetOverrideTag(MToonUtils.TagRenderTypeKey, MToonUtils.TagRenderTypeValueTransparent);
        //             material.SetInt(MToonUtils.PropSrcBlend, BlendMode.SrcAlpha);
        //             material.SetInt(MToonUtils.PropDstBlend, BlendMode.OneMinusSrcAlpha);
        //             material.SetInt(MToonUtils.PropZWrite, MToonUtils.EnabledIntValue);
        //             material.SetInt(MToonUtils.PropAlphaToMask, MToonUtils.DisabledIntValue);
        //             SetKeyword(material, MToonUtils.KeyAlphaTestOn, false);
        //             SetKeyword(material, MToonUtils.KeyAlphaBlendOn, true);
        //             SetKeyword(material, MToonUtils.KeyAlphaPremultiplyOn, false);
        //             break;
        //     }

        //     if (isChangedByUser)
        //     {
        //         // ValidateRenderQueue(material, offset: 0);
        //     }
        //     else
        //     {
        //         var requirement = MToonUtils.GetRenderQueueRequirement(renderMode);
        //         // ValidateRenderQueue(material, offset: material.renderQueue - requirement.DefaultValue);
        //     }
        // }

        // private static void ValidateRenderQueue(Material material, int offset)
        // {
        //     var requirement = MToonUtils.GetRenderQueueRequirement(GetBlendMode(material));
        //     var value = Mathf.Clamp(requirement.DefaultValue + offset, requirement.MinValue, requirement.MaxValue);
        //     material.renderQueue = value;
        // }

        // private static void ValidateOutlineMode(Material material, OutlineWidthMode outlineWidthMode,
        //     OutlineColorMode outlineColorMode)
        // {
        //     var isFixed = outlineColorMode == OutlineColorMode.FixedColor;
        //     var isMixed = outlineColorMode == OutlineColorMode.MixedLighting;

        //     switch (outlineWidthMode)
        //     {
        //         case OutlineWidthMode.None:
        //             SetKeyword(material, MToonUtils.KeyOutlineWidthWorld, false);
        //             SetKeyword(material, MToonUtils.KeyOutlineWidthScreen, false);
        //             SetKeyword(material, MToonUtils.KeyOutlineColorFixed, false);
        //             SetKeyword(material, MToonUtils.KeyOutlineColorMixed, false);
        //             break;
        //         case OutlineWidthMode.WorldCoordinates:
        //             SetKeyword(material, MToonUtils.KeyOutlineWidthWorld, true);
        //             SetKeyword(material, MToonUtils.KeyOutlineWidthScreen, false);
        //             SetKeyword(material, MToonUtils.KeyOutlineColorFixed, isFixed);
        //             SetKeyword(material, MToonUtils.KeyOutlineColorMixed, isMixed);
        //             break;
        //         case OutlineWidthMode.ScreenCoordinates:
        //             SetKeyword(material, MToonUtils.KeyOutlineWidthWorld, false);
        //             SetKeyword(material, MToonUtils.KeyOutlineWidthScreen, true);
        //             SetKeyword(material, MToonUtils.KeyOutlineColorFixed, isFixed);
        //             SetKeyword(material, MToonUtils.KeyOutlineColorMixed, isMixed);
        //             break;
        //     }
        // }

        // private static void ValidateNormalMode(Material material, bool requireNormalMapping)
        // {
        //     SetKeyword(material, MToonUtils.KeyNormalMap, requireNormalMapping);
        // }

        // private static void ValidateCullMode(Material material, CullMode cullMode)
        // {
        //     switch (cullMode)
        //     {
        //         case CullMode.Back:
        //             material.SetInt(MToonUtils.PropCullMode, CullMode.Back);
        //             material.SetInt(MToonUtils.PropOutlineCullMode, CullMode.Front);
        //             break;
        //         case CullMode.Front:
        //             material.SetInt(MToonUtils.PropCullMode, CullMode.Front);
        //             material.SetInt(MToonUtils.PropOutlineCullMode, CullMode.Back);
        //             break;
        //         case CullMode.Off:
        //             material.SetInt(MToonUtils.PropCullMode, CullMode.Off);
        //             material.SetInt(MToonUtils.PropOutlineCullMode, CullMode.Front);
        //             break;
        //     }
        // }
    }
}