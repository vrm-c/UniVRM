using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;
using VRMShaders;
using VRMShaders.VRM10.MToon10.Runtime;
using ColorSpace = VRMShaders.ColorSpace;
using OutlineWidthMode = UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode;

namespace UniVRM10
{
    /// <summary>
    /// Convert MToon parameters from glTF specification to Unity implementation.
    /// </summary>
    public static class BuiltInVrm10MToonMaterialImporter
    {
        /// <summary>
        /// VMRC_materials_mtoon の場合にマテリアル生成情報を作成する
        /// </summary>
        public static bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            var m = data.GLTF.materials[i];
            if (!UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions,
                out UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon mtoon))
            {
                // Fallback to glTF, when MToon extension does not exist.
                matDesc = default;
                return false;
            }

            // use material.name, because material name may renamed in GltfParser.
            matDesc = new MaterialDescriptor(
                m.name,
                Shader.Find(MToon10Meta.UnityShaderName),
                null,
                Vrm10MToonTextureImporter.EnumerateAllTextures(data, m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.Item2.Item2),
                TryGetAllFloats(m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.value),
                TryGetAllColors(m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.value),
                TryGetAllFloatArrays(m, mtoon).ToDictionary(tuple => tuple.key, tuple => tuple.value),
                new Action<Material>[]
                {
                    material =>
                    {
                        // Set hidden properties, keywords from float properties.
                        new MToonValidator(material).Validate();
                    }
                });

