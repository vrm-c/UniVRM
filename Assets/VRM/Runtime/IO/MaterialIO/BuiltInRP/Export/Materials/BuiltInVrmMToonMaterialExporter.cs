using System;
using MToon;
using UniGLTF;
using UnityEngine;
using ColorSpace = UniGLTF.ColorSpace;
using RenderMode = MToon.RenderMode;

namespace VRM
{
    /// <summary>
    /// VRM/MToon のマテリアル情報をエクスポートする。
    /// ただし VRM 0.x としては VRM extension 内の materialProperties に記録されているデータが正である。
    /// したがって、ここで出力するデータはあくまで VRM を表示できない glTF ビューワでの見た目をある程度保証するために作成するものである。
    /// </summary>
    public static class BuiltInVrmMToonMaterialExporter
    {
        public const string TargetShaderName = MToon.Utils.ShaderName;

        public static bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            if (src.shader.name != TargetShaderName)
            {
                dst = default;
                return false;
            }

            var srcProps = MToon.Utils.GetMToonParametersFromMaterial(src);

            dst = glTF_KHR_materials_unlit.CreateDefault();
            dst.name = src.name;
            ExportRenderingSettings(srcProps, dst);
            ExportBaseColor(src, srcProps, textureExporter, dst);
            ExportEmission(src, srcProps, textureExporter, dst);
            ExportNormal(src, srcProps, textureExporter, dst);

            return true;
        }

        private static void ExportRenderingSettings(MToonDefinition src, glTFMaterial dst)
        {
            switch (src.Rendering.RenderMode)
            {
                case RenderMode.Opaque:
                    dst.alphaMode = glTFBlendMode.OPAQUE.ToString();
                    break;
                case RenderMode.Cutout:
                    dst.alphaMode = glTFBlendMode.MASK.ToString();
                    dst.alphaCutoff = src.Color.CutoutThresholdValue;
                    break;
                case RenderMode.Transparent:
                    dst.alphaMode = glTFBlendMode.BLEND.ToString();
                    break;
                case RenderMode.TransparentWithZWrite:
                    // NOTE: Ambiguous but better.
                    dst.alphaMode = glTFBlendMode.BLEND.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (src.Rendering.CullMode)
            {
                case CullMode.Off:
                    dst.doubleSided = true;
                    break;
                case CullMode.Front:
                    // NOTE: Ambiguous but better.
                    dst.doubleSided = true;
                    break;
                case CullMode.Back:
                    dst.doubleSided = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ExportBaseColor(Material srcMaterial, MToonDefinition src, ITextureExporter textureExporter, glTFMaterial dst)
        {
            dst.pbrMetallicRoughness.baseColorFactor = src.Color.LitColor.ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);

            if (src.Color.LitMultiplyTexture != null)
            {
                var index = textureExporter.RegisterExportingAsSRgb(src.Color.LitMultiplyTexture, src.Rendering.RenderMode != RenderMode.Opaque);
                if (index != -1)
                {
                    dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo()
                    {
                        index = index,
                    };
                    ExportMainTextureTransform(srcMaterial, dst.pbrMetallicRoughness.baseColorTexture);
                }
            }
        }

        private static void ExportEmission(Material srcMaterial, MToonDefinition src, ITextureExporter textureExporter, glTFMaterial dst)
        {
            var emissionFactor = src.Emission.EmissionColor;
            if (emissionFactor.maxColorComponent > 1)
            {
                emissionFactor /= emissionFactor.maxColorComponent;
            }
            dst.emissiveFactor = emissionFactor.ToFloat3(ColorSpace.Linear, ColorSpace.Linear);

            if (src.Emission.EmissionMultiplyTexture != null)
            {
                var index = textureExporter.RegisterExportingAsSRgb(src.Emission.EmissionMultiplyTexture, needsAlpha: false);
                if (index != -1)
                {
                    dst.emissiveTexture = new glTFMaterialEmissiveTextureInfo()
                    {
                        index = index,
                    };
                    ExportMainTextureTransform(srcMaterial, dst.emissiveTexture);
                }
            }
        }

        private static void ExportNormal(Material srcMaterial, MToonDefinition src, ITextureExporter textureExporter, glTFMaterial dst)
        {
            if (src.Lighting.Normal.NormalTexture != null)
            {
                var index = textureExporter.RegisterExportingAsNormal(src.Lighting.Normal.NormalTexture);
                if (index != -1)
                {
                    dst.normalTexture = new glTFMaterialNormalTextureInfo()
                    {
                        index = index,
                        scale = src.Lighting.Normal.NormalScaleValue,
                    };
                    ExportMainTextureTransform(srcMaterial, dst.normalTexture);
                }
            }
        }

        private static void ExportMainTextureTransform(Material src, glTFTextureInfo targetTextureInfo)
        {
            GltfMaterialExportUtils.ExportTextureTransform(src, targetTextureInfo, MToon.Utils.PropMainTex);
        }
    }
}