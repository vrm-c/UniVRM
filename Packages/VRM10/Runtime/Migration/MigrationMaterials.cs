using System;
using System.Linq;
using UniGLTF;
using UniJSON;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// VRM 拡張に含まれる Material 情報を用いて、マイグレーションを行う.
    /// 前提として、glTF の仕様の範囲で glTFMaterial は既に読み込み済みであると仮定する.
    /// </summary>
    internal static class MigrationMaterials
    {
        private const string DontUseExtensionShaderName = "VRM_USE_GLTFSHADER";
        private const string MaterialPropertiesKey = "materialProperties";

        public static void Migrate(glTF gltf, JsonNode vrm0)
        {
            var needsDisablingVertexColor = false;
            var vrm0XMaterialList = vrm0[MaterialPropertiesKey].ArrayItems().ToArray();

            try
            {
                // 1. VRM 拡張がついていない PBR Material のマイグレーション.
                MigrationPbrMaterial.Migrate(gltf, vrm0);

                // 2. VRM 拡張のうち、古い Unlit 情報からの取得を試みる.
                if (MigrationLegacyUnlitMaterial.Migrate(gltf, vrm0XMaterialList))
                {
                    // NOTE: 古い Unlit である場合、頂点カラー情報を破棄する.
                    needsDisablingVertexColor = true;
                }

                // 3. VRM 拡張のうち、UnlitTransparentZWrite 情報からの取得を試みる.
                if (MigrationUnlitTransparentZWriteMaterial.Migrate(gltf, vrm0XMaterialList))
                {
                    // NOTE: 古い Unlit である場合、頂点カラー情報を破棄する.
                    needsDisablingVertexColor = true;
                }

                // 4. VRM 拡張のうち、MToon 情報からの取得を試みる.
                // NOTE: MToon だった場合、内部で material.extensions を破棄してしまう.
                MigrationMToonMaterial.Migrate(gltf, vrm0);
            }
            catch (Exception ex)
            {
                UniGLTFLogger.Exception(ex);
            }

            if (needsDisablingVertexColor)
            {
                DisableVertexColor(gltf);
            }
        }

        private static void DisableVertexColor(glTF gltf)
        {
            foreach (var mesh in gltf.meshes)
            {
                foreach (var primitive in mesh.primitives)
                {
                    primitive.attributes.COLOR_0 = -1;
                }
            }
        }
    }
}