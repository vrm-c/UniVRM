using System.Collections.Generic;
using UniGLTF;
using VRMShaders;

namespace VRM
{
    public class VRMTextureEnumerator
    {
        readonly glTF_VRM_extensions m_vrm;
        public VRMTextureEnumerator(glTF_VRM_extensions vrm)
        {
            m_vrm = vrm;
        }

        public IEnumerable<TextureImportParam> Enumerate(GltfParser parser)
        {
            for (int i = 0; i < parser.GLTF.materials.Count; ++i)
            {
                var vrmMaterial = m_vrm.materialProperties[i];
                if (vrmMaterial.shader == MToon.Utils.ShaderName)
                {
                    // MToon
                    foreach (var kv in vrmMaterial.textureProperties)
                    {
                        // SRGB color or normalmap
                        yield return TextureFactory.Create(parser, kv.Value, kv.Key, default, default);
                    }
                }
                else
                {
                    // PBR or Unlit
                    foreach (var textureInfo in GltfTextureEnumerator.EnumerateTextures(parser, parser.GLTF.materials[i]))
                    {
                        yield return textureInfo;
                    }
                }
            }

            // thumbnail
            if (m_vrm.meta != null && m_vrm.meta.texture != -1)
            {
                yield return TextureFactory.CreateSRGB(parser, m_vrm.meta.texture);
            }
        }
    }
}
