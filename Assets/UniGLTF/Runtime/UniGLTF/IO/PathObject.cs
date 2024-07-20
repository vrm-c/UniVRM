using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        public bool Exists => System.IO.File.Exists(FullPath);

        /// <summary>
        /// AssetDatabase の引き数になるパスを想定。
        /// Assets のひとつ上を 基準とする相対パス。
        /// Application.dataPath は Assets を得る。
        /// </summary>
        /// <returns></returns>
        public string UnityAssetPath
        {
            get
            {
                var root = UnityRoot;
                if (!IsDescendantOf(UnityRoot))
                {
                    throw new ArgumentException($"{FullPath} is not under UnityAssetPath");
                }
                return FullPath.Substring(root.FullPath.Length + 1);
            }
        }

        public static PathObject UnityRoot { get; } = FromFullPath(Path.GetDirectoryName(Application.dataPath));

        // 記述順に解決？
        public static PathObject UnityAssets { get; } = UnityRoot.Child("Assets/");

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
                var assetPath = UnityAssetPath;
                return $"<unity:{assetPath}>";
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

        /// <param name="src">AssetDatabase が使うパス</param>
        /// <returns></returns>
        public static PathObject FromUnityAssetPath(string src)
        {
            return UnityRoot.Child(src);
        }

        public static bool TryGetFromEnvironmentVariable(string key, out PathObject dst)
        {
            var value = System.Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                dst = default;
                return false;
            }
            dst = PathObject.FromFullPath(value);
            return true;
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
            if (FullPath.Length <= ascendant.FullPath.Length || FullPath[ascendant.FullPath.Length] != '/')
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

#if UNITY_EDITOR
        public static bool TryGetFromAsset(UnityEngine.Object src, out PathObject dst)
        {
            if (src == null)
            {
                dst = default;
                return false;
            }

            var assetPath = AssetDatabase.GetAssetPath(src);
            if (!string.IsNullOrEmpty(assetPath))
            {
                dst = FromUnityAssetPath(assetPath);
                return true;
            }

            var prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(src);
            if (!string.IsNullOrEmpty(prefab))
            {
                dst = FromUnityAssetPath(prefab);
                return true;
            }

            dst = default;
            return false;

        }

        public void ImportAsset()
        {
            AssetDatabase.ImportAsset(UnityAssetPath);
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
#endif
    }
}
