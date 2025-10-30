using System;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// 非対応のシェーダでも、baseColor だけ読み取ったマテリアルにする.
    /// </summary>
    public static class BuiltInFallbackMaterialExporter
    {
        private const string ColorPropertyName = "_Color";
        private const string ColorTexturePropertyName = "_MainTex";

        public static glTFMaterial ExportMaterial(Material src, ITextureExporter textureExporter)
        {
            var dst = new glTFMaterial
            {
                name = src.name,
                pbrMetallicRoughness = new glTFPbrMetallicRoughness(),
            };

            if (src.HasProperty(ColorPropertyName))
            {
                dst.pbrMetallicRoughness.baseColorFactor = src.GetColor(ColorPropertyName).ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
            }

            if (src.HasProperty(ColorTexturePropertyName) && src.GetTexture(ColorTexturePropertyName) != null)
            {
                dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo()
                {
                    index = textureExporter.RegisterExportingAsSRgb(src.GetTexture(ColorTexturePropertyName), false),
                };
                GltfMaterialExportUtils.ExportTextureTransform(src, dst.pbrMetallicRoughness.baseColorTexture, ColorTexturePropertyName);
            }

            return dst;
        }
    }
}