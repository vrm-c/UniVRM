using System.Collections.Generic;
using System.Linq;
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
            if (BuiltInVrmMaterialExporter.SupportedShaderNames.Contains(shaderName))
            {
                return "VRM0X";
            }
            return base.GetGltfMaterialTypeFromUnityShaderName(shaderName);
        }

        public override IEnumerable<(string propertyName, Texture texture)> EnumerateTextureProperties(Material m)
        {
            /// 歴史的経緯により処理ロジックが２種類ある
            if (m.shader.name == "VRM/MToon")
            {
                // [Unity列挙]
                // * UnityEditor.ShaderUtil により Shader の Property を列挙する。Editor専用。
                // * あらかじめEditorで実行して property 一覧をハードコーディングしてある `Assets\VRMShaders\VRM\IO\Runtime\VRM\PreExportShaders_VRM.cs` 界隈。
                // * 今は "VRM/MToon" のみ
                // 
                // extensions.VRM.materialProperties に記録する
                // 
                var prop = UniGLTF.ShaderPropExporter.PreShaderPropExporter.GetPropsForMToon();
                foreach (var kv in prop.Properties)
                {
                    if (kv.ShaderPropertyType == UniGLTF.ShaderPropExporter.ShaderPropertyType.TexEnv)
                    {
                        yield return (kv.Key, m.GetTexture(kv.Key));
                    }
                }
            }
            else
            {
                // [Shaderの定義から手で記述]
                //
                // PBR, Unlit
                // * DefaultMaterialValidator.EnumerateTextureProperties
                //
                // glTF.materials に記録する
                //
                foreach (var textureProperty in base.EnumerateTextureProperties(m))
                {
                    yield return textureProperty;
                }
            }
        }
    }
}
