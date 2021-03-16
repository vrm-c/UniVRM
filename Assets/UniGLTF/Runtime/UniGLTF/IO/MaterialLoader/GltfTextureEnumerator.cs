using System.Collections.Generic;

namespace UniGLTF
{
    public delegate IEnumerable<GetTextureParam> TextureEnumerator(glTF gltf);

    public static class GltfTextureEnumerator
    {
        public static IEnumerable<GetTextureParam> EnumerateTextures(glTF gltf, glTFMaterial m)
        {
            if (m.pbrMetallicRoughness != null)
            {
                // base color
                if (m.pbrMetallicRoughness?.baseColorTexture != null)
                {
                    yield return PBRMaterialItem.BaseColorTexture(gltf, m);
                }

                // metallic roughness
                if (m.pbrMetallicRoughness?.metallicRoughnessTexture != null)
                {
                    yield return PBRMaterialItem.MetallicRoughnessTexture(gltf, m);
                }
            }

            // emission
            if (m.emissiveTexture != null)
            {
                yield return GetTextureParam.Create(gltf, m.emissiveTexture.index);
            }

            // normal
            if (m.normalTexture != null)
            {
                yield return PBRMaterialItem.NormalTexture(gltf, m);
            }

            // occlusion
            if (m.occlusionTexture != null)
            {
                yield return PBRMaterialItem.OcclusionTexture(gltf, m);
            }
        }

        public static IEnumerable<GetTextureParam> Enumerate(glTF gltf)
        {
            foreach (var material in gltf.materials)
            {
                foreach (var textureInfo in EnumerateTextures(gltf, material))
                {
                    yield return textureInfo;
                }
            }
        }
    }
}
