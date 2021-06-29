using System.Collections.Generic;
using VRMShaders;

namespace UniGLTF
{
    public sealed class GltfPbrTextureImporter
    {
        public static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateAllTextures(GltfData data, int i)
        {
            var m = data.GLTF.materials[i];

            int? metallicRoughnessTexture = default;
            if (m.pbrMetallicRoughness != null)
            {
                // base color
                if (m.pbrMetallicRoughness?.baseColorTexture != null)
                {
                    yield return BaseColorTexture(data, m);
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
                yield return EmissiveTexture(data, m);
            }

            // normal
            if (m.normalTexture != null)
            {
                yield return NormalTexture(data, m);
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
                yield return StandardTexture(data, m);
            }
        }

        public static (SubAssetKey, TextureDescriptor) BaseColorTexture(GltfData data, glTFMaterial src)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
            return GltfTextureImporter.CreateSRGB(data, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale);
        }

        public static (SubAssetKey, TextureDescriptor) StandardTexture(GltfData data, glTFMaterial src)
        {
            var metallicFactor = 1.0f;
            var roughnessFactor = 1.0f;
            if (src.pbrMetallicRoughness != null)
            {
                metallicFactor = src.pbrMetallicRoughness.metallicFactor;
                roughnessFactor = src.pbrMetallicRoughness.roughnessFactor;
            }
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.metallicRoughnessTexture);
            return GltfTextureImporter.CreateStandard(data,
                            src.pbrMetallicRoughness?.metallicRoughnessTexture?.index,
                            src.occlusionTexture?.index,
                            offset, scale,
                            metallicFactor,
                            roughnessFactor);
        }

        public static (SubAssetKey, TextureDescriptor) NormalTexture(GltfData data, glTFMaterial src)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.normalTexture);
            return GltfTextureImporter.CreateNormal(data, src.normalTexture.index, offset, scale);
        }

        public static (SubAssetKey, TextureDescriptor) EmissiveTexture(GltfData data, glTFMaterial src)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.emissiveTexture);
            return GltfTextureImporter.CreateSRGB(data, src.emissiveTexture.index, offset, scale);
        }

    }
}
