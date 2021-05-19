using System;
using System.Collections.Generic;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;
using ColorSpace = UniGLTF.ColorSpace;
using OutlineWidthMode = UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode;

namespace UniVRM10
{
    /// <summary>
    /// Convert MToon parameters from glTF specification to Unity implementation.
    /// </summary>
    public static class Vrm10MToonMaterialParameterImporter
    {
        public static IEnumerable<(string key, Color value)> TryGetAllColors(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            const ColorSpace gltfColorSpace = ColorSpace.Linear;
            
            var baseColor = material?.pbrMetallicRoughness?.baseColorFactor?.ToColor4(gltfColorSpace, ColorSpace.sRGB);
            if (baseColor.HasValue)
            {
                yield return (MToon.Utils.PropColor, baseColor.Value);
            }

            var emissionColor = material?.emissiveFactor?.ToColor3(gltfColorSpace, ColorSpace.Linear);
            if (emissionColor.HasValue)
            {
                yield return (MToon.Utils.PropEmissionColor, emissionColor.Value);
            }

            var shadeColor = mToon?.ShadeColorFactor?.ToColor3(gltfColorSpace, ColorSpace.sRGB);
            if (shadeColor.HasValue)
            {
                yield return (MToon.Utils.PropShadeColor, shadeColor.Value);
            }

            var rimColor = mToon?.ParametricRimColorFactor?.ToColor3(gltfColorSpace, ColorSpace.Linear);
            if (rimColor.HasValue)
            {
                yield return (MToon.Utils.PropRimColor, rimColor.Value);
            }

            var outlineColor = mToon?.OutlineColorFactor?.ToColor3(gltfColorSpace, ColorSpace.sRGB);
            if (outlineColor.HasValue)
            {
                yield return (MToon.Utils.PropOutlineColor, outlineColor.Value);
            }
        }

        public static IEnumerable<(string key, float value)> TryGetAllFloats(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            var renderMode = GetMToonRenderMode(material, mToon);
            {
                yield return (MToon.Utils.PropBlendMode, (float) renderMode);
            }

            var cullMode = GetMToonCullMode(material, mToon);
            {
                yield return (MToon.Utils.PropCullMode, (float) cullMode);
            }

            var outlineMode = GetMToonOutlineWidthMode(material, mToon);
            {
                yield return (MToon.Utils.PropOutlineWidthMode, (float) outlineMode);
                
                // In case of VRM 1.0 MToon, outline color mode is always MixedLighting.
                yield return (MToon.Utils.PropOutlineColorMode, (float) MToon.OutlineColorMode.MixedLighting);
            }
            
            var cutoff = material?.alphaCutoff;
            if (cutoff.HasValue)
            {
                yield return (MToon.Utils.PropCutoff, cutoff.Value);
            }
            
            var normalScale = material?.normalTexture?.scale;
            if (normalScale.HasValue)
            {
                yield return ("_BumpScale", normalScale.Value);
            }
            
            var shadingShift = mToon?.ShadingShiftFactor;
            if (shadingShift.HasValue)
            {
                yield return (MToon.Utils.PropShadeShift, shadingShift.Value);
            }

            var shadingShiftTextureScale = mToon?.ShadingShiftTexture?.Scale;
            if (shadingShiftTextureScale.HasValue)
            {
                Debug.LogWarning("Need VRM 1.0 MToon implementation.");
                yield return ("_NEED_IMPLEMENTATION_MTOON_1_0_shadingShiftTextureScale", shadingShiftTextureScale.Value);
            }

            var shadingToony = mToon?.ShadingToonyFactor;
            if (shadingToony.HasValue)
            {
                yield return (MToon.Utils.PropShadeToony, shadingToony.Value);
            }

            var giIntensity = mToon?.GiIntensityFactor;
            if (giIntensity.HasValue)
            {
                yield return (MToon.Utils.PropIndirectLightIntensity, giIntensity.Value);
            }

            var rimLightMix = mToon?.RimLightingMixFactor;
            if (rimLightMix.HasValue)
            {
                yield return (MToon.Utils.PropRimLightingMix, rimLightMix.Value);
            }

            var rimFresnelPower = mToon?.ParametricRimFresnelPowerFactor;
            if (rimFresnelPower.HasValue)
            {
                yield return (MToon.Utils.PropRimFresnelPower, rimFresnelPower.Value);
            }

            var rimLift = mToon?.ParametricRimLiftFactor;
            if (rimLift.HasValue)
            {
                yield return (MToon.Utils.PropRimLift, rimLift.Value);
            }

            // Unit conversion.
            // Because Unity implemented MToon uses centimeter unit in width parameter.
            var outlineWidth = mToon?.OutlineWidthFactor;
            if (outlineWidth.HasValue)
            {
                const float meterToCentimeter = 100f;

                yield return (MToon.Utils.PropOutlineWidth, outlineWidth.Value * meterToCentimeter);
            }

            var outlineLightMix = mToon?.OutlineLightingMixFactor;
            if (outlineLightMix.HasValue)
            {
                yield return (MToon.Utils.PropOutlineLightingMix, outlineLightMix.Value);
            }

            var uvAnimSpeedScrollX = mToon?.UvAnimationScrollXSpeedFactor;
            if (uvAnimSpeedScrollX.HasValue)
            {
                yield return (MToon.Utils.PropUvAnimScrollX, uvAnimSpeedScrollX.Value);
            }
            
            // UV coords conversion.
            // glTF (top-left origin) to Unity (bottom-left origin)
            var uvAnimSpeedScrollY = mToon?.UvAnimationScrollYSpeedFactor;
            if (uvAnimSpeedScrollY.HasValue)
            {
                const float invertY = -1f;
                
                yield return (MToon.Utils.PropUvAnimScrollY, uvAnimSpeedScrollY.Value * invertY);
            }

            var uvAnimSpeedRotation = mToon?.UvAnimationRotationSpeedFactor;
            if (uvAnimSpeedRotation.HasValue)
            {
                yield return (MToon.Utils.PropUvAnimRotation, uvAnimSpeedRotation.Value);
            }
        }

