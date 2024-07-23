using System;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;
using VRM10.MToon10;
using ColorSpace = UniGLTF.ColorSpace;

namespace UniVRM10
{
    public static class BuiltInVrm10MToonMaterialExporter
    {
        public static bool TryExportMaterialAsMToon(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            if (src.shader.name != MToon10Meta.UnityShaderName)
            {
                dst = null;
                return false;
            }

            // Get MToon10 Context
            var context = new MToon10Context(src);
            context.Validate();

            // base material
            dst = glTF_KHR_materials_unlit.CreateDefault();
            dst.name = src.name;

            // vrmc_materials_mtoon ext
            var mtoon = new UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon();
            mtoon.SpecVersion = Vrm10Exporter.MTOON_SPEC_VERSION;

            // Rendering
            dst.alphaMode = ExportAlphaMode(context.AlphaMode);
            mtoon.TransparentWithZWrite = context.TransparentWithZWriteMode == MToon10TransparentWithZWriteMode.On;
            dst.alphaCutoff = Mathf.Max(0, context.AlphaCutoff);
            mtoon.RenderQueueOffsetNumber = context.RenderQueueOffsetNumber;
            dst.doubleSided = context.DoubleSidedMode == MToon10DoubleSidedMode.On;

            // Lighting
            dst.pbrMetallicRoughness = new glTFPbrMetallicRoughness();
            dst.pbrMetallicRoughness.baseColorFactor = context.BaseColorFactorSrgb.ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
            var baseColorTextureIndex = textureExporter.RegisterExportingAsSRgb(context.BaseColorTexture, context.AlphaMode != MToon10AlphaMode.Opaque);
            if (baseColorTextureIndex != -1)
            {
                dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                {
                    index = baseColorTextureIndex,
                };
            }
            mtoon.ShadeColorFactor = context.ShadeColorFactorSrgb.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear);
            var shadeColorTextureIndex = textureExporter.RegisterExportingAsSRgb(context.ShadeColorTexture, needsAlpha: false);
            if (shadeColorTextureIndex != -1)
            {
                mtoon.ShadeMultiplyTexture = new TextureInfo
                {
                    Index = shadeColorTextureIndex,
                };
            }
            var normalTextureIndex = textureExporter.RegisterExportingAsNormal(context.NormalTexture);
            if (normalTextureIndex != -1)
            {
                dst.normalTexture = new glTFMaterialNormalTextureInfo
                {
                    index = normalTextureIndex,
                    scale = context.NormalTextureScale,
                };
            }
            mtoon.ShadingShiftFactor = context.ShadingShiftFactor;
            var shadingShiftTextureIndex = textureExporter.RegisterExportingAsLinear(context.ShadingShiftTexture, needsAlpha: false);
            if (shadingShiftTextureIndex != -1)
            {
                mtoon.ShadingShiftTexture = new ShadingShiftTextureInfo
                {
                    Index = shadingShiftTextureIndex,
                    Scale = context.ShadingShiftTextureScale,
                };
            }
            mtoon.ShadingToonyFactor = context.ShadingToonyFactor;

            // GI
            mtoon.GiEqualizationFactor = context.GiEqualizationFactor;

            // Emission
            // Emissive factor is stored in Linear space
            var emissiveFactor = context.EmissiveFactorLinear;
            var maxColorComponent = emissiveFactor.maxColorComponent;
            if (maxColorComponent > 1.0f)
            {
                UniGLTF.glTF_KHR_materials_emissive_strength.Serialize(ref dst.extensions, maxColorComponent);
                emissiveFactor /= maxColorComponent;
            }
            dst.emissiveFactor = emissiveFactor.ToFloat3(ColorSpace.Linear, ColorSpace.Linear);
            var emissiveTextureIndex = textureExporter.RegisterExportingAsSRgb(context.EmissiveTexture, needsAlpha: false);
            if (emissiveTextureIndex != -1)
            {
                dst.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                {
                    index = emissiveTextureIndex,
                };
            }

            // Rim Lighting
            mtoon.MatcapFactor = context.MatcapColorFactorSrgb.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear);
            var matcapTextureIndex = textureExporter.RegisterExportingAsSRgb(context.MatcapTexture, needsAlpha: false);
            if (matcapTextureIndex != -1)
            {
                mtoon.MatcapTexture = new TextureInfo
                {
                    Index = matcapTextureIndex,
                };
            }

