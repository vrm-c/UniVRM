using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


namespace UniGLTF.MeshUtility
{
    /// <summary>
    /// Application.dataPath を root とするファイルパスを扱う。
    /// (Project/Assets)
    /// </summary>
    public struct AssetsPath
    {
        public string RelativePath
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return $"assets://{RelativePath}";
        }

        public string FileName
        {
            get { return Path.GetFileName(RelativePath); }
        }

        public string FileNameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(RelativePath);
            }
        }

        public string Extension
        {
            get { return Path.GetExtension(RelativePath); }
        }

        public AssetsPath Parent
        {
            get
            {
                return new AssetsPath(Path.GetDirectoryName(RelativePath));
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

        public AssetsPath Child(string name)
        {
            if (RelativePath == null)
            {
                throw new NotImplementedException();
            }
            else if (RelativePath == "")
            {
                return new AssetsPath(name);
            }
            else
            {
                return new AssetsPath(RelativePath + "/" + name);
            }
        }

        public override int GetHashCode()
        {
            if (RelativePath == null)
            {
                return 0;
            }
            return RelativePath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is AssetsPath)
            {
                var rhs = (AssetsPath)obj;
                if (RelativePath == null && rhs.RelativePath == null)
                {
                    return true;
                }
                else if (RelativePath == null)
                {
                    return false;
                }
                else if (rhs.RelativePath == null)
                {
                    return false;
                }
                else
                {
                    return RelativePath == rhs.RelativePath;
                }
            }
            else
            {
                return false;
            }
        }

        AssetsPath(string value) : this()
        {
            RelativePath = value.Replace("\\", "/");
        }

        #region FullPath
        static string s_basePath;
        static string BaseFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(s_basePath))
                {
                    s_basePath = Path.GetFullPath(Application.dataPath).Replace("\\", "/");
                }
                return s_basePath;
            }
        }

        public string FullPath
        {
            get
            {
                if (RelativePath == null)
                {
                    throw new NotImplementedException();
                }
                return Path.Combine(BaseFullPath, RelativePath).Replace("\\", "/");
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
        public static AssetsPath FromFullpath(string fullPath)
        {
            if (fullPath == null)
            {
                fullPath = "";
            }
            fullPath = fullPath.Replace("\\", "/");

            if (fullPath == BaseFullPath)
            {
                return new AssetsPath
                {
                    RelativePath = ""
                };
            }
            else if (fullPath.StartsWith(BaseFullPath + "/"))
            {
                return new AssetsPath(fullPath.Substring(BaseFullPath.Length + 1));
            }
            else
            {
                return default(AssetsPath);
            }
        }
        #endregion

        public IEnumerable<AssetsPath> TraverseDir()
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

        public IEnumerable<AssetsPath> ChildDirs
        {
            get
            {
                foreach (var x in Directory.GetDirectories(FullPath))
                {
                    yield return AssetsPath.FromFullpath(x);
                }
            }
        }

        public IEnumerable<AssetsPath> ChildFiles
        {
            get
            {
                foreach (var x in Directory.GetFiles(FullPath))
                {
                    yield return AssetsPath.FromFullpath(x);
                }
            }
        }

#if UNITY_EDITOR
        string UnityPath => $"Assets/{RelativePath}";

        public T GetImporter<T>() where T : AssetImporter
        {
            return AssetImporter.GetAtPath(UnityPath) as T;
        }

        public static AssetsPath FromAsset(UnityEngine.Object asset)
        {
            // skip Assets/
            return new AssetsPath(AssetDatabase.GetAssetPath(asset).Substring(7));
        }

        public void ImportAsset()
        {
            AssetDatabase.ImportAsset(UnityPath);
        }

        public void EnsureFolder()
        {
            if (RelativePath == null)
            {
                throw new NotImplementedException();
            }

            if (!string.IsNullOrEmpty(Parent.RelativePath))
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
                    parent.UnityPath,
                    Path.GetFileName(RelativePath)
                    );
                ImportAsset();
            }
        }

        public UnityEngine.Object[] GetSubAssets()
        {
            return AssetDatabase.LoadAllAssetsAtPath(UnityPath);
        }

        public void CreateAsset(UnityEngine.Object o)
        {
            AssetDatabase.CreateAsset(o, UnityPath);
        }

        public void AddObjectToAsset(UnityEngine.Object o)
        {
            AssetDatabase.AddObjectToAsset(o, UnityPath);
        }

        public T LoadAsset<T>() where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(UnityPath);
        }

        public AssetsPath GenerateUniqueAssetPath()
        {
            return new AssetsPath(AssetDatabase.GenerateUniqueAssetPath(UnityPath));
        }
#endif
    }
}
