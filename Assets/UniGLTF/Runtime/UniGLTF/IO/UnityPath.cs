using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif


namespace UniGLTF
{
    /// <summary>
    /// Manage paths that can be handled by AssetDatabase
    /// Supports Assets or editable Package
    ///
    /// note : Use '\' instead of '/' to delimit folders
    /// </summary>
    public struct UnityPath
    {
#if UNITY_EDITOR
        #region UnityPath

        public string Value
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("unity://{0}", Value);
        }

        public bool IsNull
        {
            get { return Value == null; }
        }

        /// <summary>
        /// If under Assets or under an editable Package return true.
        /// For historical reasons "Assets" is true.
        /// </summary>
        public bool IsUnderWritableFolder
        {
            get
            {
                if (IsNull) return false;

                if (PathType == PathType.Assets) return true;

                if (PathType == PathType.Packages)
                {
                    var split = Value.Split('/');
                    if (split.Length <= 1) return false;

                    var packageDirectory = $"{split[0]}/{split[1]}";
                    if (!Directory.Exists(packageDirectory)) return false;

                    var packageInfo = GetPackageInfo(packageDirectory);
                    if (packageInfo == null) return false;

                    // Local and Embedded packages are editable
                    if (packageInfo.source == PackageSource.Local
                        || packageInfo.source == PackageSource.Embedded) return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Recursively check if path is included in local packages
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static PackageInfo GetPackageInfo(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            var packageInfo = PackageList.Find(x => x.resolvedPath == path || x.assetPath == path);
            if (packageInfo != null)
            {
                return packageInfo;
            }

            return GetPackageInfo(Path.GetDirectoryName(path));
        }

        /// <summary>
        /// List of packages loaded in unity
        /// </summary>
        private static readonly List<PackageInfo> PackageList
            = AssetDatabase.FindAssets("package")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(x => AssetDatabase.LoadAssetAtPath<TextAsset>(x) != null)
            .Select(PackageInfo.FindForAssetPath)
            .Where(x => x != null)
            .ToList();

        public bool IsStreamingAsset
        {
            get
            {
                if (IsNull)
                {
                    return false;
                }

                return FullPath.FastStartsWith(Application.streamingAssetsPath + "/");
            }
        }

        public string FileName
        {
            get { return Path.GetFileName(Value); }
        }

        public string FileNameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(Value); }
        }

        public string Extension
        {
            get { return Path.GetExtension(Value); }
        }

        public UnityPath Parent
        {
            get
            {
                if (IsNull)
                {
                    return default(UnityPath);
                }

                return new UnityPath(Path.GetDirectoryName(Value));
            }
        }

        public bool HasParent
        {
            get
            {
                return !string.IsNullOrEmpty(Value);
            }
        }

        public PathType PathType
        {
            get
            {
                if (string.IsNullOrEmpty(Value))
                {
                    return PathType.Unsupported;
                }
                if (Value == "Assets" || Value.FastStartsWith("Assets/"))
                {
                    // #1941
                    return PathType.Assets;
                }

                var directory = Path.GetDirectoryName(Value);
                var rootDirectoryName = directory.Split(Path.DirectorySeparatorChar);
                switch (rootDirectoryName[0])
                {
                    case "Assets":
                        return PathType.Assets;
                    case "Packages":
                        return PathType.Packages;
                    default:
                        return PathType.Unsupported;
                }
            }
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

        static string EscapeFilePath(string path)
        {
            foreach (var x in EscapeChars)
            {
                path = path.Replace(x, '+');
            }
            return path;
        }

        public UnityPath Child(string name)
        {
            if (IsNull)
            {
                throw new NotImplementedException();
            }
            else if (Value == "")
            {
                return new UnityPath(name);
            }
            else
            {
                return new UnityPath($"{Value}/{name}");
            }
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is UnityPath)
            {
                var rhs = (UnityPath)obj;
                if (Value == null && rhs.Value == null)
                {
                    return true;
                }
                else if (Value == null)
                {
                    return false;
                }
                else if (rhs.Value == null)
                {
                    return false;
                }
                else
                {
                    return Value == rhs.Value;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Remove extension and add suffix
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public UnityPath GetAssetFolder(string suffix)
        {
            if (!IsUnderWritableFolder)
            {
                throw new NotImplementedException();
            }

            return new UnityPath(
                string.Format("{0}/{1}{2}",
                Parent.Value,
                FileNameWithoutExtension,
                suffix
                ));
        }

        UnityPath(string value) : this()
        {
            Value = value.Replace("\\", "/");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="unityPath">Relative from unity current path. GetParent(Application.dataPath)</param>
        /// <returns></returns>
        public static UnityPath FromUnityPath(string unityPath)
        {
            if (unityPath == null)
            {
                return new UnityPath
                {
                    Value = null
                };
            }
            if (unityPath == "" || unityPath == ".")
            {
                return new UnityPath
                {
                    Value = ""
                };
            }
            return new UnityPath(unityPath);
        }
        #endregion

        #region FullPath
        static string s_basePath;
        static string BaseFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(s_basePath))
                {
                    s_basePath = Path.GetFullPath(Application.dataPath + "/..").Replace("\\", "/");
                }
                return s_basePath;
            }
        }

        public string FullPath
        {
            get
            {
                if (IsNull)
                {
                    throw new NotImplementedException();
                }
                return Path.GetFullPath(Value == "" ? "." : Value).Replace("\\", "/");
            }
        }

        public bool IsFileExists
        {
            get { return File.Exists(FullPath); }
        }

        public bool IsDirectoryExists
        {
            get { return Directory.Exists(FullPath); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fullPath">C:/path/to/file</param>
        /// <returns></returns>
        public static UnityPath FromFullpath(string fullPath)
        {
            if (fullPath == null)
            {
                fullPath = "";
            }
            fullPath = fullPath.Replace("\\", "/");

            if (fullPath == BaseFullPath)
            {
                return new UnityPath("");
            }

            if (fullPath.FastStartsWith($"{BaseFullPath}/Assets"))
            {
                return new UnityPath(fullPath.Substring(BaseFullPath.Length + 1));
            }

            var packageInfo = GetPackageInfo(fullPath);
            if (packageInfo != null)
            {
                var packagePath = packageInfo.assetPath;
                var fileName = fullPath.Substring(packageInfo.resolvedPath.Length + 1);
                var relativePath = $"{packagePath}/{fileName}";
                return new UnityPath(relativePath);
            }

            return default(UnityPath);
        }
        #endregion

        [Obsolete("Use TraverseDir()")]
        public IEnumerable<UnityPath> TravserseDir()
        {
            return TraverseDir();
        }

        public IEnumerable<UnityPath> TraverseDir()
        {
            if (IsDirectoryExists)
            {
                yield return this;

                foreach (var child in ChildDirs)
                {
                    foreach (var x in child.TraverseDir())
                    {
                        yield return x;
                    }
                }
            }
        }

        public IEnumerable<UnityPath> ChildDirs
        {
            get
            {
                foreach (var x in Directory.GetDirectories(FullPath))
                {
                    yield return UnityPath.FromFullpath(x);
                }
            }
        }

        public IEnumerable<UnityPath> ChildFiles
        {
            get
            {
                foreach (var x in Directory.GetFiles(FullPath))
                {
                    yield return UnityPath.FromFullpath(x);
                }
            }
        }

        public T GetImporter<T>() where T : AssetImporter
        {
            return AssetImporter.GetAtPath(Value) as T;
        }

        public static UnityPath FromAsset(UnityEngine.Object asset)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new System.ArgumentNullException();
            }
            return new UnityPath(assetPath);
        }

        public void ImportAsset()
        {
            if (!IsUnderWritableFolder)
            {
                throw new NotImplementedException();
            }
            AssetDatabase.ImportAsset(Value);
        }

        public void EnsureFolder()
        {
            if (IsNull)
            {
                return;
            }

            if (HasParent)
            {
                Parent.EnsureFolder();
            }

            if (!IsDirectoryExists)
            {
                var parent = Parent;
                // ensure parent
                parent.ImportAsset();
                // create
                AssetDatabase.CreateFolder(
                    parent.Value,
                    Path.GetFileName(Value)
                    );
                ImportAsset();
            }
        }

        public UnityEngine.Object[] GetSubAssets()
        {
            if (!IsUnderWritableFolder)
            {
                throw new NotImplementedException();
            }

            return AssetDatabase.LoadAllAssetsAtPath(Value);
        }

        public void CreateAsset(UnityEngine.Object o)
        {
            if (!IsUnderWritableFolder)
            {
                throw new NotImplementedException();
            }

            try
            {
                AssetDatabase.CreateAsset(o, Value);
            }
            catch (UnityException)
            {
                // アセットを作ることができないファイル名だったと仮定。
                // 元のファイル名のどこに問題があるか不明なので Guid で置き換える。
                var newName = $"{Parent.Value}/{Guid.NewGuid().ToString("N")}{Extension}";
                UniGLTFLogger.Warning($"rename: {Value} => {newName}");

                AssetDatabase.CreateAsset(o, newName);
            }
        }

        public void AddObjectToAsset(UnityEngine.Object o)
        {
            if (!IsUnderWritableFolder)
            {
                throw new NotImplementedException();
            }

            AssetDatabase.AddObjectToAsset(o, Value);
        }

        public T LoadAsset<T>() where T : UnityEngine.Object
        {
            if (!IsUnderWritableFolder)
            {
                throw new NotImplementedException();
            }

            return AssetDatabase.LoadAssetAtPath<T>(Value);
        }

        public UnityPath GenerateUniqueAssetPath()
        {
            if (!IsUnderWritableFolder)
            {
                throw new NotImplementedException();
            }

            return new UnityPath(AssetDatabase.GenerateUniqueAssetPath(Value));
        }
#endif
    }

    public enum PathType
    {
        Assets,
        Packages,
        Unsupported,
    }
}
