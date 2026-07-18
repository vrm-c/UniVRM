using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// glTF 準拠 PBR ShaderGraph (UniGltfPbr) 向けの MaterialDescriptor を生成する。
    ///
    /// 既定の Standard / URP-Lit 経路との違い:
    /// - metallicRoughnessTexture / occlusionTexture を glTF のパッキングのまま Linear で取り込む
    ///   (OcclusionMetallicRoughnessConverter による GPU blit + ReadPixels 変換を行わない)
    /// - metallicFactor / roughnessFactor をそのまま格納する (1.0 固定や smoothness 反転をしない)
    ///
    /// 構造は UniGLTF.UrpGltfPbrMaterialImporter を踏襲する。
    /// https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#materials
    /// </summary>
    public class InvariantRpUniGltfPbrMaterialImporter
    {
        public Shader Shader { get; set; }
        
        public InvariantRpUniGltfPbrMaterialImporter(Shader shader = null)
        {
            Shader = shader ?? UniGltfPbrContext.GetShader();
            if (Shader == null)
            {
                // Player ビルドではどのアセットからも参照されないシェーダは含まれず Shader.Find が null を返す
                UniGLTFLogger.Error($"shader '{UniGltfPbrContext.ShaderName}' not found. Project Settings > Graphics > Always Included Shaders への登録が必要です");
            }
        }

        public bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            if (i < 0 || i >= data.GLTF.materials.Count)
            {
                matDesc = default;
                return false;
            }

            var src = data.GLTF.materials[i];
            matDesc = new MaterialDescriptor(
                GltfMaterialImportUtils.ImportMaterialName(i, src),
                Shader,
                new[] { (MaterialDescriptor.MaterialGenerateAsyncFunc)AsyncAction }
            );
            return true;

            Task AsyncAction(Material x, GetTextureAsyncFunc y, IAwaitCaller z) => GenerateMaterialAsync(data, src, x, y, z);
        }

        /// <summary>
        /// glTF default material (プロパティ既定値でシェーダを割り当てるだけ)。
        /// シェーダ側の既定値 (baseColor=white, metallic=1, roughness=1) が glTF default に一致する。
        /// </summary>
        public MaterialDescriptor CreateDefaultParam(string materialName)
        {
            return new MaterialDescriptor(
                materialName,
                Shader,
                new List<MaterialDescriptor.MaterialGenerateAsyncFunc>()
            );
        }

        public static async Task GenerateMaterialAsync(GltfData data, glTFMaterial src, Material dst, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            var context = new UniGltfPbrContext(dst);

            ImportSurfaceSettings(src, context);
            await ImportBaseColorAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportMetallicRoughnessAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportOcclusionAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportNormalAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportEmissionAsync(data, src, context, getTextureAsync, awaitCaller);

            context.Validate();
        }

        public static void ImportSurfaceSettings(glTFMaterial src, UniGltfPbrContext context)
        {
            context.SurfaceType = src.alphaMode switch
            {
                "OPAQUE" => UniGltfPbrSurfaceType.Opaque,
                "MASK" => UniGltfPbrSurfaceType.Opaque, // AlphaClip. Opaque queue (AlphaTest)
                "BLEND" => UniGltfPbrSurfaceType.Transparent,
                _ => UniGltfPbrSurfaceType.Opaque,
            };
            context.IsAlphaClipEnabled = src.alphaMode == "MASK";
            context.AlphaCutoff = src.alphaCutoff;
            context.CullMode = src.doubleSided ? CullMode.Off : CullMode.Back;
        }

        public static async Task ImportBaseColorAsync(GltfData data, glTFMaterial src, UniGltfPbrContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            var baseColorFactor = GltfMaterialImportUtils.ImportLinearBaseColorFactor(data, src);
            if (baseColorFactor.HasValue)
            {
                // _baseColorFactor は Default(sRGB) カラー想定なので .gamma で格納する
                context.BaseColorSrgb = baseColorFactor.Value.gamma;
            }

            if (src is { pbrMetallicRoughness: { baseColorTexture: { index: >= 0 } } })
            {
                if (GltfPbrTextureImporter.TryBaseColorTexture(data, src, out _, out var desc))
                {
                    context.BaseColorTexture = await getTextureAsync(desc, awaitCaller);
                    context.BaseColorTextureOffset = desc.Offset;
                    context.BaseColorTextureScale = desc.Scale;
                }
            }
        }

        public static async Task ImportMetallicRoughnessAsync(GltfData data, glTFMaterial src, UniGltfPbrContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            if (src.pbrMetallicRoughness != null)
            {
                context.MetallicFactor = src.pbrMetallicRoughness.metallicFactor;
                context.RoughnessFactor = src.pbrMetallicRoughness.roughnessFactor;
            }

            if (src is { pbrMetallicRoughness: { metallicRoughnessTexture: { index: >= 0 } } })
            {
                var info = src.pbrMetallicRoughness.metallicRoughnessTexture;
                var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(info);
                // NOTE: glTF の ORM パッキングをそのまま Linear で取り込む (変換なし)
                if (GltfTextureImporter.TryCreateLinear(data, info.index, offset, scale, out _, out var desc))
                {
                    context.MetallicRoughnessTexture = await getTextureAsync(desc, awaitCaller);
                    context.MetallicRoughnessTextureOffset = desc.Offset;
                    context.MetallicRoughnessTextureScale = desc.Scale;
                }
            }
        }

        public static async Task ImportOcclusionAsync(GltfData data, glTFMaterial src, UniGltfPbrContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            if (src is { occlusionTexture: { index: >= 0 } })
            {
                context.OcclusionStrength = src.occlusionTexture.strength;

                var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.occlusionTexture);
                if (GltfTextureImporter.TryCreateLinear(data, src.occlusionTexture.index, offset, scale, out _, out var desc))
                {
                    context.OcclusionTexture = await getTextureAsync(desc, awaitCaller);
                    context.OcclusionTextureOffset = desc.Offset;
                    context.OcclusionTextureScale = desc.Scale;
                }
            }
        }

        private static async Task ImportNormalAsync(GltfData data, glTFMaterial src, UniGltfPbrContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            if (src.normalTexture is { index: >= 0 })
            {
                if (GltfPbrTextureImporter.TryNormalTexture(data, src, out _, out var desc))
                {
                    context.NormalTexture = await getTextureAsync(desc, awaitCaller);
                    context.NormalScale = src.normalTexture.scale;
                    context.NormalTextureOffset = desc.Offset;
                    context.NormalTextureScale = desc.Scale;
                }
            }
        }

        /// <summary>
        /// emissiveFactor と KHR_materials_emissive_strength を分離して取り込む。
        /// (GltfMaterialImportUtils.ImportLinearEmissiveFactor は strength を factor に畳み込むため使わない)
        /// </summary>
        private static async Task ImportEmissionAsync(GltfData data, glTFMaterial src, UniGltfPbrContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            if (src.emissiveFactor != null && src.emissiveFactor.Length == 3)
            {
                var emissive = new Vector3(src.emissiveFactor[0], src.emissiveFactor[1], src.emissiveFactor[2]);
                var linear = data.MigrationFlags.IsEmissiveFactorGamma
                    ? emissive.ToColor3(ColorSpace.sRGB, ColorSpace.Linear)
                    : emissive.ToColor3(ColorSpace.Linear, ColorSpace.Linear);
                context.EmissiveFactorLinear = linear;
            }

            var strength = 1.0f;
            if (glTF_KHR_materials_emissive_strength.TryGet(src.extensions, out var emissiveStrength))
            {
                strength = emissiveStrength.emissiveStrength;
            }
            context.EmissiveStrength = strength;

            if (src is { emissiveTexture: { index: >= 0 } })
            {
                if (GltfPbrTextureImporter.TryEmissiveTexture(data, src, out _, out var desc))
                {
                    context.EmissiveTexture = await getTextureAsync(desc, awaitCaller);
                    context.EmissiveTextureOffset = desc.Offset;
                    context.EmissiveTextureScale = desc.Scale;
                }
            }

            var isEmissive = context.EmissiveFactorLinear.maxColorComponent > 0 || context.EmissiveTexture != null;
            context.Material.globalIlluminationFlags = isEmissive
                ? context.Material.globalIlluminationFlags & ~MaterialGlobalIlluminationFlags.EmissiveIsBlack
                : context.Material.globalIlluminationFlags | MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
    }
}
