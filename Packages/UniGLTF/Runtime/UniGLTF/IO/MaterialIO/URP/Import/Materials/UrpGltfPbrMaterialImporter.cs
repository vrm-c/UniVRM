using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    /// <summary>
    /// A class that generates MaterialDescriptor for "Universal Render Pipeline/Lit" shader based on glTF Material specification.
    ///
    /// https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#materials
    /// </summary>
    public class UrpGltfPbrMaterialImporter
    {
        /// <summary>
        /// Can be replaced with custom shaders that are compatible with "Universal Render Pipeline/Lit" properties and keywords.
        /// </summary>
        public Shader Shader { get; set; }

        public UrpGltfPbrMaterialImporter(Shader shader = null)
        {
            Shader = shader != null ? shader : Shader.Find("Universal Render Pipeline/Lit");
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
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new List<Action<Material>>(),
                new [] { (MaterialDescriptor.MaterialGenerateAsyncFunc)AsyncAction }
            );
            return true;

            Task AsyncAction(Material x, GetTextureAsyncFunc y, IAwaitCaller z) => GenerateMaterialAsync(data, src, x, y, z);
        }

        public static async Task GenerateMaterialAsync(GltfData data, glTFMaterial src, Material dst, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            var context = new UrpLitContext(dst);
            context.UnsafeEditMode = true;

            ImportSurfaceSettings(src, context);
            await ImportBaseColorAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportMetallicSmoothnessAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportOcclusionAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportNormalAsync(data, src, context, getTextureAsync, awaitCaller);
            await ImportEmissionAsync(data, src, context, getTextureAsync, awaitCaller);

            context.Validate();
        }

        public static void ImportSurfaceSettings(glTFMaterial src, UrpLitContext context)
        {
            context.SurfaceType = src.alphaMode switch
            {
                "OPAQUE" => UrpLitSurfaceType.Opaque,
                "MASK" => UrpLitSurfaceType.Transparent,
                "BLEND" => UrpLitSurfaceType.Transparent,
                _ => UrpLitSurfaceType.Opaque,
            };
            context.BlendMode = context.SurfaceType switch
            {
                UrpLitSurfaceType.Transparent => UrpLitBlendMode.Alpha,
                _ => UrpLitBlendMode.Alpha,
            };
            context.IsAlphaClipEnabled = src.alphaMode switch
            {
                "MASK" => true,
                _ => false,
            };
            context.Cutoff = src.alphaCutoff;
            context.CullMode = src.doubleSided ? CullMode.Off : CullMode.Back;
        }

        public static async Task ImportBaseColorAsync(GltfData data, glTFMaterial src, UrpLitContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
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

        public static async Task ImportMetallicSmoothnessAsync(GltfData data, glTFMaterial src, UrpLitContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            context.WorkflowType = UrpLitWorkflowType.Metallic;
            context.SmoothnessTextureChannel = UrpLitSmoothnessMapChannel.SpecularMetallicAlpha;
            context.Metallic = src.pbrMetallicRoughness.metallicFactor;
            context.Smoothness = 1.0f - src.pbrMetallicRoughness.roughnessFactor;

            if (src is { pbrMetallicRoughness: { metallicRoughnessTexture: { index: >= 0 } } })
            {
                if (GltfPbrTextureImporter.TryStandardTexture(data, src, out _, out var desc))
                {
                    context.MetallicGlossMap = await getTextureAsync(desc, awaitCaller);
                    context.Metallic = 1;
                    context.Smoothness = 1;
                }
            }
        }

        public static async Task ImportOcclusionAsync(GltfData data, glTFMaterial src, UrpLitContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            if (src is { occlusionTexture: { index: >= 0 } })
            {
                if (GltfPbrTextureImporter.TryStandardTexture(data, src, out _, out var desc))
                {
                    context.OcclusionTexture = await getTextureAsync(desc, awaitCaller);
                    context.OcclusionStrength = src.occlusionTexture.strength;
                }
            }
        }

        private static async Task ImportNormalAsync(GltfData data, glTFMaterial src, UrpLitContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
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

        private static async Task ImportEmissionAsync(GltfData data, glTFMaterial src, UrpLitContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
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

            if (context.EmissionColorLinear is {maxColorComponent: > 0} || context.EmissionTexture != null)
            {
                context.IsEmissionEnabled = true;
            }
        }
    }
}
