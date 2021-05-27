using System.Collections.Generic;
using VRMShaders;

namespace UniGLTF
{
    public sealed class GltfPbrTextureImporter
    {
        public static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateAllTextures(GltfParser parser, int i)
        {
            var m = parser.GLTF.materials[i];

            int? metallicRoughnessTexture = default;
            if (m.pbrMetallicRoughness != null)
            {
                // base color
                if (m.pbrMetallicRoughness?.baseColorTexture != null)
                {
                    yield return BaseColorTexture(parser, m);
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
                yield return EmissiveTexture(parser, m);
            }

            // normal
            if (m.normalTexture != null)
            {
                yield return NormalTexture(parser, m);
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
                yield return StandardTexture(parser, m);
            }
        }

        public static (SubAssetKey, TextureDescriptor) BaseColorTexture(GltfParser parser, glTFMaterial src)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
            return GltfTextureImporter.CreateSRGB(parser, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale);
        }

        public static (SubAssetKey, TextureDescriptor) StandardTexture(GltfParser parser, glTFMaterial src)
        {
            var metallicFactor = 1.0f;
            var roughnessFactor = 1.0f;
            if (src.pbrMetallicRoughness != null)
            {
                metallicFactor = src.pbrMetallicRoughness.metallicFactor;
                roughnessFactor = src.pbrMetallicRoughness.roughnessFactor;
            }
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.metallicRoughnessTexture);
            return GltfTextureImporter.CreateStandard(parser,
                            src.pbrMetallicRoughness?.metallicRoughnessTexture?.index,
                            src.occlusionTexture?.index,
                            offset, scale,
                            metallicFactor,
                            roughnessFactor);
        }

        public static (SubAssetKey, TextureDescriptor) NormalTexture(GltfParser parser, glTFMaterial src)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.normalTexture);
            return GltfTextureImporter.CreateNormal(parser, src.normalTexture.index, offset, scale);
        }

        public static (SubAssetKey, TextureDescriptor) EmissiveTexture(GltfParser parser, glTFMaterial src)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.emissiveTexture);
            return GltfTextureImporter.CreateSRGB(parser, src.emissiveTexture.index, offset, scale);
        }

    }
}
