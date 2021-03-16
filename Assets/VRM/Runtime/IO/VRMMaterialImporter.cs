using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace VRM
{
    public static class MToonMaterialItem
    {
        static string[] VRM_SHADER_NAMES =
        {
            "Standard",
            "VRM/MToon",
            "UniGLTF/UniUnlit",

            "VRM/UnlitTexture",
            "VRM/UnlitCutout",
            "VRM/UnlitTransparent",
            "VRM/UnlitTransparentZWrite",
        };

        public static async Task<Material> CreateAsync(IAwaitCaller awaitCaller, glTF gltf, int m_index, glTF_VRM_Material vrmMaterial, GetTextureAsyncFunc getTexture)
        {
            var item = vrmMaterial;
            var shaderName = item.shader;
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                //
                // no shader
                //
                if (VRM_SHADER_NAMES.Contains(shaderName))
                {
                    Debug.LogErrorFormat("shader {0} not found. set Assets/VRM/Shaders/VRMShaders to Edit - project setting - Graphics - preloaded shaders", shaderName);
                }
                else
                {
                    // #if VRM_DEVELOP                    
                    //                     Debug.LogWarningFormat("unknown shader {0}.", shaderName);
                    // #endif                    
                }
                return await MaterialFactory.DefaultCreateMaterialAsync(awaitCaller, gltf, m_index, getTexture);
            }

            //
            // restore VRM material
            //
            var material = new Material(shader);
            // use material.name, because material name may renamed in GltfParser.
            material.name = gltf.materials[m_index].name;
            material.renderQueue = item.renderQueue;

            foreach (var kv in item.floatProperties)
            {
                material.SetFloat(kv.Key, kv.Value);
            }
            foreach (var kv in item.vectorProperties)
            {
                if (item.textureProperties.ContainsKey(kv.Key))
                {
                    // texture offset & scale
                    material.SetTextureOffset(kv.Key, new Vector2(kv.Value[0], kv.Value[1]));
                    material.SetTextureScale(kv.Key, new Vector2(kv.Value[2], kv.Value[3]));
                }
                else
                {
                    // vector4
                    var v = new Vector4(kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3]);
                    material.SetVector(kv.Key, v);
                }
            }
            foreach (var kv in item.textureProperties)
            {
                var param = GetTextureParam.Create(gltf, kv.Value, kv.Key);
                var texture = await getTexture(awaitCaller, gltf, param);
                if (texture != null)
                {
                    material.SetTexture(kv.Key, texture);
                }
            }
            foreach (var kv in item.keywordMap)
            {
                if (kv.Value)
                {
                    material.EnableKeyword(kv.Key);
                }
                else
                {
                    material.DisableKeyword(kv.Key);
                }
            }
            foreach (var kv in item.tagMap)
            {
                material.SetOverrideTag(kv.Key, kv.Value);
            }

            if (shaderName == MToon.Utils.ShaderName)
            {
                // TODO: Material拡張にMToonの項目が追加されたら旧バージョンのshaderPropから変換をかける
                // インポート時にUniVRMに含まれるMToonのバージョンに上書きする
                material.SetFloat(MToon.Utils.PropVersion, MToon.Utils.VersionNumber);
            }

            return material;
        }
    }

    public class VRMMaterialImporter
    {
        List<glTF_VRM_Material> m_materials;
        public VRMMaterialImporter(List<glTF_VRM_Material> materials)
        {
            m_materials = materials;
        }

        public Task<Material> CreateMaterialAsync(IAwaitCaller awaitCaller, glTF gltf, int i, GetTextureAsyncFunc getTexture)
        {
            if (i == 0 && m_materials.Count == 0)
            {
                // dummy
                return MaterialFactory.DefaultCreateMaterialAsync(awaitCaller, gltf, i, getTexture);
            }

            return MToonMaterialItem.CreateAsync(awaitCaller, gltf, i, m_materials[i], getTexture);
        }
    }
}
