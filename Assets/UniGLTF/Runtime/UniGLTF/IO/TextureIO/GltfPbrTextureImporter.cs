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
                    if (TryBaseColorTexture(data, m, out var value))
                    {
                        yield return value;
                    }
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
                if (TryEmissiveTexture(data, m, out var value))
                {
                    yield return value;
                }
            }

            // normal
            if (m.normalTexture != null)
            {
                if (TryNormalTexture(data, m, out var value))
                {
                    yield return value;
                }
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
                if (TryStandardTexture(data, m, out var value))
                {
                    yield return value;
                }
            }
        }

        public static bool TryBaseColorTexture(GltfData data, glTFMaterial src, out (SubAssetKey, TextureDescriptor) value)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.baseColorTexture);
            return GltfTextureImporter.TryCreateSrgb(data, src.pbrMetallicRoughness.baseColorTexture.index, offset, scale, out value);
        }

        public static bool TryStandardTexture(GltfData data, glTFMaterial src, out (SubAssetKey, TextureDescriptor) value)
        {
            var metallicFactor = 1.0f;
            var roughnessFactor = 1.0f;
            if (src.pbrMetallicRoughness != null)
            {
                metallicFactor = src.pbrMetallicRoughness.metallicFactor;
                roughnessFactor = src.pbrMetallicRoughness.roughnessFactor;
            }
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.pbrMetallicRoughness.metallicRoughnessTexture);
            return GltfTextureImporter.TryCreateStandard(data,
                            src.pbrMetallicRoughness?.metallicRoughnessTexture?.index,
                            src.occlusionTexture?.index,
                            offset, scale,
                            metallicFactor,
                            roughnessFactor, out value);
        }

        public static bool TryNormalTexture(GltfData data, glTFMaterial src, out (SubAssetKey, TextureDescriptor) value)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.normalTexture);
            return GltfTextureImporter.TryCreateNormal(data, src.normalTexture.index, offset, scale, out value);
        }

        public static bool TryEmissiveTexture(GltfData data, glTFMaterial src, out (SubAssetKey, TextureDescriptor) value)
        {
            var (offset, scale) = GltfTextureImporter.GetTextureOffsetAndScale(src.emissiveTexture);
            return GltfTextureImporter.TryCreateSrgb(data, src.emissiveTexture.index, offset, scale, out value);
        }
    }
}
