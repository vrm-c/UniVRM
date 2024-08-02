using System;
using UniGLTF.UniUnlit;
using UnityEngine;

namespace UniGLTF
{
    public class UrpUniUnlitMaterialExporter
    {
        public Shader Shader { get; set; }

        /// <summary>
        /// "UniGLTF/UniUnlit" シェーダのマテリアルをエクスポートする。
        ///
        /// プロパティに互換性がある他のシェーダを指定することもできる。
        /// </summary>
        public UrpUniUnlitMaterialExporter(Shader shader = null)
        {
            Shader = shader != null ? shader : Shader.Find("UniGLTF/UniUnlit");
        }

        public bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            try
            {
                if (src == null) throw new ArgumentNullException(nameof(src));
                if (src.shader != Shader) throw new ArgumentException(nameof(src));
                if (textureExporter == null) throw new ArgumentNullException(nameof(textureExporter));

                dst = glTF_KHR_materials_unlit.CreateDefault();
                dst.name = src.name;

                var context = new UniUnlitContext(src);

                dst.alphaMode = context.RenderMode switch
                {
                    UniUnlitRenderMode.Opaque => glTFBlendMode.OPAQUE.ToString(),
                    UniUnlitRenderMode.Cutout => glTFBlendMode.MASK.ToString(),
                    UniUnlitRenderMode.Transparent => glTFBlendMode.BLEND.ToString(),
                    _ => throw new ArgumentOutOfRangeException(),
                };
                dst.alphaCutoff = context.Cutoff;
                dst.doubleSided = context.CullMode != UniUnlitCullMode.Back;

                dst.pbrMetallicRoughness.baseColorFactor = context.MainColorSrgb
                    .ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);
                if (context.MainTexture != null)
                {
                    var needsAlpha = context.RenderMode != UniUnlitRenderMode.Opaque;
                    var index = textureExporter.RegisterExportingAsSRgb(context.MainTexture, needsAlpha);
                    if (index >= 0)
                    {
                        dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                        {
                            index = index,
                            texCoord = 0,
                        };
                        GltfMaterialExportUtils.ExportTextureTransform(
                            context.MainTextureOffset,
                            context.MainTextureScale,
                            dst.pbrMetallicRoughness.baseColorTexture);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                dst = default;
                return false;
            }
        }
    }
}