        public static IEnumerable<(string key, Vector4 value)> TryGetAllFloatArrays(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            yield break;
        }

        public static int? TryGetRenderQueue(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            var renderQueueOffset = mToon?.RenderQueueOffsetNumber;
            if (renderQueueOffset.HasValue)
            {
                var renderMode = GetMToonRenderMode(material, mToon);
                return MToon.Utils.GetRenderQueueRequirement(renderMode).DefaultValue +
                                  renderQueueOffset.Value;
            }
            return default;
        }


        private static MToon.RenderMode GetMToonRenderMode(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            switch (material?.alphaMode)
            {
                case "OPAQUE":
                    return MToon.RenderMode.Opaque;
                case "MASK":
                    return MToon.RenderMode.Cutout;
                case "BLEND":
                    var mToonZWrite = mToon?.TransparentWithZWrite;
                    if (mToonZWrite.HasValue)
                    {
                        if (mToonZWrite.Value)
                        {
                            return MToon.RenderMode.TransparentWithZWrite;
                        }
                        else
                        {
                            return MToon.RenderMode.Transparent;
                        }
                    }
                    else
                    {
                        // Invalid
                        return MToon.RenderMode.Transparent;
                    }
                default:
                    // Invalid
                    return MToon.RenderMode.Opaque;
            }
        }

        private static MToon.CullMode GetMToonCullMode(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            var doubleSided = material?.doubleSided;
            if (doubleSided.HasValue && doubleSided.Value)
            {
                return MToon.CullMode.Off;
            }
            else
            {
                return MToon.CullMode.Back;
            }
        }

        private static MToon.OutlineWidthMode GetMToonOutlineWidthMode(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            switch (mToon.OutlineWidthMode)
            {
                case OutlineWidthMode.none:
                    return MToon.OutlineWidthMode.None;
                case OutlineWidthMode.worldCoordinates:
                    return MToon.OutlineWidthMode.WorldCoordinates;
                case OutlineWidthMode.screenCoordinates:
                    return MToon.OutlineWidthMode.ScreenCoordinates;
                default:
                    // Invalid
                    return MToon.OutlineWidthMode.None;
            }
        }
    }
}