using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using UniGLTF.ShaderPropExporter;
using System.Collections.Generic;

namespace UniGLTF
{
    public static class ShaderPropMenu
    {
#if VRM_DEVELOP
        [MenuItem("VRM/ShaderProperty/PreExport ShaderProps")]
#endif
        public static void PreExport()
        {
            foreach (var fi in typeof(PreExportShaders).GetFields(
                BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic))
            {
                var attr = fi.GetCustomAttributes(true).FirstOrDefault(y => y is PreExportShadersAttribute);
                if (attr != null)
                {
                    var supportedShaders = fi.GetValue(null) as SupportedShader[];
                    foreach (var supported in supportedShaders)
                    {
                        PreExport(supported);
                    }
                }
            }
        }

        static string EscapeShaderName(string name)
        {
            return name.Replace("/", "_").Replace(" ", "_");
        }

        static string ExportDir
        {
            get
            {
                return Application.dataPath + "/VRM/ShaderProperty/Runtime";
            }
        }

        static void PreExport(SupportedShader supportedShader)
        {
            var shader = Shader.Find(supportedShader.ShaderName);
            var props = ShaderProps.FromShader(shader);

            var path = Path.Combine(ExportDir, supportedShader.TargetFolder);
            path = Path.Combine(path, EscapeShaderName(supportedShader.ShaderName) + ".cs").Replace("\\", "/");
            Debug.LogFormat("PreExport: {0}", path);
            File.WriteAllText(path, ToString(props, shader.name));
        }

        static string ToString(ShaderProps props, string shaderName)
        {
            var list = new List<string>();
            foreach (var prop in props.Properties)
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
, string.Join(",", list.ToArray()));
        }

    }
}
