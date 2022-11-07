using System;
using System.Linq;
using UniGLTF;
using UniJSON;
using UnityEngine;
using ColorSpace = VRMShaders.ColorSpace;

namespace UniVRM10
{
    internal static class MigrationPbrMaterial
    {
        private const string UsingGltfMaterialKeywordInVrmExtension = "VRM_USE_GLTFSHADER";
        private const string ExporterVersionKey = "exporterVersion";

        /// <summary>
        /// vanilla の gltf material を対象とする.
        /// KHR_materials_unlit 拡張や VRM.materialProperties で MToon を対象とするマテリアルは処理してはいけない。
        /// </summary>
        public static void Migrate(glTF gltf, JsonNode vrm0XExtension)
        {
            var needMigrationRoughnessTextureValueSquared = false;
            var needMigrationEmissiveFactorGamma = false;
            if (vrm0XExtension.TryGet(ExporterVersionKey, out var vrm0XVersionStringNode))
            {
                var vrm0XVersionString = vrm0XVersionStringNode.GetString();
                if (Vrm0XVersion.ParseVersion(vrm0XVersionString, out var vrm0XVersion))
                {
                    needMigrationRoughnessTextureValueSquared = Vrm0XVersion.IsNewer(
                        new Vrm0XVersion.Version
                        {
                            Major = 0,
                            Minor = 69,
                            Patch = 0,
                            Pre = "",
                        },
                        vrm0XVersion
                    );
                    needMigrationEmissiveFactorGamma = Vrm0XVersion.IsNewer(
                        new Vrm0XVersion.Version
                        {
                            Major = 0,
                            Minor = 107,
                            Patch = 0,
                            Pre = "",
                        },
                        vrm0XVersion
                    );
                }
            }
            for (var idx = 0; idx < gltf.materials.Count; ++idx)
            {
                MigrateMaterial(gltf, vrm0XExtension, idx, needMigrationRoughnessTextureValueSquared, needMigrationEmissiveFactorGamma);
            }
        }

        private static void MigrateMaterial(glTF gltf, JsonNode vrm0XExtension, int idx, bool needMigrationRoughnessTextureValueSquared, bool needMigrationEmissiveFactorGamma)
        {
            var src = gltf.materials[idx];
            var vrm0XMaterial = vrm0XExtension["materialProperties"][idx];

            // NOTE: ignore MToon
            if (MigrationMaterialUtil.GetShaderName(vrm0XMaterial) != UsingGltfMaterialKeywordInVrmExtension) return;
            // NOTE: ignore Unlit
            if (glTF_KHR_materials_unlit.IsEnable(src)) return;

            if (needMigrationRoughnessTextureValueSquared)
            {
                // NOTE: 非常に実装がめんどくさい、かつ刺さるシチュエーションがかなり少ないので放置.
            }

            if (needMigrationEmissiveFactorGamma && src.emissiveFactor != null && src.emissiveFactor.Length == 3)
            {
                var emissiveFactor = new Color(src.emissiveFactor[0], src.emissiveFactor[1], src.emissiveFactor[2]);
                if (UniGLTF.glTF_KHR_materials_emissive_strength.TryGet(src.extensions, out var emissiveStrength))
                {
                    emissiveFactor *= emissiveStrength.emissiveStrength;
                }
                else if (UniGLTF.Extensions.VRMC_materials_hdr_emissiveMultiplier.GltfDeserializer.TryGet(src.extensions, out var ex))
                {
                    if (ex.EmissiveMultiplier != null)
                    {
                        emissiveFactor *= ex.EmissiveMultiplier.Value;
                    }
                }
                // NOTE: 新しい emissive_strength 拡張を書き込むため、拡張をまっさらにする
                var extensionsExport = new glTFExtensionExport();
                src.extensions = extensionsExport;

                var linearEmissiveFactor = emissiveFactor.linear;
                if (linearEmissiveFactor.maxColorComponent > 1)
                {
                    var maxColorComponent = linearEmissiveFactor.maxColorComponent;
                    linearEmissiveFactor /= maxColorComponent;
                    UniGLTF.glTF_KHR_materials_emissive_strength.Serialize(ref src.extensions, maxColorComponent);
                }
                src.emissiveFactor = linearEmissiveFactor.ToFloat3(ColorSpace.Linear, ColorSpace.Linear);
            }
        }
    }
}