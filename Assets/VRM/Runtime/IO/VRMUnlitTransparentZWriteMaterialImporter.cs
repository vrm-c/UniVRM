using System;
using System.Collections.Generic;
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

            var textureSlots = new Dictionary<string, TextureDescriptor>();
            var floatValues = new Dictionary<string, float>();
            var colors = new Dictionary<string, Color>();
            var vectors = new Dictionary<string, Vector4>();
            var actions = new List<Action<Material>>();

            //
            // import as MToon
            //
            matDesc = new MaterialDescriptor(
                name,
                Utils.ShaderName, 
                Utils.GetRenderQueueRequirement(RenderMode.TransparentWithZWrite).DefaultValue,
                textureSlots,
                floatValues,
                colors,
                vectors,
                actions
            );

            // NOTE: Unlit のフォールバックなので、 Lit/Shade 色は黒とし、Emissive Factor に設定する.
            // また、元のシェーダのうちユーザが設定できるプロパティは Texture のみ.
            colors[Utils.PropColor] = Color.black;
            colors[Utils.PropShadeColor] = Color.black;
            colors[Utils.PropEmissionColor] = Color.white;

            if (vrmMaterial.textureProperties.ContainsKey(Utils.PropMainTex))
            {
                if (VRMMToonTextureImporter.TryGetTextureFromMaterialProperty(data, vrmMaterial, Utils.PropMainTex, out var texture))
                {
                    textureSlots.Add(Utils.PropEmissionMap, texture.Item2);
                }
            }

            actions.Add(unityMaterial =>
            {
                // NOTE: ZWrite などの属性は util に設定させる.
                var parameter = Utils.GetMToonParametersFromMaterial(unityMaterial);
                parameter.Rendering.CullMode = CullMode.Back;
                parameter.Rendering.RenderMode = RenderMode.TransparentWithZWrite;
                parameter.Rendering.RenderQueueOffsetNumber = 0;
                Utils.SetMToonParametersToMaterial(unityMaterial, parameter);
            });

            if (vrmMaterial.shader == Utils.ShaderName)
            {
                // TODO: Material拡張にMToonの項目が追加されたら旧バージョンのshaderPropから変換をかける
                // インポート時にUniVRMに含まれるMToonのバージョンに上書きする
                floatValues[Utils.PropVersion] = Utils.VersionNumber;
            }

            actions.Add(m =>
            {
                m.SetFloat(Utils.PropBlendMode, (float)RenderMode.TransparentWithZWrite);
                Utils.ValidateProperties(m, true);
            });

            return true;
        }
    }
}
