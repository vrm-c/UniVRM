using System;
using System.Collections.Generic;
using System.Linq;
using VRMShaders;


namespace UniGLTF
{
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
    public sealed class GltfTextureDescriptorGenerator : ITextureDescriptorGenerator
    {
        private readonly GltfData m_data;
        private TextureDescriptorSet _textureDescriptorSet;

        public GltfTextureDescriptorGenerator(GltfData data)
        {
            m_data = data;
        }

        public TextureDescriptorSet Get()
        {
            if (_textureDescriptorSet == null)
            {
                _textureDescriptorSet = new TextureDescriptorSet();
                foreach (var (_, param) in EnumerateAllTextures(m_data))
                {
                    _textureDescriptorSet.Add(param);
                }
            }
            return _textureDescriptorSet;
        }

        /// <summary>
        /// glTF 全体で使うテクスチャーを列挙。
        /// </summary>
        private static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateAllTextures(GltfData data)
        {
            for (int i = 0; i < data.GLTF.materials.Count; ++i)
            {
                foreach (var kv in GltfPbrTextureImporter.EnumerateAllTextures(data, i))
                {
                    yield return kv;
                }
            }
        }
    }
}
