using System;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public static class VRMSampleCopy
    {
        struct CopyInfo
        {
            public string Src;
            public string Dst;

            public CopyInfo(string src, string dst)
            {
                Src = src;
                Dst = dst;
            }
        }

        static CopyInfo[] CopyList = new CopyInfo[]{
            new CopyInfo(Path.Combine(Application.dataPath, "VRM_Samples"), Path.Combine(Application.dataPath, "VRM/Samples~")),
            new CopyInfo(Path.Combine(Application.dataPath, "VRM10_Samples"), Path.Combine(Application.dataPath, "VRM10/Samples~")),
            new CopyInfo(Path.Combine(Application.dataPath, "UniGLTF_Samples"), Path.Combine(Application.dataPath, "UniGLTF/Samples~")),
        };

        /// <summary>
        /// Assets/VRM_Samples
        /// を
        /// Assets/VRM/Samples~
        /// にコピーする
        /// 
        /// Assets/VRM10_Samples
        /// を
        /// Assets/VRM10/Samples~
        /// にコピーする
        /// 
        /// Assets/UniGLTF_Samples
        /// を
        /// Assets/UniGLTF/Samples~
        /// にコピーする
        /// </summary>
        public static void Execute()
        {
            foreach (var info in CopyList)
            {
                Copy(info.Src, info.Dst);
            }
        }

        static void Copy(string srcDir, string dstDir)
        {
            // delete dst
            if (Directory.Exists(dstDir))
            {
                Directory.Delete(dstDir, recursive: true);
            }
            _Copy(srcDir, dstDir);
            UniGLTFLogger.Log($"copy {srcDir} \n  => {dstDir}");
        }

        static void _Copy(string src, string dst)
        {
            if (Directory.Exists(src))
            {
                Directory.CreateDirectory(dst);
                foreach (var child in Directory.EnumerateFileSystemEntries(src))
                {
                    _Copy(child, Path.Combine(dst, Path.GetFileName(child)));
                }
            }
            else if (File.Exists(src))
            {
                File.Copy(src, dst);
            }
            else
            {
                throw new FileNotFoundException(src);
            }
        }

        public static bool Validate()
        {
            return CopyList.All(x => _Validate(x.Src, x.Dst));
        }

        static bool _Validate(string src, string dst)
        {
            if (Directory.Exists(src))
            {
                if (!Directory.Exists(dst))
                {
                    UniGLTFLogger.Error($"{dst} not exists");
                    return false;

                }
                var list = Directory.EnumerateFileSystemEntries(dst).Select(x => x.Substring(dst.Length + 1)).ToList();
                foreach (var child in Directory.EnumerateFileSystemEntries(src))
                {
                    if (!_Validate(child, Path.Combine(dst, Path.GetFileName(child)).Replace("\\", "/")))
                    {
                        return false;
                    }
                    var rel = child.Substring(src.Length + 1);
                    list.Remove(rel);
                }
                if (list.Count > 0)
                {
                    var remain = string.Join(",", list);
                    UniGLTFLogger.Error($"only dst: {remain}");
                    return false;
                }
                return true;
            }
            else if (File.Exists(src))
            {
                // same file
                if (!File.Exists(dst))
                {
                    UniGLTFLogger.Error($"{dst} not exists");
                    return false;
                }
                if (!File.ReadAllBytes(src).SequenceEqual(File.ReadAllBytes(dst)))
                {
                    UniGLTFLogger.Error($"{src} != {dst}");
                    return false;
                }
                return true;
            }
            else
            {
                // throw new FileNotFoundException(src);
                UniGLTFLogger.Error($"dir nor file");
                return false;
            }
        }
    }
}
