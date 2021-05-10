using System;
using System.Collections.Generic;
using VRMShaders;


namespace UniGLTF
{
    public delegate IEnumerable<(SubAssetKey Key, TextureImportParam Param)> EnumerateAllTexturesDistinctFunc(GltfParser parser);

    /// <summary>
    /// Texture 生成に関して
    /// Runtimeは LoadImage するだけだが、Editor時には Asset 化するにあたって続きの処理がある。
    ///
    /// * (gltf/glb/vrm-1): AssetImporterContext.AddObjectToAsset(SubAsset) 
    /// * (gltf/glb/vrm-1): ScriptedImporter.GetExternalObjectMap(Extracted) 
    /// * (vrm-0): (Extracted) ScriptedImporter では無いので ScriptedImporter.AddRemap, GetExternalObjectMap が無い 
    ///
    /// AddRemap, GetExternalObjectMap は Dictionary[SourceAssetIdentifier, UnityEngine.Object] に対する API で
    /// SourceAssetIdentifier 型がリソースを識別するキーとなる。
    /// 
    /// gltfTexture から SourceAssetIdentifier を作り出すことで、GetExternalObjectMap との対応関係を作る。
    /// 
    /// [例外]
    /// glTF で外部ファイルを uri 参照する場合
    /// * sRGB 外部ファイルをそのまま使うので SubAsset にしない
    /// * normal 外部ライルをそのまま使うので SubAsset にしない(normalとしてロードするためにAssetImporterの設定は必用)
    /// * metallicRoughnessOcclusion 変換結果を SubAsset 化する
    /// </summary>
    public static class GltfTextureEnumerator
    {
        public static IEnumerable<(SubAssetKey, TextureImportParam)> EnumerateTexturesReferencedByMaterials(GltfParser parser, int i)
        {
            var m = parser.GLTF.materials[i];

            int? metallicRoughnessTexture = default;
            if (m.pbrMetallicRoughness != null)
            {
                // base color
                if (m.pbrMetallicRoughness?.baseColorTexture != null)
                {
                    yield return GltfPBRMaterial.BaseColorTexture(parser, m);
                }

                // metallic roughness
                if (m.pbrMetallicRoughness?.metallicRoughnessTexture != null && m.pbrMetallicRoughness.metallicRoughnessTexture.index != -1)
                {
                    metallicRoughnessTexture = m.pbrMetallicRoughness?.metallicRoughnessTexture?.index;
                }
            }

            // emission
            if (m.emissiveTexture != null)
            {
                yield return GltfPBRMaterial.EmissiveTexture(parser, m);
            }

            // normal
            if (m.normalTexture != null)
            {
                yield return GltfPBRMaterial.NormalTexture(parser, m);
            }

            // occlusion
            int? occlusionTexture = default;
            if (m.occlusionTexture != null && m.occlusionTexture.index != -1)
            {
                occlusionTexture = m.occlusionTexture.index;
            }

            // metallicSmooth and occlusion
            if (metallicRoughnessTexture.HasValue || occlusionTexture.HasValue)
            {
                yield return GltfPBRMaterial.StandardTexture(parser, m);
            }
        }

        /// <summary>
        /// glTF 全体で使うテクスチャーをユニークになるように列挙する
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IEnumerable<(SubAssetKey, TextureImportParam)> EnumerateAllTexturesDistinct(GltfParser parser)
        {
            var usedTextures = new HashSet<SubAssetKey>();
            for (int i = 0; i < parser.GLTF.materials.Count; ++i)
            {
                foreach ((SubAssetKey key, TextureImportParam) kv in EnumerateTexturesReferencedByMaterials(parser, i))
                {
                    if (usedTextures.Add(kv.key))
                    {
                        yield return kv;
                    }
                }
            }
        }
    }
}
