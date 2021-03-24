using System.Collections.Generic;
using VRMShaders;

namespace UniGLTF
{
    public delegate IEnumerable<GetTextureParam> TextureEnumerator(GltfParser parser);

    public static class GltfTextureEnumerator
    {
        public static IEnumerable<GetTextureParam> EnumerateTextures(GltfParser parser, glTFMaterial m)
        {
            int? metallicRoughnessTexture = default;
            if (m.pbrMetallicRoughness != null)
            {
                // base color
                if (m.pbrMetallicRoughness?.baseColorTexture != null)
                {
                    yield return PBRMaterialItem.BaseColorTexture(parser, m);
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
                yield return TextureFactory.CreateSRGB(parser, m.emissiveTexture.index);
            }

            // normal
            if (m.normalTexture != null)
            {
                yield return PBRMaterialItem.NormalTexture(parser, m);
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
                yield return PBRMaterialItem.StandardTexture(parser, m);
            }
        }

        public static IEnumerable<GetTextureParam> Enumerate(GltfParser parser)
        {
            var used = new HashSet<GetTextureParam>();
            foreach (var material in parser.GLTF.materials)
            {
                foreach (var textureInfo in EnumerateTextures(parser, material))
                {
                    if(used.Add(textureInfo)){
                        yield return textureInfo;
                    }
                }
            }
        }
    }
}
