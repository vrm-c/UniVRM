using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    /// <summary>
    /// TinyPbr 向け
    /// </summary>
    public sealed class TinyPbrMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public UrpGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public UrpGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();

        public Material OpaqueMaterial { get; set; }
        public Material AlphaBlendMaterial { get; set; }

        /// <param name="material">TinyPbr material</param>
        public TinyPbrMaterialDescriptorGenerator(
            Material opaque,
            Material alphaBlend
            )
        {
            if (opaque == null)
            {
                throw new ArgumentNullException("opaque");
            }
            OpaqueMaterial = opaque;
            AlphaBlendMaterial = alphaBlend ?? opaque;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            // UNLIT
            if (BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out var param)) return param;

            if (TryCreateParam(data, i, out param)) return param;

            // NOTE: Fallback to default material
            if (Symbols.VRM_DEVELOP)
            {
                UniGLTFLogger.Warning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null) => DefaultMaterialImporter.CreateParam(materialName);

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
                src.alphaMode == "BLEND" ? AlphaBlendMaterial.shader : OpaqueMaterial.shader,
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new List<Action<Material>>(),
                new[] { (MaterialDescriptor.MaterialGenerateAsyncFunc)AsyncAction }
            );
            return true;

            Task AsyncAction(Material x, GetTextureAsyncFunc y, IAwaitCaller z) => GenerateMaterialAsync(data, src, x, y, z);
        }

        public static async Task GenerateMaterialAsync(GltfData data, glTFMaterial src, Material dst, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            var context = new TinyPbrMaterialContext(dst);

            ImportSurfaceSettings(src, context);
            await ImportBaseColorAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportMetallicRoughnessAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportOcclusionAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportNormalAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportEmissionAsync(data, src, context, getTextureAsync, awaitCaller);

            // context.Validate();
        }

        public static void ImportSurfaceSettings(glTFMaterial src, TinyPbrMaterialContext context)
        {
            // context.SurfaceType = src.alphaMode switch
            // {
            //     "OPAQUE" => UrpLitSurfaceType.Opaque,
            //     "MASK" => UrpLitSurfaceType.Transparent,
            //     "BLEND" => UrpLitSurfaceType.Transparent,
            //     _ => UrpLitSurfaceType.Opaque,
            // };
            // context.BlendMode = context.SurfaceType switch
            // {
            //     UrpLitSurfaceType.Transparent => UrpLitBlendMode.Alpha,
            //     _ => UrpLitBlendMode.Alpha,
            // };
            context.CutoffEnabled = src.alphaMode == "MASK";
            context.Cutoff = src.alphaCutoff;
            // context.CullMode = src.doubleSided ? CullMode.Off : CullMode.Back;
        }

        public static async Task ImportBaseColorAsync(GltfData data, glTFMaterial src, TinyPbrMaterialContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            var baseColorFactor = GltfMaterialImportUtils.ImportLinearBaseColorFactor(data, src);
            if (baseColorFactor.HasValue)
            {
                context.BaseColorSrgb = baseColorFactor.Value.gamma;
            }

            if (src is { pbrMetallicRoughness: { baseColorTexture: { index: >= 0 } } })
            {
                if (GltfPbrTextureImporter.TryBaseColorTexture(data, src, out _, out var desc))
                {
                    context.BaseTexture = await getTextureAsync(desc, awaitCaller);
                    context.BaseTextureOffset = desc.Offset;
                    context.BaseTextureScale = desc.Scale;
                }
            }
        }

        public static async Task ImportMetallicRoughnessAsync(GltfData data, glTFMaterial src, TinyPbrMaterialContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            if (src.pbrMetallicRoughness != null)
            {
                context.Metallic = src.pbrMetallicRoughness.metallicFactor;
                context.Roughness = src.pbrMetallicRoughness.roughnessFactor;
            }

            if (src is { pbrMetallicRoughness: { metallicRoughnessTexture: { index: >= 0 } } })
            {
                var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.metallicRoughnessTexture);
                if (GltfTextureImporter.TryCreateLinear(data, src.pbrMetallicRoughness.metallicRoughnessTexture.index, offset, scale, out var _, out var desc))
                {
                    context.MetallicRoughnessMap = await getTextureAsync(desc, awaitCaller);
                }
            }
        }

        public static async Task ImportOcclusionAsync(GltfData data, glTFMaterial src, TinyPbrMaterialContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            if (src.occlusionTexture != null)
            {
                context.OcclusionStrength = src.occlusionTexture.strength;
            }
            if (src is { occlusionTexture: { index: >= 0 } })
            {
                var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.occlusionTexture);
                if (GltfTextureImporter.TryCreateLinear(data, src.occlusionTexture.index, offset, scale, out var _, out var desc))
                {
                    context.OcclusionTexture = await getTextureAsync(desc, awaitCaller);
                }
            }
        }

        private static async Task ImportNormalAsync(GltfData data, glTFMaterial src, TinyPbrMaterialContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            if (src.normalTexture is { index: >= 0 })
            {
                if (GltfPbrTextureImporter.TryNormalTexture(data, src, out _, out var desc))
                {
                    context.BumpMap = await getTextureAsync(desc, awaitCaller);
                    context.BumpScale = src.normalTexture.scale;
                }
            }
        }

        private static async Task ImportEmissionAsync(GltfData data, glTFMaterial src, TinyPbrMaterialContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            var emissiveFactor = GltfMaterialImportUtils.ImportLinearEmissiveFactor(data, src);
            if (emissiveFactor.HasValue)
            {
                context.EmissionColorLinear = emissiveFactor.Value;
            }

            if (src is { emissiveTexture: { index: >= 0 } })
            {
                if (GltfPbrTextureImporter.TryEmissiveTexture(data, src, out _, out var desc))
                {
                    context.EmissionTexture = await getTextureAsync(desc, awaitCaller);
                }
            }
        }
    }
}