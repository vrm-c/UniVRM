using System;
using MToon;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;
using VRMShaders;
using ColorSpace = VRMShaders.ColorSpace;
using OutlineWidthMode = MToon.OutlineWidthMode;
using RenderMode = MToon.RenderMode;

namespace UniVRM10
{
    public static class Vrm10MToonMaterialExporter
    {
        public static bool TryExportMaterialAsMToon(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            try
            {
                if (src.shader.name != MToon.Utils.ShaderName)
                {
                    dst = null;
                    return false;
                }

                // convert MToon intermediate value from UnityEngine.Material
                var def = MToon.Utils.GetMToonParametersFromMaterial(src);

                // base material
                dst = glTF_KHR_materials_unlit.CreateDefault();
                dst.name = src.name;

                // Rendering
                dst.doubleSided = def.Rendering.CullMode == CullMode.Off;
                dst.alphaMode = ExportAlphaMode(def.Rendering.RenderMode);
                dst.alphaCutoff = Mathf.Max(def.Color.CutoutThresholdValue, 0);

                // Lighting
                dst.pbrMetallicRoughness = new glTFPbrMetallicRoughness
                {
                    baseColorFactor = def.Color.LitColor.ToFloat4(ColorSpace.sRGB, ColorSpace.Linear),
                    baseColorTexture = new glTFMaterialBaseColorTextureInfo
                    {
                        index = textureExporter.ExportAsSRgb(def.Color.LitMultiplyTexture),
                    },
                };

                // Normal
                var normalTextureIndex = textureExporter.ExportAsNormal(def.Lighting.Normal.NormalTexture);
                if (normalTextureIndex != -1)
                {
                    dst.normalTexture = new glTFMaterialNormalTextureInfo
                    {
                        index = normalTextureIndex,
                        scale = def.Lighting.Normal.NormalScaleValue,
                    };
                }

                // Emission
                dst.emissiveFactor = def.Emission.EmissionColor.ToFloat3(ColorSpace.Linear, ColorSpace.Linear);
                var emissiveTextureIndex = textureExporter.ExportAsSRgb(def.Emission.EmissionMultiplyTexture);
                if (emissiveTextureIndex != -1)
                {
                    dst.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                    {
                        index = emissiveTextureIndex,
                    };
                }

                const float centimeterToMeter = 0.01f;
                const float invertY = -1f;

                // VRMC_materials_mtoon
                var mtoon = new UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon
                {
                    Version = "",

                    // Rendering
                    TransparentWithZWrite = def.Rendering.RenderMode == RenderMode.TransparentWithZWrite,
                    RenderQueueOffsetNumber = ExportRenderQueueOffset(def.Rendering.RenderMode, def.Rendering.RenderQueueOffsetNumber),

                    // Lighting
                    ShadeColorFactor = def.Color.ShadeColor.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear),
                    ShadeMultiplyTexture = new TextureInfo
                    {
                        Index = textureExporter.ExportAsSRgb(def.Color.ShadeMultiplyTexture),
                    },
                    ShadingToonyFactor = def.Lighting.LitAndShadeMixing.ShadingToonyValue,
                    ShadingShiftFactor = def.Lighting.LitAndShadeMixing.ShadingShiftValue,
                    ShadingShiftTexture = null,

                    // Global Illumination
                    GiIntensityFactor = def.Lighting.LightingInfluence.GiIntensityValue,

                    // Rim Lighting
                    MatcapTexture = new TextureInfo
                    {
                        Index = textureExporter.ExportAsSRgb(def.MatCap.AdditiveTexture),
                    },
                    ParametricRimColorFactor = def.Rim.RimColor.ToFloat3(ColorSpace.Linear, ColorSpace.Linear),
                    ParametricRimFresnelPowerFactor = def.Rim.RimFresnelPowerValue,
                    ParametricRimLiftFactor = def.Rim.RimLiftValue,
                    RimMultiplyTexture = new TextureInfo
                    {
                        Index = textureExporter.ExportAsSRgb(def.Rim.RimMultiplyTexture),
                    },
                    RimLightingMixFactor = def.Rim.RimLightingMixValue,

                    // Outline
                    OutlineWidthMode = ExportOutlineWidthMode(def.Outline.OutlineWidthMode),
                    OutlineWidthFactor = def.Outline.OutlineWidthValue * centimeterToMeter,
                    OutlineWidthMultiplyTexture = new TextureInfo
                    {
                        Index = textureExporter.ExportAsLinear(def.Outline.OutlineWidthMultiplyTexture),
                    },
                    OutlineColorFactor = def.Outline.OutlineColor.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear),
                    OutlineLightingMixFactor = ExportOutlineLightingMixFactor(def.Outline.OutlineColorMode, def.Outline.OutlineLightingMixValue),

                    // UV Anim
                    UvAnimationMaskTexture = new TextureInfo
                    {
                        Index = textureExporter.ExportAsLinear(def.TextureOption.UvAnimationMaskTexture),
                    },
                    UvAnimationScrollXSpeedFactor = def.TextureOption.UvAnimationScrollXSpeedValue,
                    UvAnimationScrollYSpeedFactor = def.TextureOption.UvAnimationScrollYSpeedValue * invertY,
                    UvAnimationRotationSpeedFactor = def.TextureOption.UvAnimationRotationSpeedValue,
                };

