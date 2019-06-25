using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF
{
    /// <summary>
    /// relative path from Unity project root.
    /// For AssetDatabase.
    /// </summary>
    public struct UnityPath
    {
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

        public bool IsUnderAssetsFolder
        {
            get
            {
                if (IsNull)
                {
                    return false;
                }
                return Value == "Assets" || Value.StartsWith("Assets/");
            }
        }

        public bool IsStreamingAsset
        {
            get
            {
                if (IsNull)
                {
                    return false;
                }

                return FullPath.StartsWith(Application.streamingAssetsPath + "/");
            }
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
                return new UnityPath(Value + "/" + name);
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
            if(obj is UnityPath)
            {
                var rhs = (UnityPath)obj;
                if(Value==null && rhs.Value == null)
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
            if (!IsUnderAssetsFolder)
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
            if (String.IsNullOrEmpty(unityPath))
            {
                return new UnityPath
                {
                    Value=""
                };
            }
            return FromFullpath(Path.GetFullPath(unityPath));
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

        static string AssetFullPath
        {
            get
            {
                return BaseFullPath + "/Assets";
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
                return Path.Combine(BaseFullPath, Value).Replace("\\", "/");
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
            if(fullPath == null)
            {
                fullPath = "";
            }
            fullPath = fullPath.Replace("\\", "/");

            if (fullPath == BaseFullPath) {
                return new UnityPath
                {
                    Value=""
                };
            }
            else if(fullPath.StartsWith(BaseFullPath + "/"))
            {
                return new UnityPath(fullPath.Substring(BaseFullPath.Length + 1));
            }
            else
            {
                return default(UnityPath);
            }
        }

        public static bool IsUnderAssetFolder(string fullPath)
        {
            return fullPath.Replace("\\", "/").StartsWith(AssetFullPath);
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

                foreach(var child in ChildDirs)
                {
                    foreach(var x in child.TraverseDir())
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
                foreach(var x in Directory.GetDirectories(FullPath))
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

#if UNITY_EDITOR
        public T GetImporter<T>() where T : AssetImporter
        {
            return AssetImporter.GetAtPath(Value) as T;
        }

        public static UnityPath FromAsset(UnityEngine.Object asset)
        {
            return new UnityPath(AssetDatabase.GetAssetPath(asset));
        }

        public void ImportAsset()
        {
            if (!IsUnderAssetsFolder)
            {
                throw new NotImplementedException();
            }
            AssetDatabase.ImportAsset(Value);
        }

        public void EnsureFolder()
        {
            if (IsNull)
            {
                throw new NotImplementedException();
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
            if (!IsUnderAssetsFolder)
            {
                throw new NotImplementedException();
            }

            return AssetDatabase.LoadAllAssetsAtPath(Value);
        }

        public void CreateAsset(UnityEngine.Object o)
        {
            if (!IsUnderAssetsFolder)
            {
                throw new NotImplementedException();
            }

            AssetDatabase.CreateAsset(o, Value);
        }

        public void AddObjectToAsset(UnityEngine.Object o)
        {
            if (!IsUnderAssetsFolder)
            {
                throw new NotImplementedException();
            }

            AssetDatabase.AddObjectToAsset(o, Value);
        }

        public T LoadAsset<T>() where T : UnityEngine.Object
        {
            if (!IsUnderAssetsFolder)
            {
                throw new NotImplementedException();
            }

            return AssetDatabase.LoadAssetAtPath<T>(Value);
        }

        public UnityPath GenerateUniqueAssetPath()
        {
            if (!IsUnderAssetsFolder)
            {
                throw new NotImplementedException();
            }

            return new UnityPath(AssetDatabase.GenerateUniqueAssetPath(Value));
        }
        #endif
    }
}
