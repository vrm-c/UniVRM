using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MToon
{
    public static class EditorUtils
    {
        private static string BasePath { get { return Path.Combine(Application.dataPath, "VRM/MToon"); } }

        private static string ShaderFilePath { get { return Path.Combine(BasePath, "MToon/Resources/Shaders/MToon.shader"); } }
        private static string ReadMeFilePath { get { return Path.Combine(BasePath, "README.md"); } }
        private static string VersionFilePath { get { return Path.Combine(BasePath, "MToon/Scripts/UtilsVersion.cs"); } }


        //[MenuItem("VRM/MToon Version Up")]
        private static void VerUp(string version)
        {
            UpdateShaderFile(version);
            UpdateReadMeFile(version);
            UpdateVersionFile(version);
        }
        
        private static void UpdateShaderFile(string version)
        {
            var file = File.ReadAllText(ShaderFilePath);
            file = Regex.Replace(
                file,
                "(_MToonVersion \\(\"_MToonVersion\", Float\\) = )(\\d+)",
                "${1}" + version
            );
            File.WriteAllText(ShaderFilePath, file);
        }
        
        private static void UpdateReadMeFile(string version)
        {
            version = "v" + version.Substring(0, version.Length - 1) + "." + version[version.Length - 1];
            
            var file = File.ReadAllText(ReadMeFilePath);
            file = Regex.Replace(
                file,
                "v(\\d+)\\.(\\d+)",
                version
            );
            File.WriteAllText(ReadMeFilePath, file);
        }

        private static void UpdateVersionFile(string version)
        {
            var file = File.ReadAllText(VersionFilePath);
            file = Regex.Replace(
                file,
                "(public const int VersionNumber = )(\\d+)(;)",
                "${1}" + version + "${3}"
            );
            File.WriteAllText(VersionFilePath, file);
            
        }
    }
}