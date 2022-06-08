using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public static class VRMMToonMaterialImporter
    {
        static string[] MToonTextureSlots = new string[]{
            "_MainTex",
            "_ShadeTexture",
            "_BumpMap",
            "_EmissionMap",
            "_OutlineWidthTexture",
            "_ReceiveShadowTexture",
            "_RimTexture",
            "_ShadingGradeTexture",
            "_SphereAdd",
            "_UvAnimMaskTexture",
        };

        public static bool TryCreateParam(GltfData data, glTF_VRM_extensions vrm, int materialIdx,
            out MaterialDescriptor matDesc)
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
            if (vrmMaterial.shader == glTF_VRM_Material.VRM_USE_GLTFSHADER)
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

            var textureSlots = new Dictionary<string, TextureDescriptor>();
            var floatValues = new Dictionary<string, float>();
            var colors = new Dictionary<string, Color>();
            var vectors = new Dictionary<string, Vector4>();
            var actions = new List<Action<Material>>();
            matDesc = new MaterialDescriptor(
                name,
                vrmMaterial.shader,
                vrmMaterial.renderQueue,
                textureSlots,
                floatValues,
                colors,
                vectors,
                actions);

            foreach (var kv in vrmMaterial.floatProperties)
            {
                floatValues.Add(kv.Key, kv.Value);
            }

            foreach (var kv in vrmMaterial.vectorProperties)
            {
                // vector4 exclude TextureOffsetScale
                if (MToonTextureSlots.Contains(kv.Key))
                {
                    continue;
                }
                var v = new Vector4(kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3]);
                vectors.Add(kv.Key, v);
            }

            foreach (var kv in vrmMaterial.textureProperties)
            {
                if (VRMMToonTextureImporter.TryGetTextureFromMaterialProperty(data, vrmMaterial, kv.Key,
                    out var texture))
                {
                    textureSlots.Add(kv.Key, texture.Item2);
                }
            }

            foreach (var kv in vrmMaterial.keywordMap)
            {
                if (kv.Value)
                {
                    actions.Add(material => material.EnableKeyword(kv.Key));
                }
                else
                {
                    actions.Add(material => material.DisableKeyword(kv.Key));
                }
            }

            foreach (var kv in vrmMaterial.tagMap)
            {
                actions.Add(material => material.SetOverrideTag(kv.Key, kv.Value));
            }

            if (vrmMaterial.shader == MToon.Utils.ShaderName)
            {
                // TODO: Material拡張にMToonの項目が追加されたら旧バージョンのshaderPropから変換をかける
                // インポート時にUniVRMに含まれるMToonのバージョンに上書きする
                floatValues[MToon.Utils.PropVersion] = MToon.Utils.VersionNumber;
            }

            return true;
        }
    }
}