                // Texture Transforms
                var scale = def.TextureOption.MainTextureLeftBottomOriginScale;
                var offset = def.TextureOption.MainTextureLeftBottomOriginOffset;
                ExportTextureTransform(dst.pbrMetallicRoughness.baseColorTexture, scale, offset);
                ExportTextureTransform(dst.emissiveTexture, scale, offset);
                ExportTextureTransform(dst.normalTexture, scale, offset);
                ExportTextureTransform(mtoon.ShadeMultiplyTexture, scale, offset);
                ExportTextureTransform(mtoon.MatcapTexture, scale, offset);
                ExportTextureTransform(mtoon.RimMultiplyTexture, scale, offset);
                ExportTextureTransform(mtoon.OutlineWidthMultiplyTexture, scale, offset);
                ExportTextureTransform(mtoon.UvAnimationMaskTexture, scale, offset);

                UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref dst.extensions, mtoon);

                return true;
            }
            catch (Exception)
            {
                dst = null;
                return false;
            }
        }

        private static string ExportAlphaMode(MToon.RenderMode renderMode)
        {
            switch (renderMode)
            {
                case RenderMode.Opaque:
                    return "OPAQUE";
                case RenderMode.Cutout:
                    return "MASK";
                case RenderMode.Transparent:
                    return "BLEND";
                case RenderMode.TransparentWithZWrite:
                    return "BLEND";
                default:
                    throw new ArgumentOutOfRangeException(nameof(renderMode), renderMode, null);
            }
        }

        private static int ExportRenderQueueOffset(MToon.RenderMode renderMode, int offset)
        {
            switch (renderMode)
            {
                case RenderMode.Opaque:
                    return 0;
                case RenderMode.Cutout:
                    return 0;
                case RenderMode.Transparent:
                    return Mathf.Clamp(offset, -9, 0);
                case RenderMode.TransparentWithZWrite:
                    return Mathf.Clamp(offset, 0, +9);
                default:
                    throw new ArgumentOutOfRangeException(nameof(renderMode), renderMode, null);
            }
        }

        private static UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode ExportOutlineWidthMode(
            MToon.OutlineWidthMode mode)
        {
            switch (mode)
            {
                case OutlineWidthMode.None:
                    return UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode.none;
                case OutlineWidthMode.WorldCoordinates:
                    return UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode.worldCoordinates;
                case OutlineWidthMode.ScreenCoordinates:
                    return UniGLTF.Extensions.VRMC_materials_mtoon.OutlineWidthMode.screenCoordinates;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private static float ExportOutlineLightingMixFactor(OutlineColorMode mode, float mixValue)
        {
            switch (mode)
            {
                case OutlineColorMode.FixedColor:
                    return 0;
                case OutlineColorMode.MixedLighting:
                    return mixValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }


        private static void ExportTextureTransform(glTFTextureInfo textureInfo, Vector2 unityScale, Vector2 unityOffset)
        {
            if (textureInfo == null)
            {
                return;
            }
            var scale = unityScale;
            var offset = new Vector2(unityOffset.x, 1.0f - unityOffset.y - unityScale.y);

            glTF_KHR_texture_transform.Serialize(textureInfo, (offset.x, offset.y), (scale.x, scale.y));
        }

        private static void ExportTextureTransform(TextureInfo textureInfo, Vector2 unityScale, Vector2 unityOffset)
        {
            // Generate extension to empty holder.
            var gltfTextureInfo = new EmptyGltfTextureInfo();
            ExportTextureTransform(gltfTextureInfo, unityScale, unityOffset);

            // Copy extension from empty holder.
            textureInfo.Extensions = gltfTextureInfo.extensions;
        }

        private sealed class EmptyGltfTextureInfo : glTFTextureInfo
        {

        }
    }
}
