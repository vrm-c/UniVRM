using System.Collections.Generic;
using System.Linq;
using UniGLTF;

namespace UniVRM10
{
    public sealed class Vrm10TextureDescriptorGenerator : ITextureDescriptorGenerator
    {
        public const string UniqueThumbnailName = "thumbnail__VRM10";

        private readonly GltfData m_data;
        private TextureDescriptorSet _textureDescriptorSet;

        public Vrm10TextureDescriptorGenerator(GltfData data)
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
        /// glTF 全体で使うテクスチャーを列挙する
        /// </summary>
        private static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateAllTextures(GltfData data)
        {
            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(data.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm))
            {
                throw new System.Exception("not vrm");
            }

            // Textures referenced by Materials.
            for (var materialIdx = 0; materialIdx < data.GLTF.materials.Count; ++materialIdx)
            {
                var m = data.GLTF.materials[materialIdx];
                if (UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(m.extensions, out var mToon))
                {
                    foreach (var (_, tex) in Vrm10MToonTextureImporter.EnumerateAllTextures(data, m, mToon))
                    {
                        yield return tex;
                    }
                }
                else
                {
                    // Fallback to glTF PBR & glTF Unlit
                    foreach (var tex in GltfPbrTextureImporter.EnumerateAllTextures(data, materialIdx))
                    {
                        yield return tex;
                    }
                }
            }

            // Thumbnail Texture referenced by VRM Meta.
            if (TryGetMetaThumbnailTextureImportParam(data, vrm, out (SubAssetKey key, TextureDescriptor) thumbnail))
            {
                yield return thumbnail;
            }
        }

        /// <summary>
        /// VRM-1 の thumbnail テクスチャー。gltf.textures ではなく gltf.images の参照であることに注意(sampler等の設定が無い)
        /// </summary>
        public static bool TryGetMetaThumbnailTextureImportParam(GltfData data, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm, out (SubAssetKey, TextureDescriptor) value)
        {
            if (vrm?.Meta?.ThumbnailImage == null)
            {
                value = default;
                return false;
            }
            var thumbnailImage = vrm.Meta.ThumbnailImage;
            if (!thumbnailImage.HasValue)
            {
                value = default;
                return false;
            }
            var imageIndex = thumbnailImage.Value;
            if (imageIndex < 0 || imageIndex >= data.GLTF.images.Count)
            {
                value = default;
                return false;
            }

            var gltfImage = data.GLTF.images[imageIndex];

            // data.GLTF.textures は前処理によりユニーク性がある
            // unique な名前を振り出す
            var used = new HashSet<string>(data.GLTF.textures.Select(x => x.name));
            var uniqueName = GlbLowLevelParser.FixNameUnique(used, UniqueThumbnailName);

            value = GltfTextureImporter.CreateSrgbFromOnlyImage(data, imageIndex, uniqueName, gltfImage.uri);
            return true;
        }
    }
}
