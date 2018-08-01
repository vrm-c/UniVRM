using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UniGLTF;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VRM
{
    public class VRMPreExportShaderAttribute : Attribute { }

    public static partial class VRMPreShaderPropExporter
    {
        public static string[] SupportedShaders = new string[]
        {
            "VRM/MToon",
            "VRM/UnlitTexture",
            "VRM/UnlitCutout",
            "VRM/UnlitTransparent",
            "VRM/UnlitTransparentZWrite",
        };

#if UNITY_EDITOR
        [MenuItem(VRMVersion.VRM_VERSION + "/PreExport ShaderProps")]
        static void PreExport()
        {
            foreach (var shaderName in SupportedShaders)
            {
                var shader = Shader.Find(shaderName);
                PreExport(shader);
            }
        }

        static string EscapeShaderName(string name)
        {
            return name.Replace("/", "_").Replace(" ", "_");
        }

        const string EXPORT_DIR = "Assets/VRM/Scripts/PreExportShaderProps/";
        static void PreExport(Shader shader)
        {
            var path = UnityPath.FromUnityPath(EXPORT_DIR + EscapeShaderName(shader.name) + ".cs");
            Debug.LogFormat("PreExport: {0}", path.FullPath);

            var props = ShaderProps.FromShader(shader);

            File.WriteAllText(path.FullPath, props.ToString(shader.name));
        }
#endif

        public enum ShaderPropertyType
        {
            TexEnv,
            Color,
            Range,
            Float,
            Vector,
        }

        public class ShaderProps
        {
            public KeyValuePair<string, ShaderPropertyType>[] Properties;

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
                var properties = new List<KeyValuePair<string, ShaderPropertyType>>();
                for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); ++i)
                {
                    var name = ShaderUtil.GetPropertyName(shader, i);
                    var propType = ShaderUtil.GetPropertyType(shader, i);
                    properties.Add(new KeyValuePair<string, ShaderPropertyType>(name, ConvType(propType)));
                }

                return new ShaderProps
                {
                    Properties = properties.ToArray(),
                };
            }

            public string ToString(string shaderName)
            {
                var list = new List<string>();
                foreach(var kv in Properties)
                {
                    list.Add(string.Format("new KeyValuePair<string, ShaderPropertyType>(\"{0}\", ShaderPropertyType.{1})\r\n", kv.Key, kv.Value));
                }

                return string.Format(@"using System.Collections.Generic;


namespace VRM 
{{
    public static partial class VRMPreShaderPropExporter
    {{
        [VRMPreExportShader]
        static KeyValuePair<string, ShaderProps> {0} 
        {{
            get 
            {{
                return new KeyValuePair<string, ShaderProps>(
                    ""{1}"",
                    new ShaderProps
                    {{
                        Properties = new KeyValuePair<string, ShaderPropertyType>[]{{
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

        #region Runtime
        static Dictionary<string, ShaderProps> m_shaderPropMap;

        public static ShaderProps GetPropsForSupportedShader(string shaderName)
        {
            if (m_shaderPropMap == null)
            {
                m_shaderPropMap = new Dictionary<string, ShaderProps>();
                foreach (var prop in typeof(VRMPreShaderPropExporter).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (prop.GetCustomAttributes(typeof(VRMPreExportShaderAttribute), true).Any())
                    {
                        var kv = (KeyValuePair<string, ShaderProps>)prop.GetValue(null, null);
                        m_shaderPropMap.Add(kv.Key, kv.Value);
                    }
                }
            }

            ShaderProps props;
            if (!m_shaderPropMap.TryGetValue(shaderName, out props))
            {
                return null;
            }

            return props;
        }
        #endregion
    }
}
