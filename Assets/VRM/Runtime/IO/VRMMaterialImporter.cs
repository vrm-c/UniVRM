using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace VRM
{
    public class MToonMaterialItem : MaterialItemBase
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

        glTF_VRM_Material m_vrmMaterial;
        bool m_hasVertexColor;

        public MToonMaterialItem(int i, glTFMaterial src, bool hasVertexColor, glTF_VRM_Material vrmMaterial) : base(i, src)
        {
            m_hasVertexColor = hasVertexColor;
            m_vrmMaterial = vrmMaterial;
        }

        public override async Task<Material> GetOrCreateAsync(GetTextureAsyncFunc getTexture)
        {
            var item = m_vrmMaterial;
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
                return await MaterialFactory.CreateMaterial(m_index, m_src, m_hasVertexColor).GetOrCreateAsync(getTexture);
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
                var texture = await getTexture(new GetTextureParam(kv.Key, default, kv.Value, default, default, default, default, default));
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

        public MaterialItemBase CreateMaterial(int i, glTFMaterial src, bool hasVertexColor)
        {
            if (i == 0 && m_materials.Count == 0)
            {
                // dummy
                return new PBRMaterialItem(i, src);
            }

            return new MToonMaterialItem(i, src, hasVertexColor, m_materials[i]);
        }
    }
}
