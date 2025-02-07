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
        public TinyMToonrMaterialImporter(Material material)
        {
            m_opaque = material;
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
                m_opaque.shader,
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new List<Action<Material>>(),
                new[] { (MaterialDescriptor.MaterialGenerateAsyncFunc)AsyncAction }
            );
            return true;

            Task AsyncAction(Material x, GetTextureAsyncFunc y, IAwaitCaller z) => GenerateMaterialAsync(data, src, mtoon, x, y, z);
        }

        public static async Task GenerateMaterialAsync(GltfData data, glTFMaterial src, VRMC_materials_mtoon mtoon, Material dst, GetTextureAsyncFunc getTextureAsync, IAwaitCaller awaitCaller)
        {
            var context = new TinyMToonMaterialContext(dst);

            // ImportSurfaceSettings(src, context);
            await ImportBaseShadeColorAsync(data, src, mtoon, context, getTextureAsync, awaitCaller);
            // await ImportMetallicRoughnessAsync(data, src, context, getTextureAsync, awaitCaller);
            // await ImportOcclusionAsync(data, src, context, getTextureAsync, awaitCaller);
            // await ImportNormalAsync(data, src, context, getTextureAsync, awaitCaller);
            // await ImportEmissionAsync(data, src, context, getTextureAsync, awaitCaller);

            // context.Validate();
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
    }
}