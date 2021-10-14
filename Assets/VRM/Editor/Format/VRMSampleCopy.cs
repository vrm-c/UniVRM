using System;
using System.IO;
using UnityEngine;

namespace VRM
{
    public static class VRMSampleCopy
    {
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
        /// 南無
        /// </summary>
        public static void Execute()
        {
            Copy(Path.Combine(Application.dataPath, "VRM_Samples"), Path.Combine(Application.dataPath, "VRM/Samples~"));
            Copy(Path.Combine(Application.dataPath, "VRM10_Samples"), Path.Combine(Application.dataPath, "VRM10/Samples~"));
        }

        static void Copy(string srcDir, string dstDir)
        {
            // delete dst
            if (Directory.Exists(dstDir))
            {
                Directory.Delete(dstDir, recursive: true);
            }
            _Copy(srcDir, dstDir);
            Debug.Log($"copy {srcDir} \n  => {dstDir}");
        }

        static void _Copy(string src, string dst)
        {
            if (Directory.Exists(src))
            {
                // Debug.Log($"CreateDirectory({dst})");
                Directory.CreateDirectory(dst);
                foreach (var child in Directory.EnumerateFileSystemEntries(src))
                {
                    _Copy(child, Path.Combine(dst, Path.GetFileName(child)));
                }
            }
            else if (File.Exists(src))
            {
                // Debug.Log($"Copy {src} => {dst}");
                File.Copy(src, dst);
            }
            else
            {
                throw new FileNotFoundException(src);
            }
        }
    }
}
