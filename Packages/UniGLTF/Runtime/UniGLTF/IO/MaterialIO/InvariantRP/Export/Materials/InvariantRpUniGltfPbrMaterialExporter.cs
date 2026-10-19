using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    public class InvariantRpUniGltfPbrMaterialExporter
    {
        public Shader Shader { get; set; }
        
        public InvariantRpUniGltfPbrMaterialExporter(Shader shader = null)
        {
            Shader = shader ?? UniGltfPbrContext.GetShader();
        }

        public bool TryExportMaterial(Material src, ITextureExporter textureExporter, out glTFMaterial dst)
        {
            if (src == null || src.shader == null || src.shader != Shader)
            {
                dst = default;
                return false;
            }

            dst = new glTFMaterial
            {
                name = src.name,
                pbrMetallicRoughness = new glTFPbrMetallicRoughness(),
            };

            var context = new UniGltfPbrContext(src);

            ExportSurfaceSettings(context, dst);
            ExportBaseColor(context, dst, textureExporter);
            ExportMetallicRoughness(context, dst, textureExporter);
            ExportOcclusion(context, dst, textureExporter);
            ExportNormal(context, dst, textureExporter);
            ExportEmission(context, dst, textureExporter);

            return true;
        }

        private static void ExportSurfaceSettings(UniGltfPbrContext context, glTFMaterial dst)
        {
            dst.alphaMode = (context.SurfaceType, context.IsAlphaClipEnabled) switch
            {
                (UniGltfPbrSurfaceType.Opaque, false) => glTFBlendMode.OPAQUE.ToString(),
                (UniGltfPbrSurfaceType.Opaque, true) => glTFBlendMode.MASK.ToString(),
                (UniGltfPbrSurfaceType.Transparent, _) => glTFBlendMode.BLEND.ToString(),
                _ => glTFBlendMode.OPAQUE.ToString(),
            };
            dst.alphaCutoff = context.AlphaCutoff;
            dst.doubleSided = context.CullMode != CullMode.Back; // NOTE: cull front は glTF 非対応
        }

        private static void ExportBaseColor(UniGltfPbrContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            // _baseColorFactor は sRGB 格納なので sRGB->Linear で glTF に戻す
            dst.pbrMetallicRoughness.baseColorFactor = context.BaseColorSrgb.ToFloat4(ColorSpace.sRGB, ColorSpace.Linear);

            if (context.BaseColorTexture != null)
            {
                var needsAlpha = context.SurfaceType != UniGltfPbrSurfaceType.Opaque;
                var index = textureExporter.RegisterExportingAsSRgb(context.BaseColorTexture, needsAlpha);
                if (index >= 0)
                {
                    dst.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                    {
                        index = index,
                        texCoord = 0,
                    };
                    ExportTextureTransform(context.BaseColorTextureOffset, context.BaseColorTextureScale, dst.pbrMetallicRoughness.baseColorTexture);
                }
            }
        }

        private static void ExportMetallicRoughness(UniGltfPbrContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            // 反転・1 固定なし。factor とテクスチャは独立 (glTF セマンティクス通り)
            dst.pbrMetallicRoughness.metallicFactor = context.MetallicFactor;
            dst.pbrMetallicRoughness.roughnessFactor = context.RoughnessFactor;

            if (context.MetallicRoughnessTexture != null)
            {
                // 既に glTF ネイティブ ORM パッキング -> Linear passthrough (チャンネル入替なし)
                var index = textureExporter.RegisterExportingAsLinear(context.MetallicRoughnessTexture, false);
                if (index >= 0)
                {
                    dst.pbrMetallicRoughness.metallicRoughnessTexture = new glTFMaterialMetallicRoughnessTextureInfo
                    {
                        index = index,
                        texCoord = 0,
                    };
                    ExportTextureTransform(context.MetallicRoughnessTextureOffset, context.MetallicRoughnessTextureScale, dst.pbrMetallicRoughness.metallicRoughnessTexture);
                }
            }
        }

        private static void ExportOcclusion(UniGltfPbrContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            if (context.OcclusionTexture != null)
            {
                // metallicRoughnessTexture と同一 Texture の場合は TextureExporter が index を dedupe する
                var index = textureExporter.RegisterExportingAsLinear(context.OcclusionTexture, false);
                if (index >= 0)
                {
                    dst.occlusionTexture = new glTFMaterialOcclusionTextureInfo
                    {
                        index = index,
                        texCoord = 0,
                        strength = context.OcclusionStrength,
                    };
                    ExportTextureTransform(context.OcclusionTextureOffset, context.OcclusionTextureScale, dst.occlusionTexture);
                }
            }
        }

        private static void ExportNormal(UniGltfPbrContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            if (context.NormalTexture == null) return;

            var index = textureExporter.RegisterExportingAsNormal(context.NormalTexture);
            if (index >= 0)
            {
                dst.normalTexture = new glTFMaterialNormalTextureInfo
                {
                    index = index,
                    texCoord = 0,
                    scale = context.NormalScale,
                };
                ExportTextureTransform(context.NormalTextureOffset, context.NormalTextureScale, dst.normalTexture);
            }
        }

        private static void ExportEmission(UniGltfPbrContext context, glTFMaterial dst, ITextureExporter textureExporter)
        {
            var emissive = context.EmissiveFactorLinear; // linear
            var strength = context.EmissiveStrength;

            // emissiveFactor * emissiveStrength を放射輝度とし、glTF-valid (factor<=1) になるよう正規化する。
            // maxComponent が 1 を超える分は KHR_materials_emissive_strength に逃がす。
            var total = new Vector3(emissive.r * strength, emissive.g * strength, emissive.b * strength);
            var maxComp = Mathf.Max(total.x, Mathf.Max(total.y, total.z));
            if (maxComp > 1.0f)
            {
                dst.emissiveFactor = new[] { total.x / maxComp, total.y / maxComp, total.z / maxComp };
                glTF_KHR_materials_emissive_strength.Serialize(ref dst.extensions, maxComp);
            }
            else
            {
                dst.emissiveFactor = new[] { total.x, total.y, total.z };
            }

            if (context.EmissiveTexture != null)
            {
                var index = textureExporter.RegisterExportingAsSRgb(context.EmissiveTexture, true);
                if (index >= 0)
                {
                    dst.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                    {
                        index = index,
                        texCoord = 0,
                    };
                    ExportTextureTransform(context.EmissiveTextureOffset, context.EmissiveTextureScale, dst.emissiveTexture);
                }
            }
        }

        private static void ExportTextureTransform(Vector2 offset, Vector2 scale, glTFTextureInfo dst)
        {
            // NOTE: シェーダは全テクスチャに ScaleOffset を持つため、各テクスチャ固有の transform を出力する
            GltfMaterialExportUtils.ExportTextureTransform(offset, scale, dst);
        }
    }
}