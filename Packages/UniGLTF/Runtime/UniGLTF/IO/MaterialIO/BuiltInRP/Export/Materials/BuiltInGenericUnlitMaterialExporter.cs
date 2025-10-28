using System;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Built-in RP で一般的に用いられる Unlit Shader をエクスポートすることを試みる。
    /// </summary>
    public static class BuiltInGenericUnlitMaterialExporter
    {
        private const string ColorFactorPropertyName = "_Color";
        private const string ColorTexturePropertyName = "_MainTex";
        private const string CutoffPropertyName = "_Cutoff";

        public static bool TryExportMaterial(Material src, glTFBlendMode blendMode, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            dst = glTF_KHR_materials_unlit.CreateDefault();
            dst.name = src.name;

            ExportRenderingSettings(src, blendMode, dst);
            ExportBaseColor(src, blendMode, textureExporter, dst);

            return true;
        }

        private static void ExportRenderingSettings(Material src, glTFBlendMode blendMode, glTFMaterial dst)
        {
            switch (blendMode)
            {
                case glTFBlendMode.OPAQUE:
                    dst.alphaMode = glTFBlendMode.OPAQUE.ToString();
                    break;
                case glTFBlendMode.MASK:
                    dst.alphaMode = glTFBlendMode.MASK.ToString();
                    dst.alphaCutoff = src.GetFloat(CutoffPropertyName);
                    break;
                case glTFBlendMode.BLEND:
                    dst.alphaMode = glTFBlendMode.BLEND.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(blendMode), blendMode, null);
            }
        }

        private static void ExportBaseColor(Material src, glTFBlendMode blendMode, ITextureExporter textureExporter, glTFMaterial dst)
        {
            if (src.HasProperty(ColorFactorPropertyName))
            {
                dst.pbrMetallicRoughness.baseColorFactor = src.GetColor(ColorFactorPropertyName).ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
            }

            if (src.HasProperty(ColorTexturePropertyName))
            {
                // Don't export alpha channel if material was OPAQUE
                var unnecessaryAlpha = blendMode == glTFBlendMode.OPAQUE;

                var index = textureExporter.RegisterExportingAsSRgb(src.GetTexture(ColorTexturePropertyName), !unnecessaryAlpha);
                if (index != -1)
                {
                    dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo()
                    {
                        index = index,
                    };

                    GltfMaterialExportUtils.ExportTextureTransform(src, dst.pbrMetallicRoughness.baseColorTexture, ColorTexturePropertyName);
                }
            }
        }
    }
}