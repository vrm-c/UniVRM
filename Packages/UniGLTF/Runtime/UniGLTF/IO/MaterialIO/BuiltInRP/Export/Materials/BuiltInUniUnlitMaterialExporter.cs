using System;
using UniGLTF.UniUnlit;
using UnityEngine;

namespace UniGLTF
{
    public static class BuiltInUniUnlitMaterialExporter
    {
        public const string TargetShaderName = UniUnlit.UniUnlitUtil.ShaderName;

        public static bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            if (src.shader.name != TargetShaderName)
            {
                dst = default;
                return false;
            }

            dst = glTF_KHR_materials_unlit.CreateDefault();
            dst.name = src.name;

            ExportRenderingSettings(src, dst);
            ExportBaseColor(src, textureExporter, dst);

            return true;
        }

        private static void ExportRenderingSettings(Material src, glTFMaterial dst)
        {
            switch (UniUnlitUtil.GetRenderMode(src))
            {
                case UniUnlitRenderMode.Opaque:
                    dst.alphaMode = glTFBlendMode.OPAQUE.ToString();
                    break;
                case UniUnlitRenderMode.Cutout:
                    dst.alphaMode = glTFBlendMode.MASK.ToString();
                    dst.alphaCutoff = src.GetFloat(UniUnlitUtil.PropNameCutoff);
                    break;
                case UniUnlitRenderMode.Transparent:
                    dst.alphaMode = glTFBlendMode.BLEND.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (UniUnlitUtil.GetCullMode(src))
            {
                case UniUnlitCullMode.Off:
                    dst.doubleSided = true;
                    break;
                case UniUnlitCullMode.Back:
                    dst.doubleSided = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ExportBaseColor(Material src, ITextureExporter textureExporter, glTFMaterial dst)
        {
            if (src.HasProperty(UniUnlitUtil.PropNameColor))
            {
                dst.pbrMetallicRoughness.baseColorFactor = src.GetColor(UniUnlitUtil.PropNameColor).ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
            }

            if (src.HasProperty(UniUnlitUtil.PropNameMainTex))
            {
                var index = textureExporter.RegisterExportingAsSRgb(src.GetTexture(UniUnlitUtil.PropNameMainTex), UniUnlitUtil.GetRenderMode(src) != UniUnlitRenderMode.Opaque);
                if (index != -1)
                {
                    dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo()
                    {
                        index = index,
                    };

                    GltfMaterialExportUtils.ExportTextureTransform(src, dst.pbrMetallicRoughness.baseColorTexture, UniUnlitUtil.PropNameMainTex);
                }
            }
        }
    }
}