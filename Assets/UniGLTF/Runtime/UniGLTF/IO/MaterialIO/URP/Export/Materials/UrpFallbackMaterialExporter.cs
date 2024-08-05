using System;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// フォールバック目的で最低限なにかをエクスポートする。
    ///
    /// メインカラーとメインテクスチャをエクスポートする。
    /// </summary>
    public class UrpFallbackMaterialExporter
    {
        public glTFMaterial ExportMaterial(Material src, ITextureExporter textureExporter)
        {
            var dst = new glTFMaterial
            {
                name = src.name,
                pbrMetallicRoughness = new glTFPbrMetallicRoughness(),
            };

            dst.pbrMetallicRoughness.baseColorFactor = src.color.ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
            var index = textureExporter.RegisterExportingAsSRgb(src.mainTexture, false);
            if (index >= 0)
            {
                dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                {
                    index = index,
                    texCoord = 0,
                };
                GltfMaterialExportUtils.ExportTextureTransform(src.mainTextureOffset, src.mainTextureScale, dst.pbrMetallicRoughness.baseColorTexture);
            }

            return dst;
        }
    }
}