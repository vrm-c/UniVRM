using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public class VRMMaterialImporter
    {
        readonly glTF_VRM_extensions m_vrm;
        public VRMMaterialImporter(glTF_VRM_extensions vrm)
        {
            m_vrm = vrm;
        }

        public bool TryCreateParam(GltfParser parser, int i, out MaterialImportParam param)
        {
            var vrmMaterial = m_vrm.materialProperties[i];
            if (vrmMaterial.shader == VRM.glTF_VRM_Material.VRM_USE_GLTFSHADER)
            {
                // fallback to gltf
                param = default;
                return false;
            }

            //
            // restore VRM material
            //
            // use material.name, because material name may renamed in GltfParser.
            var name = parser.GLTF.materials[i].name;
            param = new MaterialImportParam(name, vrmMaterial.shader);

            param.RenderQueue = vrmMaterial.renderQueue;

            foreach (var kv in vrmMaterial.floatProperties)
            {
                param.FloatValues.Add(kv.Key, kv.Value);
            }

            var offsetScaleMap = new Dictionary<string, float[]>();
            foreach (var kv in vrmMaterial.vectorProperties)
            {
                if (vrmMaterial.textureProperties.ContainsKey(kv.Key))
                {
                    // texture offset & scale
                    offsetScaleMap.Add(kv.Key, kv.Value);
                }
                else
                {
                    // vector4
                    var v = new Vector4(kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3]);
                    param.Vectors.Add(kv.Key, v);
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

                var (key, textureParam) = MToonTextureParam.Create(parser, kv.Value, offset, scale, kv.Key, 1, 1);
                param.TextureSlots.Add(kv.Key, textureParam);
            }

            foreach (var kv in vrmMaterial.keywordMap)
            {
                if (kv.Value)
                {
                    param.Actions.Add(material => material.EnableKeyword(kv.Key));
                }
                else
                {
                    param.Actions.Add(material => material.DisableKeyword(kv.Key));
                }
            }

            foreach (var kv in vrmMaterial.tagMap)
            {
                param.Actions.Add(material => material.SetOverrideTag(kv.Key, kv.Value));
            }

            if (vrmMaterial.shader == MToon.Utils.ShaderName)
            {
                // TODO: Material拡張にMToonの項目が追加されたら旧バージョンのshaderPropから変換をかける
                // インポート時にUniVRMに含まれるMToonのバージョンに上書きする
                param.FloatValues[MToon.Utils.PropVersion] = MToon.Utils.VersionNumber;
            }

            return true;
        }

        public MaterialImportParam GetMaterialParam(GltfParser parser, int i)
        {
            // mtoon
            if (!TryCreateParam(parser, i, out MaterialImportParam param))
            {
                // unlit
                if (!GltfUnlitMaterial.TryCreateParam(parser, i, out param))
                {
                    // pbr
                    GltfPBRMaterial.TryCreateParam(parser, i, out param);
                }
            }
            return param;
        }

        public IEnumerable<(SubAssetKey, TextureImportParam)> EnumerateAllTexturesDistinct(GltfParser parser)
        {
            var used = new HashSet<SubAssetKey>();

            for (int i = 0; i < parser.GLTF.materials.Count; ++i)
            {
                var vrmMaterial = m_vrm.materialProperties[i];
                if (vrmMaterial.shader == MToon.Utils.ShaderName)
                {
                    // MToon
                    if (!TryCreateParam(parser, i, out MaterialImportParam param))
                    {
                        throw new Exception();
                    }
                    foreach (var (key, value) in param.EnumerateSubAssetKeyValue())
                    {
                        if (used.Add(key))
                        {
                            yield return (key, value);
                        }
                    }
                }
                else
                {
                    // PBR or Unlit
                    foreach (var (key, value) in GltfTextureEnumerator.EnumerateTexturesReferencedByMaterials(parser, i))
                    {
                        if (used.Add(key))
                        {
                            yield return (key, value);
                        }
                    }
                }
            }

            // thumbnail
            if (m_vrm.meta != null && m_vrm.meta.texture != -1)
            {
                var (key, value) = GltfTextureImporter.CreateSRGB(parser, m_vrm.meta.texture, Vector2.zero, Vector2.one);
                if (used.Add(key))
                {
                    yield return (key, value);
                }
            }
        }
    }
}
