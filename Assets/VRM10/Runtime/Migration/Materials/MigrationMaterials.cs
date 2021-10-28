using System;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    public static class MigrationMaterials
    {
        private const string DontUseExtensionShaderName = "VRM_USE_GLTFSHADER";
        private const string MaterialPropertiesKey = "materialProperties";

        public static void Migrate(glTF gltf, JsonNode vrm0)
        {
            for (var materialIdx = 0; materialIdx < gltf.materials.Count; ++materialIdx)
            {
                try
                {
                    var vrm0XMaterial = vrm0[MaterialPropertiesKey][materialIdx];
                    var shaderName = MigrationMaterialUtil.GetShaderName(vrm0XMaterial);

                    // 1. material 拡張を使わない場合.
                    // NOTE: この ShaderName の場合、「この拡張に記載されている情報は無視する」ということを示す.
                    if (shaderName == DontUseExtensionShaderName)
                    {
                        continue;
                    }

                    // 2. VRM 拡張のうち、古い Unlit 情報からの取得を試みる.
                    var unlitMaterial = MigrationLegacyUnlitMaterial.Migrate(vrm0XMaterial);
                    if (unlitMaterial != null)
                    {
                        gltf.materials[materialIdx] = unlitMaterial;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            // 3. VRM 拡張のうち、MToon 情報からの取得を試みる.
            MigrationMToonMaterial.Migrate(gltf, vrm0);
        }
    }
}