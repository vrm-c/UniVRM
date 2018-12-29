#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#endif


namespace UniGLTF.ShaderPropExporter
{
    public enum ShaderPropertyType
    {
        TexEnv,
        Color,
        Range,
        Float,
        Vector,
    }

    public struct ShaderProperty
    {
        public string Key;
        public ShaderPropertyType ShaderPropertyType;

        public ShaderProperty(string key, ShaderPropertyType propType)
        {
            Key = key;
            ShaderPropertyType = propType;
        }
    }

    public class ShaderProps
    {
        public ShaderProperty[] Properties;

#if UNITY_EDITOR
        static ShaderPropertyType ConvType(ShaderUtil.ShaderPropertyType src)
        {
            switch (src)
            {
                case ShaderUtil.ShaderPropertyType.TexEnv: return ShaderPropertyType.TexEnv;
                case ShaderUtil.ShaderPropertyType.Color: return ShaderPropertyType.Color;
                case ShaderUtil.ShaderPropertyType.Float: return ShaderPropertyType.Float;
                case ShaderUtil.ShaderPropertyType.Range: return ShaderPropertyType.Range;
                case ShaderUtil.ShaderPropertyType.Vector: return ShaderPropertyType.Vector;
                default: throw new NotImplementedException();
            }
        }

        public static ShaderProps FromShader(Shader shader)
        {
            var properties = new List<ShaderProperty>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); ++i)
            {
                var name = ShaderUtil.GetPropertyName(shader, i);
                var propType = ShaderUtil.GetPropertyType(shader, i);
                properties.Add(new ShaderProperty(name, ConvType(propType)));
            }

            return new ShaderProps
            {
                Properties = properties.ToArray(),
            };
        }

        static string EscapeShaderName(string name)
        {
            return name.Replace("/", "_").Replace(" ", "_");
        }

        public string ToString(string shaderName)
        {
            var list = new List<string>();
            foreach (var prop in Properties)
            {
                list.Add(string.Format("new ShaderProperty(\"{0}\", ShaderPropertyType.{1})\r\n", prop.Key, prop.ShaderPropertyType));
            }

            return string.Format(@"using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{{
    public static partial class PreShaderPropExporter
    {{
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> {0} 
        {{
            get 
            {{
                return new KeyValuePair<string, ShaderProps>(
                    ""{1}"",
                    new ShaderProps
                    {{
                        Properties = new ShaderProperty[]{{
{2}
                        }}
                    }}
                );
            }}
        }}
    }}
}}
"
, EscapeShaderName(shaderName)
, shaderName
, String.Join(",", list.ToArray()));
        }
#endif
    }
}
