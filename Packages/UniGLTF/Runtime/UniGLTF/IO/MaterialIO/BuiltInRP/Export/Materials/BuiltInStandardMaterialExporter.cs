using System;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public static class BuiltInStandardMaterialExporter
    {
        public const string TargetShaderName = "Standard";

        private const string ColorTexturePropertyName = "_MainTex";
        private const string MetallicGlossTexturePropertyName = "_MetallicGlossMap";
        private const string NormalTexturePropertyName = "_BumpMap";
        private const string EmissionTexturePropertyName = "_EmissionMap";
        private const string OcclusionTexturePropertyName = "_OcclusionMap";

        public static bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            if (src.shader.name != TargetShaderName)
            {
                dst = default;
                return false;
            }

            dst = new glTFMaterial
            {
                name = src.name,
                pbrMetallicRoughness = new glTFPbrMetallicRoughness(),
            };

            ExportRenderingSettings(src, dst);
            ExportBaseColor(src, textureExporter, dst);
            ExportEmission(src, textureExporter, dst);
            ExportNormal(src, textureExporter, dst);
            ExportOcclusionMetallicRoughness(src, textureExporter, dst);

            return true;
        }

        private static void ExportRenderingSettings(Material src, glTFMaterial dst)
        {
            switch (src.GetTag("RenderType", true))
            {
                case "Transparent":
                    dst.alphaMode = glTFBlendMode.BLEND.ToString();
                    break;

                case "TransparentCutout":
                    dst.alphaMode = glTFBlendMode.MASK.ToString();
                    dst.alphaCutoff = src.GetFloat("_Cutoff");
                    break;

                default:
                    dst.alphaMode = glTFBlendMode.OPAQUE.ToString();
                    break;
            }
        }

        private static void ExportBaseColor(Material src, ITextureExporter textureExporter, glTFMaterial dst)
        {
            if (src.HasProperty("_Color"))
            {
                dst.pbrMetallicRoughness.baseColorFactor = src.GetColor("_Color").ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
            }

            if (src.HasProperty(ColorTexturePropertyName))
            {
                // Don't export alpha channel if material was OPAQUE
                var unnecessaryAlpha = string.Equals(dst.alphaMode, "OPAQUE", StringComparison.Ordinal);

                var index = textureExporter.RegisterExportingAsSRgb(src.GetTexture(ColorTexturePropertyName), !unnecessaryAlpha);
                if (index != -1)
                {
                    dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo()
                    {
                        index = index,
                    };

                    ExportMainTextureTransform(src, dst.pbrMetallicRoughness.baseColorTexture);
                }
            }
        }

        /// <summary>
        /// Occlusion, Metallic, Roughness
        /// </summary>
        private static void ExportOcclusionMetallicRoughness(Material src, ITextureExporter textureExporter, glTFMaterial dst)
        {
            Texture metallicSmoothTexture = default;
            float smoothness = 1.0f;

            var textuerNames = src.GetTexturePropertyNames();
            if (textuerNames.Contains(MetallicGlossTexturePropertyName))
            {
                if (src.HasProperty("_GlossMapScale"))
                {
                    smoothness = src.GetFloat("_GlossMapScale");
                }
                metallicSmoothTexture = src.GetTexture(MetallicGlossTexturePropertyName);
            }

            Texture occlusionTexture = default;
            var occlusionStrength = 1.0f;
            if (textuerNames.Contains(OcclusionTexturePropertyName))
            {
                occlusionTexture = src.GetTexture(OcclusionTexturePropertyName);
                if (occlusionTexture != null && src.HasProperty("_OcclusionStrength"))
                {
                    occlusionStrength = src.GetFloat("_OcclusionStrength");
                }
            }

            int index = textureExporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(metallicSmoothTexture, smoothness, occlusionTexture);

            if (index != -1 && metallicSmoothTexture != null)
            {
                dst.pbrMetallicRoughness.metallicRoughnessTexture =
                    new glTFMaterialMetallicRoughnessTextureInfo()
                    {
                        index = index,
                    };
                ExportMainTextureTransform(src, dst.pbrMetallicRoughness.metallicRoughnessTexture);

                // Set 1.0f as hard-coded. See: https://github.com/dwango/UniVRM/issues/212.
                dst.pbrMetallicRoughness.metallicFactor = 1.0f;
                dst.pbrMetallicRoughness.roughnessFactor = 1.0f;
            }
            else
            {
                if (src.HasProperty("_Metallic"))
                {
                    dst.pbrMetallicRoughness.metallicFactor = src.GetFloat("_Metallic");
                }

                if (src.HasProperty("_Glossiness"))
                {
                    dst.pbrMetallicRoughness.roughnessFactor = 1.0f - src.GetFloat("_Glossiness");
                }
            }

            if (index != -1 && occlusionTexture != null)
            {
                dst.occlusionTexture = new glTFMaterialOcclusionTextureInfo()
                {
                    index = index,
                    strength = occlusionStrength,
                };
                ExportMainTextureTransform(src, dst.occlusionTexture);
            }
        }

        private static void ExportNormal(Material src, ITextureExporter textureExporter, glTFMaterial dst)
        {
            if (src.HasProperty(NormalTexturePropertyName))
            {
                var index = textureExporter.RegisterExportingAsNormal(src.GetTexture(NormalTexturePropertyName));
                if (index != -1)
                {
                    dst.normalTexture = new glTFMaterialNormalTextureInfo()
                    {
                        index = index,
                    };

                    ExportMainTextureTransform(src, dst.normalTexture);
                }

                if (index != -1 && src.HasProperty("_BumpScale"))
                {
                    dst.normalTexture.scale = src.GetFloat("_BumpScale");
                }
            }
        }

        private static void ExportEmission(Material src, ITextureExporter textureExporter, glTFMaterial dst)
        {
            if (src.IsKeywordEnabled("_EMISSION") == false)
            {
                return;
            }

            if (src.HasProperty("_EmissionColor"))
            {
                var color = src.GetColor("_EmissionColor");
                // NOTE: Built-in RP Standard shader's emission color is in gamma color space.
                var linearFactor = color.ToFloat3(ColorSpace.sRGB, ColorSpace.Linear);
                var maxComponent = linearFactor.Max();
                if (maxComponent > 1)
                {
                    linearFactor = linearFactor.Select(x => x / maxComponent).ToArray();
                    UniGLTF.glTF_KHR_materials_emissive_strength.Serialize(ref dst.extensions, maxComponent);
                }
                dst.emissiveFactor = linearFactor;
            }

            if (src.HasProperty(EmissionTexturePropertyName))
            {
                var index = textureExporter.RegisterExportingAsSRgb(src.GetTexture(EmissionTexturePropertyName), needsAlpha: false);
                if (index != -1)
                {
                    dst.emissiveTexture = new glTFMaterialEmissiveTextureInfo()
                    {
                        index = index,
                    };

                    ExportMainTextureTransform(src, dst.emissiveTexture);
                }
            }
        }

        private static void ExportMainTextureTransform(Material src, glTFTextureInfo targetTextureInfo)
        {
            GltfMaterialExportUtils.ExportTextureTransform(src, targetTextureInfo, ColorTexturePropertyName);
        }
    }
}