            return true;
        }

        public static IEnumerable<(string key, Color value)> TryGetAllColors(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            const ColorSpace gltfColorSpace = ColorSpace.Linear;

            // Rendering
            var baseColor = material?.pbrMetallicRoughness?.baseColorFactor?.ToColor4(gltfColorSpace, ColorSpace.sRGB);
            if (baseColor.HasValue)
            {
                yield return (MToon10Prop.BaseColorFactor.ToUnityShaderLabName(), baseColor.Value);
            }

            // Lighting
            var shadeColor = mToon?.ShadeColorFactor?.ToColor3(gltfColorSpace, ColorSpace.sRGB);
            if (shadeColor.HasValue)
            {
                yield return (MToon10Prop.ShadeColorFactor.ToUnityShaderLabName(), shadeColor.Value);
            }

            // GI

            // Emission
            // Emissive factor should be stored in Linear space
            var emissionColor = material?.emissiveFactor?.ToColor3(gltfColorSpace, ColorSpace.Linear);
            if (emissionColor.HasValue)
            {
                yield return (MToon10Prop.EmissiveFactor.ToUnityShaderLabName(), emissionColor.Value);
            }

            // Matcap
            var matcapColor = mToon?.MatcapFactor?.ToColor3(gltfColorSpace, ColorSpace.sRGB);
            if (matcapColor.HasValue)
            {
                yield return (MToon10Prop.MatcapColorFactor.ToUnityShaderLabName(), matcapColor.Value);
            }

            // Rim Lighting
            var rimColor = mToon?.ParametricRimColorFactor?.ToColor3(gltfColorSpace, ColorSpace.sRGB);
            if (rimColor.HasValue)
            {
                yield return (MToon10Prop.ParametricRimColorFactor.ToUnityShaderLabName(), rimColor.Value);
            }

            // Outline
            var outlineColor = mToon?.OutlineColorFactor?.ToColor3(gltfColorSpace, ColorSpace.sRGB);
            if (outlineColor.HasValue)
            {
                yield return (MToon10Prop.OutlineColorFactor.ToUnityShaderLabName(), outlineColor.Value);
            }

            // UV Animation
        }

        public static IEnumerable<(string key, float value)> TryGetAllFloats(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            // Rendering
            var alphaMode = GetMToon10AlphaMode(material);
            {
                yield return (MToon10Prop.AlphaMode.ToUnityShaderLabName(), (float)alphaMode);
            }

            var transparentWithZWrite = GetMToon10TransparentWithZWriteMode(material, mToon);
            {
                yield return (MToon10Prop.TransparentWithZWrite.ToUnityShaderLabName(), (float)transparentWithZWrite);
            }

            var cutoff = material?.alphaCutoff;
            if (cutoff.HasValue)
            {
                yield return (MToon10Prop.AlphaCutoff.ToUnityShaderLabName(), cutoff.Value);
            }

            var renderQueueOffset = mToon?.RenderQueueOffsetNumber;
            if (renderQueueOffset.HasValue)
            {
                yield return (MToon10Prop.RenderQueueOffsetNumber.ToUnityShaderLabName(), (float)renderQueueOffset);
            }

            var doubleSidedMode = GetMToon10DoubleSidedMode(material, mToon);
            {
                yield return (MToon10Prop.DoubleSided.ToUnityShaderLabName(), (float)doubleSidedMode);
            }

            // Lighting
            var normalScale = material?.normalTexture?.scale;
            if (normalScale.HasValue)
            {
                yield return (MToon10Prop.NormalTextureScale.ToUnityShaderLabName(), normalScale.Value);
            }

            var shadingShift = mToon?.ShadingShiftFactor;
            if (shadingShift.HasValue)
            {
                yield return (MToon10Prop.ShadingShiftFactor.ToUnityShaderLabName(), shadingShift.Value);
            }

            var shadingShiftTextureScale = mToon?.ShadingShiftTexture?.Scale;
            if (shadingShiftTextureScale.HasValue)
            {
                yield return (MToon10Prop.ShadingShiftTextureScale.ToUnityShaderLabName(), shadingShiftTextureScale.Value);
            }

            var shadingToony = mToon?.ShadingToonyFactor;
            if (shadingToony.HasValue)
            {
                yield return (MToon10Prop.ShadingToonyFactor.ToUnityShaderLabName(), shadingToony.Value);
            }

            // GI
            var giEqualization = mToon?.GiEqualizationFactor;
            if (giEqualization.HasValue)
            {
                yield return (MToon10Prop.GiEqualizationFactor.ToUnityShaderLabName(), giEqualization.Value);
            }

            // Emission

            // Rim Lighting
            var rimFresnelPower = mToon?.ParametricRimFresnelPowerFactor;
            if (rimFresnelPower.HasValue)
            {
                yield return (MToon10Prop.ParametricRimFresnelPowerFactor.ToUnityShaderLabName(), rimFresnelPower.Value);
            }

            var rimLift = mToon?.ParametricRimLiftFactor;
            if (rimLift.HasValue)
            {
                yield return (MToon10Prop.ParametricRimLiftFactor.ToUnityShaderLabName(), rimLift.Value);
            }

            var rimLightMix = mToon?.RimLightingMixFactor;
            if (rimLightMix.HasValue)
            {
                yield return (MToon10Prop.RimLightingMixFactor.ToUnityShaderLabName(), rimLightMix.Value);
            }

            // Outline
            var outlineMode = GetMToon10OutlineWidthMode(material, mToon);
            {
                yield return (MToon10Prop.OutlineWidthMode.ToUnityShaderLabName(), (float)outlineMode);
            }

            var outlineWidth = mToon?.OutlineWidthFactor;
            if (outlineWidth.HasValue)
            {
                yield return (MToon10Prop.OutlineWidthFactor.ToUnityShaderLabName(), outlineWidth.Value);
            }

            var outlineLightMix = mToon?.OutlineLightingMixFactor;
            if (outlineLightMix.HasValue)
            {
                yield return (MToon10Prop.OutlineLightingMixFactor.ToUnityShaderLabName(), outlineLightMix.Value);
            }

            // UV Animation
            var uvAnimSpeedScrollX = mToon?.UvAnimationScrollXSpeedFactor;
            if (uvAnimSpeedScrollX.HasValue)
            {
                yield return (MToon10Prop.UvAnimationScrollXSpeedFactor.ToUnityShaderLabName(), uvAnimSpeedScrollX.Value);
            }

            var uvAnimSpeedScrollY = mToon?.UvAnimationScrollYSpeedFactor;
            if (uvAnimSpeedScrollY.HasValue)
            {
                // UV coords conversion.
                // glTF (top-left origin) to Unity (bottom-left origin)
                const float invertY = -1f;

                yield return (MToon10Prop.UvAnimationScrollYSpeedFactor.ToUnityShaderLabName(), uvAnimSpeedScrollY.Value * invertY);
            }

            var uvAnimSpeedRotation = mToon?.UvAnimationRotationSpeedFactor;
            if (uvAnimSpeedRotation.HasValue)
            {
                // Speed unit Conversion
                const float radianPerSecToRotationPerSec = 1f / (Mathf.PI * 2f);
                yield return (MToon10Prop.UvAnimationRotationSpeedFactor.ToUnityShaderLabName(), uvAnimSpeedRotation.Value * radianPerSecToRotationPerSec);
            }

            // UI
            if (true /* TODO: mToon.IsAdvancedMode */)
            {
                yield return (MToon10Prop.EditorEditMode.ToUnityShaderLabName(), 1);
            }
        }

        public static IEnumerable<(string key, Vector4 value)> TryGetAllFloatArrays(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            yield break;
        }

        private static MToon10AlphaMode GetMToon10AlphaMode(glTFMaterial material)
        {
            switch (material?.alphaMode)
            {
                case "OPAQUE":
                    return MToon10AlphaMode.Opaque;
                case "MASK":
                    return MToon10AlphaMode.Cutout;
                case "BLEND":
                    return MToon10AlphaMode.Transparent;
                default:
                    Debug.LogWarning($"Invalid AlphaMode");
                    return MToon10AlphaMode.Opaque;
            }
        }

        private static MToon10TransparentWithZWriteMode GetMToon10TransparentWithZWriteMode(glTFMaterial material, VRMC_materials_mtoon mtoon)
        {
            if (mtoon?.TransparentWithZWrite == true)
            {
                return MToon10TransparentWithZWriteMode.On;
            }
            else
            {
                return MToon10TransparentWithZWriteMode.Off;
            }
        }
        private static MToon10DoubleSidedMode GetMToon10DoubleSidedMode(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            if (material?.doubleSided == true)
            {
                return MToon10DoubleSidedMode.On;
            }
            else
            {
                return MToon10DoubleSidedMode.Off;
            }
        }

        private static MToon10OutlineMode GetMToon10OutlineWidthMode(glTFMaterial material, VRMC_materials_mtoon mToon)
        {
            switch (mToon?.OutlineWidthMode)
            {
                case OutlineWidthMode.none:
                    return MToon10OutlineMode.None;
                case OutlineWidthMode.worldCoordinates:
                    return MToon10OutlineMode.World;
                case OutlineWidthMode.screenCoordinates:
                    return MToon10OutlineMode.Screen;
                default:
                    // Invalid
                    Debug.LogWarning("Invalid outlineWidthMode");
                    return MToon10OutlineMode.None;
            }
        }
    }
}
