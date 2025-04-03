using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public class TinyMToonrMaterialImporter : IMaterialImporter
    {
        private Material m_opaque;
        private Material m_alphablend;
        public TinyMToonrMaterialImporter(Material opaque, Material alphablend)
        {
            m_opaque = opaque;
            m_alphablend = alphablend;
        }

        bool IMaterialImporter.TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            if (i < 0 || i >= data.GLTF.materials.Count)
            {
                matDesc = default;
                return false;
            }

            var src = data.GLTF.materials[i];
            if (!UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(src.extensions, out var mtoon))
            {
                // Fallback to glTF, when MToon extension does not exist.
                matDesc = default;
                return false;
            }

            matDesc = new MaterialDescriptor(
                GltfMaterialImportUtils.ImportMaterialName(i, src),
                src.alphaMode == "BLEND" ? m_alphablend.shader : m_opaque.shader,
                new[] { (MaterialDescriptor.MaterialGenerateAsyncFunc)AsyncAction }
            );
            return true;

            Task AsyncAction(Material x, GetTextureAsyncFunc y, IAwaitCaller z) => GenerateMaterialAsync(data, src, mtoon, x, y, z);
        }

        public static async Task GenerateMaterialAsync(GltfData data, glTFMaterial src, VRMC_materials_mtoon mtoon, Material dst, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            var context = new TinyMToonMaterialContext(dst);

            ImportSurfaceSettings(src, context);
            await ImportBaseShadeColorAsync(data, src, mtoon, context, getTextureAsync, awaitCaller);
            await ImportNormalAsync(data, src, context, getTextureAsync, awaitCaller);
            // await ImportEmissionAsync(data, src, context, getTextureAsync, awaitCaller);

            // context.Validate();
        }

        public static void ImportSurfaceSettings(glTFMaterial src, TinyMToonMaterialContext context)
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

        public static async Task ImportBaseShadeColorAsync(GltfData data, glTFMaterial src, VRMC_materials_mtoon mtoon, TinyMToonMaterialContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
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

            context.ShadingToonyFactor = mtoon.ShadingToonyFactor.GetValueOrDefault();
            context.ShadingShiftFactor = mtoon.ShadingToonyFactor.GetValueOrDefault();
            var shadeColor = mtoon.ShadeColorFactor?.ToColor3(UniGLTF.ColorSpace.Linear, UniGLTF.ColorSpace.sRGB);
            context.ShadingColorFactorSrgb = shadeColor.GetValueOrDefault(Color.white);
            if (mtoon is { ShadeMultiplyTexture: { Index: >= 0 } })
            {
                if (Vrm10MToonTextureImporter.TryGetShadeMultiplyTexture(data, mtoon, out var _, out var desc))
                {
                    context.ShadingTexture = await getTextureAsync(desc, awaitCaller);
                }
            }
        }

        private static async Task ImportNormalAsync(GltfData data, glTFMaterial src, TinyMToonMaterialContext context, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
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
    }
}