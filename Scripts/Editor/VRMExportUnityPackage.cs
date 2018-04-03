using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    public static class VRMExportUnityPackage
    {
        static string GetDesktop()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/VRM";
        }

        const string DATE_FORMAT = "yyyyMMdd";
        const string PREFIX = "UniVRM";

        static string GetPath()
        {
            var folder = GetDesktop();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            //var date = DateTime.Today.ToString(DATE_FORMAT);

            var path = string.Format("{0}/{1}-{2}.unitypackage",
                folder,
                PREFIX,
                VRMVersion.VERSION
                ).Replace("\\", "/");

            return path;
        }

        static IEnumerable<string> EnumerateFiles(string path)
        {
            if (Path.GetFileName(path).StartsWith(".git"))
            {
                yield break;
            }

            if (Directory.Exists(path))
            {
                foreach(var child in Directory.GetFileSystemEntries(path))
                {
                    foreach(var x in EnumerateFiles(child))
                    {
                        yield return x;
                    }
                }
            }
            else
            {
                if (Path.GetExtension(path).ToLower() != ".meta")
                {
                    yield return path.Replace("\\", "/");
                }
            }
        }

#if VRM_DEVELOP
        [MenuItem("VRM/Export unitypackage")]
#endif
        public static void CreateUnityPackage()
        {
            var path = GetPath();

            if (File.Exists(path))
            {
                Debug.LogErrorFormat("{0} is already exists", path);
                return;
            }

            var files = EnumerateFiles("Assets/VRM")
                .ToArray();

            AssetDatabase.ExportPackage(files, path, ExportPackageOptions.Interactive);
            Debug.LogFormat("exported: {0}", path);
        }
    }
}
