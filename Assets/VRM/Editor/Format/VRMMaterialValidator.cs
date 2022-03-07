using System.Collections.Generic;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// VRM0
    /// </summary>
    class VRMMaterialValidator : UniGLTF.DefaultMaterialValidator
    {
        public override string GetGltfMaterialTypeFromUnityShaderName(string shaderName)
        {
            var name = VRMMaterialExporter.VrmMaterialName(shaderName);
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            return base.GetGltfMaterialTypeFromUnityShaderName(shaderName);
        }

        public override IEnumerable<(string propertyName, Texture texture)> EnumerateTextureProperties(Material m)
        {
            if (m.shader.name != "VRM/MToon")
            {
                // PBR, Unlit
                foreach (var x in base.EnumerateTextureProperties(m))
                {
                    yield return x;
                }
            }

            // all            
            var prop = UniGLTF.ShaderPropExporter.PreShaderPropExporter.GetPropsForSupportedShader(m.shader.name);
            foreach (var kv in prop.Properties)
            {
                if (kv.ShaderPropertyType == UniGLTF.ShaderPropExporter.ShaderPropertyType.TexEnv)
                {
                    yield return (kv.Key, m.GetTexture(kv.Key));
                }
            }
        }
    }
}
