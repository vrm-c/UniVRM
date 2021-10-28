using System;
using UniGLTF;
using UniJSON;

namespace UniVRM10
{
    /// <summary>
    /// 過去の UniVRM において、KHR_materials_unlit 拡張を使わず、VRM 拡張を用いて Unlit を表現していた Material をマイグレーションする。
    /// </summary>
    public static class MigrationLegacyUnlitMaterial
    {
        public static glTFMaterial Migrate(JsonNode vrm0XMaterial)
        {
            var unlitMaterial = new glTFMaterial
            {
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
                default:
                    return null;
            }
        }
    }
}