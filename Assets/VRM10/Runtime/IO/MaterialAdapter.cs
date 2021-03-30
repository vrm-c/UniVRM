using VrmLib;
using System.Collections.Generic;
using System.Numerics;
using UniJSON;
using UniGLTF;
using System;

namespace UniVRM10
{
    public static class MaterialAdapter
    {
        static string CastAlphaMode(VrmLib.AlphaModeType alphaMode)
        {
            if (alphaMode == AlphaModeType.BLEND_ZWRITE)
            {
                return "BLEND";
            }
            return alphaMode.ToString();
        }

        static glTFMaterial ToGltf(this VrmLib.Material src, List<Texture> textures)
        {
            var material = new glTFMaterial
            {
                name = src.Name,
                pbrMetallicRoughness = new glTFPbrMetallicRoughness
                {
                    baseColorFactor = src.BaseColorFactor.ToFloat4(),
                },
                alphaMode = CastAlphaMode(src.AlphaMode),
                alphaCutoff = src.AlphaCutoff,
                doubleSided = src.DoubleSided,
            };
            if (src.BaseColorTexture != null)
            {
                material.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                {
                    index = textures.IndexOfNullable(src.BaseColorTexture.Texture).Value,
                };
            }
            return material;
        }

        public static glTFMaterial PBRToGltf(this PBRMaterial pbr, List<Texture> textures)
        {
            var material = pbr.ToGltf(textures);

            // MetallicRoughness
            material.pbrMetallicRoughness.baseColorFactor = pbr.BaseColorFactor.ToFloat4();
            if (pbr.BaseColorTexture != null)
            {
                material.pbrMetallicRoughness.baseColorTexture = new glTFMaterialBaseColorTextureInfo
                {
                    index = textures.IndexOfNullable(pbr.BaseColorTexture.Texture).Value,
                };
            }
            material.pbrMetallicRoughness.metallicFactor = pbr.MetallicFactor;
            material.pbrMetallicRoughness.roughnessFactor = pbr.RoughnessFactor;
            if (pbr.MetallicRoughnessTexture != null)
            {
                material.pbrMetallicRoughness.metallicRoughnessTexture = new glTFMaterialMetallicRoughnessTextureInfo
                {
                    index = textures.IndexOfNullable(pbr.MetallicRoughnessTexture).Value,
                };
            }

            // Normal
            if (pbr.NormalTexture != null)
            {
                material.normalTexture = new glTFMaterialNormalTextureInfo
                {
                    index = textures.IndexOfNullable(pbr.NormalTexture).Value,
                    scale = pbr.NormalTextureScale
                };
            }

            // Occlusion
            if (pbr.OcclusionTexture != null)
            {
                material.occlusionTexture = new glTFMaterialOcclusionTextureInfo
                {
                    index = textures.IndexOfNullable(pbr.OcclusionTexture).Value,
                    strength = pbr.OcclusionTextureStrength,
                };
            }

            // Emissive
            if (pbr.EmissiveTexture != null)
            {
                material.emissiveTexture = new glTFMaterialEmissiveTextureInfo
                {
                    index = textures.IndexOfNullable(pbr.EmissiveTexture).Value,
                };
            }
            material.emissiveFactor = pbr.EmissiveFactor.ToFloat3();

            // AlphaMode
            material.alphaMode = CastAlphaMode(pbr.AlphaMode);

            // AlphaCutoff
            material.alphaCutoff = pbr.AlphaCutoff;

            // DoubleSided
            material.doubleSided = pbr.DoubleSided;

            return material;
        }

        public static glTFMaterial UnlitToGltf(this UnlitMaterial unlit, List<Texture> textures)
        {
            var material = unlit.ToGltf(textures);

            if (!(material.extensions is glTFExtensionExport extensions))
            {
                extensions = new glTFExtensionExport();
                material.extensions = extensions;
            }
            extensions.Add(
                glTF_KHR_materials_unlit.ExtensionName,
                new ArraySegment<byte>(glTF_KHR_materials_unlit.Raw));

            material.pbrMetallicRoughness.roughnessFactor = 0.9f;
            material.pbrMetallicRoughness.metallicFactor = 0.0f;

            return material;
        }
    }
}
