using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public sealed class VrmTextureDescriptorGenerator : ITextureDescriptorGenerator
    {
        private readonly GltfData m_data;
        private readonly glTF_VRM_extensions m_vrm;
        private TextureDescriptorSet _textureDescriptorSet;

        public VrmTextureDescriptorGenerator(GltfData data, glTF_VRM_extensions vrm)
        {
            m_data = data;
            m_vrm = vrm;
        }

        public TextureDescriptorSet Get()
        {
            if (_textureDescriptorSet == null)
            {
                _textureDescriptorSet = new TextureDescriptorSet();
                foreach (var (_, param) in EnumerateAllTextures(m_data, m_vrm))
                {
                    _textureDescriptorSet.Add(param);
                }
            }
            return _textureDescriptorSet;
        }


        private static IEnumerable<(SubAssetKey, TextureDescriptor)> EnumerateAllTextures(GltfData data, glTF_VRM_extensions vrm)
        {
            // Materials
            for (var materialIdx = 0; materialIdx < data.GLTF.materials.Count; ++materialIdx)
            {
                var material = data.GLTF.materials[materialIdx];
                var vrmMaterial = vrm.materialProperties[materialIdx];

                if (vrmMaterial.shader == VRM.glTF_VRM_Material.VRM_USE_GLTFSHADER)
                {
                    // Unlit or PBR
                    foreach (var kv in GltfPbrTextureImporter.EnumerateAllTextures(data, materialIdx))
                    {
                        yield return kv;
                    }
                }
                else
                {
                    // MToon など任意の shader
                    foreach (var kv in VRMMToonTextureImporter.EnumerateAllTextures(data, vrm, materialIdx))
                    {
                        yield return kv;
                    }
                }
            }

            // Thumbnail
            if (TryGetThumbnailTexture(data, vrm, out var key, out var desc))
            {
                yield return (key, desc);
            }
        }

        private static bool TryGetThumbnailTexture(GltfData data, glTF_VRM_extensions vrm, out SubAssetKey key, out TextureDescriptor desc)
        {
            if (vrm.meta.texture > -1)
            {
                return GltfTextureImporter.TryCreateSrgb(data, vrm.meta.texture, Vector2.zero, Vector2.one, out key, out desc);
            }

            key = default;
            desc = default;
            return false;
        }
    }
}
