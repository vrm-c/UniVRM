using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF.ShaderPropExporter
{
    public class PreExportShadersAttribute : Attribute { }
    public class PreExportShaderAttribute : Attribute { }

    public struct SupportedShader
    {
        public string TargetFolder;
        public string ShaderName;

        public SupportedShader(string targetFolder, string shaderName)
        {
            TargetFolder = targetFolder;
            ShaderName = shaderName;
        }
    }

    public static partial class PreShaderPropExporter
    {
        static Dictionary<string, ShaderProps> m_shaderPropMap;

        public static ShaderProps GetPropsForSupportedShader(string shaderName)
        {
            if (m_shaderPropMap == null)
            {
                m_shaderPropMap = new Dictionary<string, ShaderProps>();
                foreach (var prop in typeof(PreShaderPropExporter).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (prop.GetCustomAttributes(typeof(PreExportShaderAttribute), true).Any())
                    {
                        var kv = (KeyValuePair<string, ShaderProps>)prop.GetValue(null, null);
                        m_shaderPropMap.Add(kv.Key, kv.Value);
                    }
                }
            }

            ShaderProps props;
            if (m_shaderPropMap.TryGetValue(shaderName, out props))
            {
                return props;
            }

#if UNITY_EDITOR
            // fallback
            Debug.LogWarningFormat("{0} is not predefined shader. Use ShaderUtil", shaderName);
            var shader = Shader.Find(shaderName);
            return ShaderProps.FromShader(shader);
#else
            return null;
#endif
        }
    }
}
