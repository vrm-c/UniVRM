using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
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
                    var offsetScaleMap = new Dictionary<string, float[]>();
                    foreach (var kv in vrmMaterial.vectorProperties)
                    {
                        if (vrmMaterial.textureProperties.ContainsKey(kv.Key))
                        {
                            // texture offset & scale
                            offsetScaleMap.Add(kv.Key, kv.Value);
                        }
                    }
                    foreach (var kv in vrmMaterial.textureProperties)
                    {
                        var (offset, scale) = (Vector2.zero, Vector2.one);
                        if (offsetScaleMap.TryGetValue(kv.Key, out float[] value))
                        {
                            offset = new Vector2(value[0], value[1]);
                            scale = new Vector2(value[2], value[3]);
                        }
                        
                        // SRGB color or normalmap
                        yield return VRMTextureParam.Create(parser, kv.Value, offset, scale, kv.Key, default, default);
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
                yield return TextureFactory.CreateSRGB(parser, m_vrm.meta.texture, Vector2.zero, Vector2.one);
            }
        }
    }
}
