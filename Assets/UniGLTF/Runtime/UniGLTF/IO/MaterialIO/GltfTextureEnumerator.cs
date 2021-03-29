using System.Collections.Generic;
using VRMShaders;


namespace UniGLTF
{
    public delegate IEnumerable<TextureImportParam> TextureEnumerator(GltfParser parser);

    /// <summary>
    /// Texture 生成に関して
    /// Runtimeは LoadImage するだけだが、Editor時には Asset 化するにあたって続きの処理がある。
    ///
    /// * (gltf/glb/vrm-1): AssetImporterContext.AddObjectToAsset(SubAsset) 
    /// * (gltf/glb/vrm-1): ScriptedImporter.GetExternalObjectMap(Extracted) 
    /// * (vrm-0): (Extracted) ScriptedImporter では無いので ScriptedImporter.AddRemap が無い 
    ///
    /// Extract は外部ファイルに png/jpg のバイト列を出力して、TextureImporter を設定すること。ScriptedImporter.AddRemap
    /// Extracted は、
    /// 
    /// ファイル名もしくはSubAsset名を介してテクスチャーアセットにアクセスるので、文字列をユニークなキーとしてテクスチャーを識別できる必要がある。
    /// 基本的に、gltf.textures と Texture2D が１対１に対応するので、 gltfTexture.name のユニーク性を確保した上で
    /// これを用いればよいが以下の例外がある。
    ///
    /// * PBR の MetallicSmoothness と Occlusion が合体する場合
    /// * (gltf)外部テクスチャーファイルの uri 参照が同じになる場合(同じイメージファイルが異なるテクスチャー設定を保持するケースをサポートしない)
    ///   * 異なる gltfTexture.source が 同じ gltfImage を参照する場合
    ///   * 異なる gltfImage.uri が 同じ ファイルを参照する場合
    ///
    /// 例外に対処した上で、ユニークなテクスチャー生成情報を列挙するのが
    /// 
    /// GltfTextureEnumerator.Enumerate
    ///
    /// である。
    /// </summary>
    public static class GltfTextureEnumerator
    {
        public static IEnumerable<TextureImportParam> EnumerateTextures(GltfParser parser, glTFMaterial m)
        {
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
                var (offset, scale) = GltfMaterialImporter.GetTextureOffsetAndScale(m.emissiveTexture);
                yield return GltfTextureImporter.CreateSRGB(parser, m.emissiveTexture.index, offset, scale);
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

        public static IEnumerable<TextureImportParam> Enumerate(GltfParser parser)
        {
            var used = new HashSet<string>();
            foreach (var material in parser.GLTF.materials)
            {
                foreach (var textureInfo in EnumerateTextures(parser, material))
                {
                    if(used.Add(textureInfo.ExtractKey)){
                        yield return textureInfo;
                    }
                }
            }
        }
    }
}
