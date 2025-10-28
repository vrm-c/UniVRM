using System;
using System.Collections.Generic;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Migration Target: VRM file with Unlit material exported with UniVRM v0.43 or lower.
    ///
    /// 過去の UniVRM において、KHR_materials_unlit 拡張を使わず、VRM 拡張を用いて Unlit を表現していた Material をマイグレーションする。
    /// KHR_materials_unlit を用いてマイグレーションする.
    /// </summary>
    internal static class MigrationLegacyUnlitMaterial
    {
        public static bool Migrate(glTF gltf, IReadOnlyList<JsonNode> vrm0XMaterials)
        {
            var anyMigrated = false;

            for (var materialIdx = 0; materialIdx < gltf.materials.Count; ++materialIdx)
            {
                try
                {
                    var newMaterial = Migrate(vrm0XMaterials[materialIdx], gltf.materials[materialIdx].name);
                    if (newMaterial != null)
                    {
                        // NOTE: マイグレーション対象だった場合、上書きする.
                        gltf.materials[materialIdx] = newMaterial;
                        anyMigrated = true;
                    }
                }
                catch (Exception ex)
                {
                    UniGLTFLogger.Exception(ex);
                }
            }

            return anyMigrated;
        }

        private static glTFMaterial Migrate(JsonNode vrm0XMaterial, string materialName)
        {
            var unlitMaterial = new glTFMaterial
            {
                name = materialName,
                pbrMetallicRoughness = new glTFPbrMetallicRoughness
                {
                    metallicFactor = 0f,
                    roughnessFactor = 1f,
                },
                extensions = new glTFExtensionExport()
                    .Add(glTF_KHR_materials_unlit.ExtensionName, new ArraySegment<byte>(glTF_KHR_materials_unlit.Raw)),
            };

            switch (MigrationMaterialUtil.GetShaderName(vrm0XMaterial))
            {
                case "Unlit/Color":
                    unlitMaterial.pbrMetallicRoughness.baseColorFactor = MigrationMaterialUtil.GetBaseColorFactor(vrm0XMaterial);
                    unlitMaterial.pbrMetallicRoughness.baseColorTexture = null;
                    return unlitMaterial;
                case "Unlit/Texture":
                    unlitMaterial.pbrMetallicRoughness.baseColorFactor = new float[] {1, 1, 1, 1};
                    unlitMaterial.pbrMetallicRoughness.baseColorTexture = MigrationMaterialUtil.GetBaseColorTexture(vrm0XMaterial);
                    return unlitMaterial;
                case "Unlit/Transparent":
                    unlitMaterial.pbrMetallicRoughness.baseColorFactor = new float[] {1, 1, 1, 1};
                    unlitMaterial.pbrMetallicRoughness.baseColorTexture = MigrationMaterialUtil.GetBaseColorTexture(vrm0XMaterial);
                    unlitMaterial.alphaMode = "BLEND";
                    return unlitMaterial;
                case "Unlit/Transparent Cutout":
                    unlitMaterial.pbrMetallicRoughness.baseColorFactor = new float[] {1, 1, 1, 1};
                    unlitMaterial.pbrMetallicRoughness.baseColorTexture = MigrationMaterialUtil.GetBaseColorTexture(vrm0XMaterial);
                    unlitMaterial.alphaMode = "MASK";
                    unlitMaterial.alphaCutoff = MigrationMaterialUtil.GetCutoff(vrm0XMaterial);
                    return unlitMaterial;
                case "VRM/UnlitTexture":
                    unlitMaterial.pbrMetallicRoughness.baseColorFactor = new float[] {1, 1, 1, 1};
                    unlitMaterial.pbrMetallicRoughness.baseColorTexture = MigrationMaterialUtil.GetBaseColorTexture(vrm0XMaterial);
                    return unlitMaterial;
                case "VRM/UnlitTransparent":
                    unlitMaterial.pbrMetallicRoughness.baseColorFactor = new float[] {1, 1, 1, 1};
                    unlitMaterial.pbrMetallicRoughness.baseColorTexture = MigrationMaterialUtil.GetBaseColorTexture(vrm0XMaterial);
                    unlitMaterial.alphaMode = "BLEND";
                    return unlitMaterial;
                case "VRM/UnlitCutout":
                    unlitMaterial.pbrMetallicRoughness.baseColorFactor = new float[] {1, 1, 1, 1};
                    unlitMaterial.pbrMetallicRoughness.baseColorTexture = MigrationMaterialUtil.GetBaseColorTexture(vrm0XMaterial);
                    unlitMaterial.alphaMode = "MASK";
                    unlitMaterial.alphaCutoff = MigrationMaterialUtil.GetCutoff(vrm0XMaterial);
                    return unlitMaterial;
                case "VRM/UnlitTransparentZWrite":
                    // NOTE: ZWrite マテリアルのみ、MToon にマイグレーションするため、別処理.
                    return null;
                default:
                    return null;
            }
        }
    }
}