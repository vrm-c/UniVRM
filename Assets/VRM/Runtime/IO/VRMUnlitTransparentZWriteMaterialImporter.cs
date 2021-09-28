using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public static class VRMUnlitTransparentZWriteMaterialImporter
    {
        public const string ShaderName = "VRM/UnlitTransparentZWrite";

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
            if (vrmMaterial.shader != ShaderName)
            {
                // fallback to gltf
                matDesc = default;
                return false;
            }

            // use material.name, because material name may renamed in GltfParser.
            var name = data.GLTF.materials[materialIdx].name;

            //
            // import as MToon
            //
            matDesc = new MaterialDescriptor(name, MToon.Utils.ShaderName);

            matDesc.RenderQueue = vrmMaterial.renderQueue;

            if (vrmMaterial.textureProperties.ContainsKey(MToon.Utils.PropMainTex))
            {
                if (VRMMToonTextureImporter.TryGetTextureFromMaterialProperty(data, vrmMaterial, MToon.Utils.PropMainTex, out var texture))
                {
                    matDesc.TextureSlots.Add(MToon.Utils.PropMainTex, texture.Item2);
                    matDesc.TextureSlots.Add(MToon.Utils.PropShadeTexture, texture.Item2);
                }
            }

            matDesc.Colors[MToon.Utils.PropColor] = Color.white;
            matDesc.Colors[MToon.Utils.PropShadeColor] = Color.white;

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

            matDesc.Actions.Add(m =>
            {
                m.SetFloat(MToon.Utils.PropBlendMode, (float)MToon.RenderMode.TransparentWithZWrite);
                MToon.Utils.ValidateProperties(m, true);
            });

            return true;
        }
    }
}
