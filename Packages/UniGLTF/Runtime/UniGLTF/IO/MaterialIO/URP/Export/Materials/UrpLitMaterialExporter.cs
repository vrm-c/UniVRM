using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    public class UrpLitMaterialExporter
    {
        public Shader Shader { get; set; }

        /// <summary>
        /// "Universal Render Pipeline/Lit" シェーダのマテリアルをエクスポートする。
        ///
        /// プロパティに互換性がある他のシェーダを指定することもできる。
        /// </summary>
        public UrpLitMaterialExporter(Shader shader = null)
        {
            Shader = shader != null ? shader : Shader.Find("Universal Render Pipeline/Lit");
        }

        public bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            try
            {
                if (src == null) throw new ArgumentNullException(nameof(src));
                if (textureExporter == null) throw new ArgumentNullException(nameof(textureExporter));
                if (src.shader != Shader || Shader == null) throw new UniGLTFShaderNotMatchedInternalException(src.shader);

                dst = new glTFMaterial
                {
                    name = src.name,
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness(),
                };

                var context = new UrpLitContext(src);
                foreach (var validation in EnumerateValidation(context))
                {
                    if (!validation.CanExport)
                    {
                        UniGLTFLogger.Error(validation.Message, src);
                        throw new UniGLTFNotSupportedException(validation.Message);
                    }
                }

                ExportSurfaceSettings(context, dst, textureExporter);
                ExportBaseColor(context, dst, textureExporter);
                ExportMetallicSmoothness(context, dst, textureExporter);
                ExportOcclusion(context, dst, textureExporter);
                ExportNormal(context, dst, textureExporter);
                ExportEmission(context, dst, textureExporter);

                return true;
            }
            catch (UniGLTFShaderNotMatchedInternalException)
            {
                dst = default;
                return false;
            }
            catch (Exception e)
            {
                UniGLTFLogger.Exception(e);
                dst = default;
                return false;
            }
        }

        public static void ExportSurfaceSettings(UrpBaseShaderContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            dst.alphaMode = (context.SurfaceType, context.IsAlphaClipEnabled) switch
            {
                (UrpLitSurfaceType.Opaque, false) => glTFBlendMode.OPAQUE.ToString(),
                (UrpLitSurfaceType.Opaque, true) => glTFBlendMode.MASK.ToString(),
                (UrpLitSurfaceType.Transparent, false) => glTFBlendMode.BLEND.ToString(),
                (UrpLitSurfaceType.Transparent, true) => glTFBlendMode.BLEND.ToString(), // NOTE: not supported in glTF
                _ => throw new ArgumentOutOfRangeException()
            };
            dst.alphaCutoff = context.Cutoff;
            dst.doubleSided = context.CullMode != CullMode.Back; // NOTE: cull front not supported in glTF
        }

        public static void ExportBaseColor(UrpBaseShaderContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            dst.pbrMetallicRoughness.baseColorFactor = context.BaseColorSrgb.ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
            if (context.BaseTexture != null)
            {
                var needsAlpha = context.SurfaceType != UrpLitSurfaceType.Opaque;
                var index = textureExporter.RegisterExportingAsSRgb(context.BaseTexture, needsAlpha);
                if (index >= 0)
                {
                    dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                    {
                        index = index,
                        texCoord = 0,
                    };
                    ExportBaseTexTransform(context, dst.pbrMetallicRoughness.baseColorTexture);
                }
            }
        }

        public static void ExportMetallicSmoothness(UrpLitContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            // NOTE: maybe KHR_materials_specular
            if (context.WorkflowType != UrpLitWorkflowType.Metallic) return;

            // Metallic-Roughness
            dst.pbrMetallicRoughness.metallicRoughnessTexture = null;
            dst.pbrMetallicRoughness.metallicFactor = context.Metallic;
            dst.pbrMetallicRoughness.roughnessFactor = 1.0f - context.Smoothness;
            if (context.MetallicGlossMap != null)
            {
                var index = textureExporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(
                    context.MetallicGlossMap,
                    context.Smoothness,
                    context.OcclusionTexture
                );
                if (index >= 0)
                {
                    dst.pbrMetallicRoughness.metallicRoughnessTexture = new glTFMaterialMetallicRoughnessTextureInfo
                    {
                        index = index,
                        texCoord = 0,
                    };
                    ExportBaseTexTransform(context, dst.pbrMetallicRoughness.metallicRoughnessTexture);
                    dst.pbrMetallicRoughness.metallicFactor = 1.0f;
                    dst.pbrMetallicRoughness.roughnessFactor = 1.0f;
                }
            }
        }

        public void ExportOcclusion(UrpLitContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            if (context.WorkflowType != UrpLitWorkflowType.Metallic) return;

            // Occlusion
            if (context.OcclusionTexture != null)
            {
                var index = textureExporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(
                    context.MetallicGlossMap,
                    context.Smoothness,
                    context.OcclusionTexture
                );
                if (index >= 0)
                {
                    dst.occlusionTexture = new glTFMaterialOcclusionTextureInfo
                    {
                        index = index,
                        texCoord = 0,
                        strength = context.OcclusionStrength,
                    };
                    ExportBaseTexTransform(context, dst.occlusionTexture);
                }
            }
        }

        public void ExportNormal(UrpLitContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            if (context.BumpMap == null) return;

            var index = textureExporter.RegisterExportingAsNormal(context.BumpMap);
            if (index >= 0)
            {
                dst.normalTexture = new glTFMaterialNormalTextureInfo
                {
                    index = index,
                    texCoord = 0,
                    scale = context.BumpScale,
                };
                ExportBaseTexTransform(context, dst.normalTexture);
            }
        }

        public void ExportEmission(UrpLitContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            if (!context.IsEmissionEnabled) return;

            dst.emissiveFactor = context.EmissionColorLinear.ToFloat3(ColorSpace.Linear, ColorSpace.Linear);
            if (context.EmissionTexture != null)
            {
                var index = textureExporter.RegisterExportingAsSRgb(context.EmissionTexture, true);
                if (index >= 0)
                {
                    dst.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                    {
                        index = index,
                        texCoord = 0,
                    };
                    ExportBaseTexTransform(context, dst.emissiveTexture);
                }
            }
        }

        private static void ExportBaseTexTransform(UrpBaseShaderContext context, glTFTextureInfo dst)
        {
            GltfMaterialExportUtils.ExportTextureTransform(
                context.BaseTextureOffset,
                context.BaseTextureScale,
                dst
            );
        }

        public static IEnumerable<Validation> EnumerateValidation(UrpLitContext context)
        {
            var validationContext = ValidationContext.Create(context.Material);

            // Surface Settings
            if (context.WorkflowType != UrpLitWorkflowType.Metallic) yield return Error(ValidationMessage.WorkflowTypeNotSupported);
            if (context.SurfaceType == UrpLitSurfaceType.Transparent && context.IsAlphaClipEnabled) yield return Warning(ValidationMessage.TransparentAlphaClipNotSupported);
            if (context.CullMode == CullMode.Front) yield return Error(ValidationMessage.BackFaceRenderingNotSupported);
            if (context.BlendMode == UrpLitBlendMode.Additive) yield return Error(ValidationMessage.AdditiveBlendModeNotSupported);
            if (context.BlendMode == UrpLitBlendMode.Multiply) yield return Error(ValidationMessage.MultiplyBlendModeNotSupported);

            // Etc Textures
            if (context.ParallaxTexture != null) yield return Warning(ValidationMessage.ParallaxTextureNotSupported);

            yield break;

            Validation Error(ValidationMessage message)
            {
                return Validation.Error(message.ToString(), validationContext);
            }

            Validation Warning(ValidationMessage message)
            {
                return Validation.Warning(message.ToString(), validationContext);
            }
        }

        public enum ValidationMessage
        {
            WorkflowTypeNotSupported,
            TransparentAlphaClipNotSupported,
            BackFaceRenderingNotSupported,
            ParallaxTextureNotSupported,
            AdditiveBlendModeNotSupported,
            MultiplyBlendModeNotSupported,
        }
    }
}