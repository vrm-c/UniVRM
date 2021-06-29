using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public static class VRMMToonMaterialImporter
    {
        public static bool TryCreateParam(GltfData data, glTF_VRM_extensions vrm, int materialIdx, out MaterialDescriptor matDesc)
        {
            if (vrm?.materialProperties == null || vrm.materialProperties.Count == 0)
            {
                matDesc = default;
                return false;
            }
            if (materialIdx < 0 || materialIdx >= vrm.materialProperties.Count)
            {
                matDesc = default;
                return false;
            }

            var vrmMaterial = vrm.materialProperties[materialIdx];
            if (vrmMaterial.shader == VRM.glTF_VRM_Material.VRM_USE_GLTFSHADER)
            {
                // fallback to gltf
                matDesc = default;
                return false;
            }

            //
            // restore VRM material
            //
            // use material.name, because material name may renamed in GltfParser.
            var name = data.GLTF.materials[materialIdx].name;
            matDesc = new MaterialDescriptor(name, vrmMaterial.shader);

            matDesc.RenderQueue = vrmMaterial.renderQueue;

            foreach (var kv in vrmMaterial.floatProperties)
            {
                matDesc.FloatValues.Add(kv.Key, kv.Value);
            }

            foreach (var kv in vrmMaterial.vectorProperties)
            {
                // vector4 exclude TextureOffsetScale
                if (!vrmMaterial.textureProperties.ContainsKey(kv.Key))
                {
                    var v = new Vector4(kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3]);
                    matDesc.Vectors.Add(kv.Key, v);
                }
            }

            foreach (var kv in vrmMaterial.textureProperties)
            {
                if (VRMMToonTextureImporter.TryGetTextureFromMaterialProperty(data, vrmMaterial, kv.Key, out var texture))
                {
                    matDesc.TextureSlots.Add(kv.Key, texture.Item2);
                }
            }

            foreach (var kv in vrmMaterial.keywordMap)
            {
                if (kv.Value)
                {
                    matDesc.Actions.Add(material => material.EnableKeyword(kv.Key));
                }
                else
                {
                    matDesc.Actions.Add(material => material.DisableKeyword(kv.Key));
                }
            }

            foreach (var kv in vrmMaterial.tagMap)
            {
                matDesc.Actions.Add(material => material.SetOverrideTag(kv.Key, kv.Value));
            }

            if (vrmMaterial.shader == MToon.Utils.ShaderName)
            {
                // TODO: Material拡張にMToonの項目が追加されたら旧バージョンのshaderPropから変換をかける
                // インポート時にUniVRMに含まれるMToonのバージョンに上書きする
                matDesc.FloatValues[MToon.Utils.PropVersion] = MToon.Utils.VersionNumber;
            }

            return true;
        }

    }
}
