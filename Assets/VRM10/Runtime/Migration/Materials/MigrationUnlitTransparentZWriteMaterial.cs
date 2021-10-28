using System;
using UniGLTF;
using UniGLTF.Extensions.VRMC_materials_mtoon;
using UniJSON;

namespace UniVRM10
{
    /// <summary>
    /// 過去の UniVRM の "VRM/UnlitTransparentZWrite" シェーダをマイグレーションする.
    /// 他の Unlit シェーダと違い、VRMC_materials_mtoon を用いてマイグレーションする.
    /// </summary>
    public static class MigrationUnlitTransparentZWriteMaterial
    {
        private const string MigrationMToon10SpecVersion = "1.0-draft";

        public static glTFMaterial Migrate(JsonNode vrm0XMaterial, string materialName)
        {
            var baseColorFactor = MigrationMaterialUtil.GetBaseColorFactor(vrm0XMaterial);
            var baseColorTexture = MigrationMaterialUtil.GetBaseColorTexture(vrm0XMaterial);
            var emissiveTexture = new glTFMaterialEmissiveTextureInfo
            {
                index = baseColorTexture.index,
                extensions = baseColorTexture.extensions,
            };

            var mtoonMaterial = new glTFMaterial
            {
                name = materialName,
                extensions = new glTFExtensionExport().Add(
                    glTF_KHR_materials_unlit.ExtensionName,
                    new ArraySegment<byte>(glTF_KHR_materials_unlit.Raw)
                ),
                pbrMetallicRoughness = new glTFPbrMetallicRoughness
                {
                    baseColorFactor = new [] {0f, 0f, 0f, baseColorFactor[3]}, // black + _Color.a
                    baseColorTexture = baseColorTexture, // _MainTex
                    metallicFactor = 0f,
                    roughnessFactor = 1f,
                },
                alphaMode = "BLEND",
                alphaCutoff = 0.5f,
                doubleSided = false,
                emissiveFactor = new [] {baseColorFactor[0], baseColorFactor[1], baseColorFactor[2]}, // _Color.rgb
                emissiveTexture = emissiveTexture,
            };

            var mtoon10 = new VRMC_materials_mtoon
            {
                SpecVersion = MigrationMToon10SpecVersion,
                TransparentWithZWrite = true, // transparent with zWrite
                RenderQueueOffsetNumber = 0,
                ShadeColorFactor = new [] {0f, 0f, 0f}, // black
                OutlineWidthMode = OutlineWidthMode.none // disable outline
            };
            UniGLTF.Extensions.VRMC_materials_mtoon.GltfSerializer.SerializeTo(ref mtoonMaterial.extensions, mtoon10);

            return mtoonMaterial;
        }
    }
}