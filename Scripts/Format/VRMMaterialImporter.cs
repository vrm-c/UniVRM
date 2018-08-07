using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using System.Linq;
using System;

namespace VRM
{
    public class VRMMaterialImporter : MaterialImporter
    {
        List<glTF_VRM_Material> m_materials;
        public VRMMaterialImporter(ImporterContext context, List<glTF_VRM_Material> materials) : base(new ShaderStore(context, "VRM/UnlitTexture"), context)
        {
            m_materials = materials;
            /*
            var CreateDefault = MaterialIO.CreateMaterialFuncFromShader(new ShaderStore("VRM/UnlitTexture"));
            var CreateZWrite = MaterialIO.CreateMaterialFuncFromShader(new ShaderStore("VRM/UnlitTransparentZWrite"));
            MaterialIO.CreateMaterialFunc fallback = (ctx, i) =>
            {
                var vrm = ctx.GLTF;
                if (vrm != null && vrm.materials[i].name.ToLower().Contains("zwrite"))
                {
                    // 一応。不要かも
                    Debug.Log("fallback to VRM/UnlitTransparentZWrite");
                    return CreateZWrite(ctx, i);
                }
                else
                {
                    Debug.Log("fallback to VRM/UnlitTexture");
                    return CreateDefault(ctx, i);
                }
            };
            if (materials == null && materials.Count == 0)
            {
                return fallback;
            }
            */
        }

        static string[] VRM_SHADER_NAMES =
        {
            "Standard",
            "VRM/UnlitTexture",
            "VRM/UnlitCutout",
            "VRM/UnlitTransparent",
            "VRM/UnlitTransparentZWrite",
            "VRM/MToon",
        };

        public override Material CreateMaterial(int i, glTFMaterial src)
        {
            var item = m_materials[i];
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
                    return base.CreateMaterial(i, src);
                }
                else
                {
                    Debug.LogWarningFormat("unknown shader {0}.", shaderName);
                    return base.CreateMaterial(i, src);
                }
            }

            //
            // restore VRM material
            //
            var material = new Material(shader);
            material.name = item.name;
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
                var texture = Context.GetTexture(kv.Value);
                if (texture != null) {
                    material.SetTexture(kv.Key, texture.Texture);
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

            return material;
        }
    }
}
