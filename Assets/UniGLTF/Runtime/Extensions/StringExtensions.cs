using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UniGLTF
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
            return path.Replace("\\", "/").FastStartsWith(UnityBasePath + "/Assets");
        }

        public static string ToUnityRelativePath(this string path)
        {
            path = path.Replace("\\", "/");
            if (path.FastStartsWith(UnityBasePath))
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
            path = Regex.Replace(path, @"[\u0000-\u001F\u007F]", "+");

            foreach(var x in EscapeChars)
            {
                path = path.Replace(x, '+');
            }

            if (path.StartsWith('.'))
                path = '+' + path;

            if (path == "")
                path = "(empty)";

            return path;
        }

        // https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity5.html
        public static bool FastStartsWith(this string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            var aLen = a.Length;
            var bLen = b.Length;
            if (aLen < bLen)
            {
                return false;
            }

            var p = 0;
            while (p < bLen && a[p] == b[p])
            {
                ++p;
            }

            return p == bLen;
        }

        public static bool FastEndsWith(this string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            var aLen = a.Length;
            var bLen = b.Length;
            if (aLen < bLen)
            {
                return false;
            }

            var p = 1;
            while (p <= bLen && a[aLen - p] == b[bLen - p])
            {
                ++p;
            }

            return p - 1 == bLen;
        }
    }
}
