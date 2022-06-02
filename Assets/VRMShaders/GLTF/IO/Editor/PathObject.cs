using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// UniGLTF.UnityPath (Assets の ひとつ上がルート) をすべてのパスが扱えるように拡張するのが趣旨。
    /// readonly struct で Immutable であるという主張。
    /// </summary>
    public readonly struct PathObject
    {
        /// <summary>
        /// * Delemeter は / を保証
        /// * .. を解決済み
        /// * フルパス
        /// * 末尾に / を付けない
        /// </summary>
        public string FullPath { get; }

        public string Extension => Path.GetExtension(FullPath);

        public string Stem => Path.GetFileNameWithoutExtension(FullPath);

        public PathObject Parent => FromFullPath(Path.GetDirectoryName(FullPath));

        public bool IsUnderAsset => IsDescendantOf(UnityAssets);

        /// <summary>
        /// relative from UnityEngine.Application.dataPath
        /// </summary>
        /// <returns></returns>
        public string UnityPath
        {
            get
            {
                var root = UnityRoot;
                if (!IsDescendantOf(UnityRoot))
                {
                    throw new ArgumentException($"{FullPath} is not under UnityPath");
                }
                return FullPath.Substring(root.FullPath.Length + 1);
            }
        }

        static PathObject? _root;
        public static PathObject UnityRoot
        {
            get
            {
                if (!_root.HasValue)
                {
                    _root = FromFullPath(Path.GetDirectoryName(Application.dataPath));
                }
                return _root.Value;
            }
        }

        public static PathObject UnityAssets => UnityRoot.Child("Assets/");

        PathObject(string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                throw new ArgumentNullException();
            }
            src = Path.GetFullPath(src).Replace('\\', '/');
            if (src.Length > 1 && src[src.Length - 1] == '/')
            {
                // drop last /
                src = src.Substring(0, src.Length - 1);
            }
            if (src[0] == '/')
            {
                FullPath = src;
            }
            else
            {
                if (src.Length >= 3 && src[1] == ':' && src[2] == '/')
                {
                    FullPath = src;
                }
                else
                {
                    throw new ArgumentException($"{src} is not fullpath");
                }
            }
        }

        public override string ToString()
        {
            try
            {
                var unityPath = UnityPath;
                return $"<unity:{unityPath}>";
            }
            catch (ArgumentException)
            {
                return $"<file:{FullPath}>";
            }
        }

        /// <param name="src">start with "X:/" on Windows else "/"</param>
        /// <returns></returns>
        public static PathObject FromFullPath(string src)
        {
            return new PathObject(src);
        }

        /// <param name="src">relative from UnityEngine.Application.dataPath</param>
        /// <returns></returns>
        public static PathObject FromUnityPath(string src)
        {
            return UnityRoot.Child(src);
        }

        public static PathObject FromAsset(UnityEngine.Object src)
        {
            if (src == null)
            {
                throw new ArgumentNullException();
            }
            var assetPath = AssetDatabase.GetAssetPath(src);
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new ArgumentException($"{src} is not asset");
            }
            return FromUnityPath(assetPath);
        }

        public PathObject Child(string child)
        {
            return FromFullPath(Path.Combine(FullPath, child));
        }

        public bool IsDescendantOf(PathObject ascendant)
        {
            if (!FullPath.StartsWith(ascendant.FullPath))
            {
                return false;
            }
            if (FullPath[ascendant.FullPath.Length] != '/')
            {
                return false;
            }
            return true;
        }

        public byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(FullPath);
        }

        public void WriteAllBytes(byte[] data)
        {
            File.WriteAllBytes(FullPath, data);
        }

        public void ImportAsset()
        {
            AssetDatabase.ImportAsset(UnityPath);
        }

        public bool TrySaveDialog(string title, string name, out PathObject dst)
        {
            var path = EditorUtility.SaveFilePanel(
                title,
                FullPath,
                name,
                "vrm");
            if (string.IsNullOrEmpty(path))
            {
                dst = default;
                return false;
            }
            dst = PathObject.FromFullPath(path);
            return true;
        }
    }
}
