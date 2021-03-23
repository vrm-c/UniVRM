using System.Collections.Generic;

namespace UniGLTF
{
    public delegate IEnumerable<GetTextureParam> TextureEnumerator(glTF gltf);

    public static class GltfTextureEnumerator
    {
        public static IEnumerable<GetTextureParam> EnumerateTextures(glTF gltf, glTFMaterial m)
        {
            int? metallicRoughnessTexture = default;
            if (m.pbrMetallicRoughness != null)
            {
                // base color
                if (m.pbrMetallicRoughness?.baseColorTexture != null)
                {
                    yield return PBRMaterialItem.BaseColorTexture(gltf, m);
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
                yield return GetTextureParam.CreateSRGB(gltf, m.emissiveTexture.index);
            }

            // normal
            if (m.normalTexture != null)
            {
                yield return PBRMaterialItem.NormalTexture(gltf, m);
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
                yield return PBRMaterialItem.StandardTexture(gltf, m);
            }
        }

        public static IEnumerable<GetTextureParam> Enumerate(glTF gltf)
        {
            var used = new HashSet<GetTextureParam>();
            foreach (var material in gltf.materials)
            {
                foreach (var textureInfo in EnumerateTextures(gltf, material))
                {
                    if(used.Add(textureInfo)){
                        yield return textureInfo;
                    }
                }
            }
        }
    }
}
