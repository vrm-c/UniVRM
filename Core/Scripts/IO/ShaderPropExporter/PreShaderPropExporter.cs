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
        const string TARGET_FOLDER = "UniGLTF/Core/Scripts";

#pragma warning disable 414
        [PreExportShaders]
        static SupportedShader[] SupportedShaders = new SupportedShader[]
        {
            new SupportedShader(TARGET_FOLDER, "Standard"),
            new SupportedShader(TARGET_FOLDER, "Unlit/Color"),
            new SupportedShader(TARGET_FOLDER, "Unlit/Texture"),
            new SupportedShader(TARGET_FOLDER, "Unlit/Transparent"),
            new SupportedShader(TARGET_FOLDER, "Unlit/Transparent Cutout"),
            new SupportedShader(TARGET_FOLDER, "UniGLTF/UniUnlit"),
        };
#pragma warning restore 414

#if UNITY_EDITOR
        [MenuItem(UniGLTFVersion.UNIGLTF_VERSION + "/PreExport ShaderProps")]
        public static void PreExport()
        {
            foreach (var fi in typeof(PreShaderPropExporter).GetFields(
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

        static UnityPath GetExportDir(string target)
        {
            foreach (var x in UnityPath.FromUnityPath("Assets").TravserseDir())
            {
                if (x.Value.EndsWith(target))
                {
                    var dir = x.Child("PreExportShaderProps");
                    dir.EnsureFolder();
                    return dir;
                }
            }
            throw new Exception(target + " not found");
        }

        static void PreExport(SupportedShader supportedShader)
        {
            var path = GetExportDir(supportedShader.TargetFolder).Child(EscapeShaderName(supportedShader.ShaderName) + ".cs");
            Debug.LogFormat("PreExport: {0}", path.FullPath);

            var shader = Shader.Find(supportedShader.ShaderName);
            var props = ShaderProps.FromShader(shader);

            File.WriteAllText(path.FullPath, props.ToString(shader.name));
        }
#endif

        #region Runtime
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
        #endregion
    }
}
