using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public sealed class VrmTextureDescriptorGenerator : ITextureDescriptorGenerator
    {
        private readonly GltfParser m_parser;
        private readonly glTF_VRM_extensions m_vrm;
        private TextureDescriptorSet _textureDescriptorSet;

        public VrmTextureDescriptorGenerator(GltfParser parser, glTF_VRM_extensions vrm)
        {
            m_parser = parser;
            m_vrm = vrm;
        }

        public TextureDescriptorSet Get()
        {
            if (_textureDescriptorSet == null)
            {
                _textureDescriptorSet = new TextureDescriptorSet();
                foreach (var (_, param) in EnumerateAllTextures(m_parser, m_vrm))
                {
                    _textureDescriptorSet.Add(param);
                }
            }
            return _textureDescriptorSet;
        }


        private static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateAllTextures(GltfParser parser, glTF_VRM_extensions vrm)
        {
            // Materials
            for (var materialIdx = 0; materialIdx < parser.GLTF.materials.Count; ++materialIdx)
            {
                var material = parser.GLTF.materials[materialIdx];
                var vrmMaterial = vrm.materialProperties[materialIdx];

                if (vrmMaterial.shader == VRM.glTF_VRM_Material.VRM_USE_GLTFSHADER)
                {
                    // Unlit or PBR
                    foreach (var kv in GltfPbrTextureImporter.EnumerateAllTextures(parser, materialIdx))
                    {
                        yield return kv;
                    }
                }
                else
                {
                    // MToon など任意の shader
                    foreach (var kv in VRMMToonTextureImporter.EnumerateAllTextures(parser, vrm, materialIdx))
                    {
                        yield return kv;
                    }
                }
            }

            // Thumbnail
            if (TryGetThumbnailTexture(parser, vrm, out var thumbnail))
            {
                yield return thumbnail;
            }
        }

        private static bool TryGetThumbnailTexture(GltfParser parser, glTF_VRM_extensions vrm, out (SubAssetKey, TextureDescriptor) texture)
        {
            if (vrm.meta.texture > -1)
            {
                texture = GltfTextureImporter.CreateSRGB(parser, vrm.meta.texture, Vector2.zero, Vector2.one);
                return true;
            }

            texture = default;
            return false;
        }
    }
}