            mtoon.ParametricRimColorFactor = context.ParametricRimColorFactorSrgb.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear);
            mtoon.ParametricRimFresnelPowerFactor = context.ParametricRimFresnelPowerFactor;
            mtoon.ParametricRimLiftFactor = context.ParametricRimLiftFactor;
            var rimMultiplyTextureIndex = textureExporter.RegisterExportingAsSRgb(context.RimMultiplyTexture, needsAlpha: false);
            if (rimMultiplyTextureIndex != -1)
            {
                mtoon.RimMultiplyTexture = new TextureInfo
                {
                    Index = rimMultiplyTextureIndex,
                };
            }
            mtoon.RimLightingMixFactor = context.RimLightingMixFactor;

            // Outline
            mtoon.OutlineWidthMode = ExportOutlineWidthMode(context.OutlineWidthMode);
            mtoon.OutlineWidthFactor = context.OutlineWidthFactor;
            var outlineWidthMultiplyTextureIndex = textureExporter.RegisterExportingAsLinear(context.OutlineWidthMultiplyTexture, needsAlpha: false);
            if (outlineWidthMultiplyTextureIndex != -1)
            {
                mtoon.OutlineWidthMultiplyTexture = new TextureInfo
                {
                    Index = outlineWidthMultiplyTextureIndex,
                };
            }
            mtoon.OutlineColorFactor = context.OutlineColorFactorSrgb.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear);
            mtoon.OutlineLightingMixFactor = context.OutlineLightingMixFactor;

            // UV Animation
            var uvAnimationMaskTextureIndex = textureExporter.RegisterExportingAsLinear(context.UvAnimationMaskTexture, needsAlpha: false);
            if (uvAnimationMaskTextureIndex != -1)
            {
                mtoon.UvAnimationMaskTexture = new TextureInfo
                {
                    Index = uvAnimationMaskTextureIndex,
                };
            }
            mtoon.UvAnimationScrollXSpeedFactor = context.UvAnimationScrollXSpeedFactor;
            {
                // Coordinate Conversion
                const float invertY = -1f;
                mtoon.UvAnimationScrollYSpeedFactor = context.UvAnimationScrollYSpeedFactor * invertY;
            }
            // Speed unit Conversion
            const float rotationPerSecToRadianPerSec = Mathf.PI * 2f;
            mtoon.UvAnimationRotationSpeedFactor = context.UvAnimationRotationSpeedFactor * rotationPerSecToRadianPerSec;

            // Texture Transforms
            var scale = context.TextureScale;
            var offset = context.TextureOffset;
            Vrm10MaterialExportUtils.ExportTextureTransform(dst.pbrMetallicRoughness.baseColorTexture, scale, offset);
            Vrm10MaterialExportUtils.ExportTextureTransform(dst.emissiveTexture, scale, offset);
            Vrm10MaterialExportUtils.ExportTextureTransform(dst.normalTexture, scale, offset);
            Vrm10MaterialExportUtils.ExportTextureTransform(mtoon.ShadeMultiplyTexture, scale, offset);
            Vrm10MaterialExportUtils.ExportTextureTransform(mtoon.ShadingShiftTexture, scale, offset);
            Vrm10MaterialExportUtils.ExportTextureTransform(mtoon.MatcapTexture, scale, offset);
            Vrm10MaterialExportUtils.ExportTextureTransform(mtoon.RimMultiplyTexture, scale, offset);
            Vrm10MaterialExportUtils.ExportTextureTransform(mtoon.OutlineWidthMultiplyTexture, scale, offset);
            Vrm10MaterialExportUtils.ExportTextureTransform(mtoon.UvAnimationMaskTexture, scale, offset);

            UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref dst.extensions, mtoon);

            return true;
        }

        private static string ExportAlphaMode(MToon10AlphaMode alphaMode)
        {
            switch (alphaMode)
            {
                case MToon10AlphaMode.Opaque:
                    return "OPAQUE";
                case MToon10AlphaMode.Cutout:
                    return "MASK";
                case MToon10AlphaMode.Transparent:
                    return "BLEND";
                default:
                    throw new ArgumentOutOfRangeException(nameof(alphaMode), alphaMode, null);
            }
        }

        private static OutlineWidthMode ExportOutlineWidthMode(MToon10OutlineMode mode)
        {
            switch (mode)
            {
                case MToon10OutlineMode.None:
                    return OutlineWidthMode.none;
                case MToon10OutlineMode.World:
                    return OutlineWidthMode.worldCoordinates;
                case MToon10OutlineMode.Screen:
                    return OutlineWidthMode.screenCoordinates;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
