using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public sealed class VRMTextureSetImporter : ITextureSetImporter
    {
        private readonly GltfParser m_parser;
        private readonly glTF_VRM_extensions m_vrm;
        private TextureSet _textureSet;

        public VRMTextureSetImporter(GltfParser parser, glTF_VRM_extensions vrm)
        {
            m_parser = parser;
            m_vrm = vrm;
        }

        public TextureSet GetTextureSet()
        {
            if (_textureSet == null)
            {
                _textureSet = new TextureSet();
                foreach (var (_, param) in EnumerateAllTextures(m_parser, m_vrm))
                {
                    _textureSet.Add(param);
                }
            }
            return _textureSet;
        }


        private static IEnumerable<(SubAssetKey, TextureImportParam)> EnumerateAllTextures(GltfParser parser, glTF_VRM_extensions vrm)
        {
            // Materials
            for (var materialIdx = 0; materialIdx < parser.GLTF.materials.Count; ++materialIdx)
            {
                var material = parser.GLTF.materials[materialIdx];
                var vrmMaterial = vrm.materialProperties[materialIdx];

                if (vrmMaterial.shader == MToon.Utils.ShaderName)
                {
                    // MToon
                    foreach (var kv in VRMMToonTextureImporter.EnumerateAllTextures(parser, vrm, materialIdx))
                    {
                        yield return kv;
                    }
                }
                else
                {
                    // Unlit or PBR
                    foreach (var kv in GltfPbrTextureImporter.EnumerateAllTextures(parser, materialIdx))
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

        private static bool TryGetThumbnailTexture(GltfParser parser, glTF_VRM_extensions vrm, out (SubAssetKey, TextureImportParam) texture)
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
