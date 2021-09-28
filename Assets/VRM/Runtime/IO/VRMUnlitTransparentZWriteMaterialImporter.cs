using MToon;
using UniGLTF;
using UnityEngine;
using VRMShaders;
using RenderMode = MToon.RenderMode;

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

            matDesc.RenderQueue = MToon.Utils.GetRenderQueueRequirement(RenderMode.TransparentWithZWrite).DefaultValue;

            // NOTE: Unlit のフォールバックなので、 Lit/Shade 色は黒とし、Emissive Factor に設定する.
            // また、元のシェーダのうちユーザが設定できるプロパティは Texture のみ.
            matDesc.Colors[MToon.Utils.PropColor] = Color.black;
            matDesc.Colors[MToon.Utils.PropShadeColor] = Color.black;
            matDesc.Colors[MToon.Utils.PropEmissionColor] = Color.white;

            if (vrmMaterial.textureProperties.ContainsKey(MToon.Utils.PropMainTex))
            {
                if (VRMMToonTextureImporter.TryGetTextureFromMaterialProperty(data, vrmMaterial, MToon.Utils.PropMainTex, out var texture))
                {
                    matDesc.TextureSlots.Add(MToon.Utils.PropEmissionMap, texture.Item2);
                }
            }

            matDesc.Actions.Add(unityMaterial =>
            {
                // NOTE: ZWrite などの属性は util に設定させる.
                var parameter = MToon.Utils.GetMToonParametersFromMaterial(unityMaterial);
                parameter.Rendering.CullMode = CullMode.Back;
                parameter.Rendering.RenderMode = RenderMode.TransparentWithZWrite;
                parameter.Rendering.RenderQueueOffsetNumber = 0;
                MToon.Utils.SetMToonParametersToMaterial(unityMaterial, parameter);
            });

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
