using System;
using UnityEngine;

namespace UniGLTF
{
    public class UrpLitMaterialExporter
    {
        public string TargetShaderName { get; }

        /// <summary>
        /// "Universal Render Pipeline/Lit" シェーダのマテリアルをエクスポートする。
        ///
        /// targetShaderName に、プロパティに互換性がある他のシェーダを指定することもできる。
        /// </summary>
        public UrpLitMaterialExporter(string targetShaderName = null)
        {
            TargetShaderName = string.IsNullOrEmpty(targetShaderName)
                ? "Universal Render Pipeline/Lit"
                : targetShaderName;
        }

        public bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            try
            {
                if (src == null) throw new ArgumentNullException(nameof(src));
                if (src.shader.name != TargetShaderName) throw new ArgumentException(nameof(src));
                if (textureExporter == null) throw new ArgumentNullException(nameof(textureExporter));

                dst = new glTFMaterial
                {
                    name = src.name,
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness(),
                };

                var context = new UrpLitContext(src);
                if (!Validate(context, out var errorMessage))
                {
                    Debug.LogError(errorMessage, src);
                    throw new UniGLTFNotSupportedException(errorMessage);
                }

                ExportBaseColor(context, dst, textureExporter);

                return true;
            }
            catch (Exception)
            {
                dst = default;
                return false;
            }
        }

        public static void ExportBaseColor(UrpLitContext context, glTFMaterial dst, ITextureExporter textureExporter)
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
                    GltfMaterialExportUtils.ExportTextureTransform(
                        context.BaseTextureOffset,
                        context.BaseTextureScale,
                        dst.pbrMetallicRoughness.baseColorTexture
                    );
                }
            }
        }

        public static bool Validate(UrpLitContext context, out string errorMessage)
        {
            if (context.WorkflowType != UrpLitWorkflowType.Metallic)
            {
                errorMessage = "Specular workflow is not supported.";
                return false;
            }

            if (context.OcclusionTexture != null)
            {
                Debug.LogWarning("Occlusion texture is not supported.");
            }

            if (context.ParallaxTexture != null)
            {
                Debug.LogWarning("Parallax texture is not supported.");
            }

            errorMessage = null;
            return true;
        }
    }
}