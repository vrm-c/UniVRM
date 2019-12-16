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
        public VRMMaterialImporter(ImporterContext context, List<glTF_VRM_Material> materials) : base(new ShaderStore(context), context)
        {
            m_materials = materials;
        }

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

        public override Material CreateMaterial(int i, glTFMaterial src, bool hasVertexColor)
        {
            if(i==0 && m_materials.Count == 0)
            {
                // dummy
                return new Material(Shader.Find("Standard"));
            }

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
                }
                else
                {
                    Debug.LogWarningFormat("unknown shader {0}.", shaderName);
                }
                return base.CreateMaterial(i, src, hasVertexColor);
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
                if (texture != null)
                {
                    var converted = texture.ConvertTexture(kv.Key);
                    if (converted != null)
                    {
                        material.SetTexture(kv.Key, converted);
                    }
                    else
                    {
                        material.SetTexture(kv.Key, texture.Texture);
                    }
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
}
