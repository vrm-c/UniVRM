using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public static class VRMMToonMaterialImporter
    {
        public static bool TryCreateParam(GltfParser parser, glTF_VRM_extensions vrm, int materialIdx, out MaterialImportParam param)
        {
            var vrmMaterial = vrm.materialProperties[materialIdx];
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
            var name = parser.GLTF.materials[materialIdx].name;
            param = new MaterialImportParam(name, vrmMaterial.shader);

            param.RenderQueue = vrmMaterial.renderQueue;

            foreach (var kv in vrmMaterial.floatProperties)
            {
                param.FloatValues.Add(kv.Key, kv.Value);
            }

            foreach (var kv in vrmMaterial.vectorProperties)
            {
                // vector4 exclude TextureOffsetScale
                if (!vrmMaterial.textureProperties.ContainsKey(kv.Key))
                {
                    var v = new Vector4(kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3]);
                    param.Vectors.Add(kv.Key, v);
                }
            }

            foreach (var kv in vrmMaterial.textureProperties)
            {
                if (VRMMToonTextureImporter.TryGetTextureFromMaterialProperty(parser, vrm, materialIdx, kv.Key, out var texture))
                {
                    param.TextureSlots.Add(kv.Key, texture.Item2);
                }
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

    }
}
