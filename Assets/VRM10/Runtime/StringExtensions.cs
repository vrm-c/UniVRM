using System.IO;
using UnityEngine;

namespace UniVRM10
{
    public static class StringExtensions
    {
        public static string ToLowerCamelCase(this string lower)
        {
            return lower.Substring(0, 1).ToLower() + lower.Substring(1);
        }
        public static string ToUpperCamelCase(this string lower)
        {
            return lower.Substring(0, 1).ToUpper() + lower.Substring(1);
        }

        static string m_unityBasePath;
        public static string UnityBasePath
        {
            get
            {
                if (m_unityBasePath == null)
                {
                    m_unityBasePath = Path.GetFullPath(Application.dataPath + "/..").Replace("\\", "/");
                }
                return m_unityBasePath;
            }
        }

        public static string AssetPathToFullPath(this string path)
        {
            return UnityBasePath + "/" + path;
        }

        public static bool StartsWithUnityAssetPath(this string path)
        {
            return path.Replace("\\", "/").StartsWith(UnityBasePath + "/Assets");
        }

        public static string ToUnityRelativePath(this string path)
        {
            path = path.Replace("\\", "/");
            if (path.StartsWith(UnityBasePath))
            {
                return path.Substring(UnityBasePath.Length + 1);
            }

            //Debug.LogWarningFormat("{0} is starts with {1}", path, basePath);
            return path;
        }

        static readonly char[] EscapeChars = new char[]
        {
            '\\',
            '/',
            ':',
            '*',
            '?',
            '"',
            '<',
            '>',
            '|',
        };
        public static string EscapeFilePath(this string path)
        {
            foreach(var x in EscapeChars)
            {
                path = path.Replace(x, '+');
            }
            return path;
        }
    }
}
