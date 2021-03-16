using System.Collections.Generic;
using UniGLTF;

namespace VRM
{
    public class VRMTextureEnumerator
    {
        readonly glTF_VRM_extensions m_vrm;
        public VRMTextureEnumerator(glTF_VRM_extensions vrm)
        {
            m_vrm = vrm;
        }

        public IEnumerable<GetTextureParam> Enumerate(glTF gltf)
        {
            for (int i = 0; i < gltf.materials.Count; ++i)
            {
                var vrmMaterial = m_vrm.materialProperties[i];
                if (vrmMaterial.shader == MToon.Utils.ShaderName)
                {
                    // MToon
                    foreach (var kv in vrmMaterial.textureProperties)
                    {
                        // SRGB color or normalmap
                        yield return GetTextureParam.Create(gltf, kv.Value, kv.Key);
                    }
                }
                else
                {
                    // PBR or Unlit
                    foreach (var textureInfo in GltfTextureEnumerator.EnumerateTextures(gltf, gltf.materials[i]))
                    {
                        yield return textureInfo;
                    }
                }
            }

            // thumbnail
            if (m_vrm.meta != null && m_vrm.meta.texture != -1)
            {
                yield return GetTextureParam.Create(gltf, m_vrm.meta.texture);
            }
        }
    }
}
