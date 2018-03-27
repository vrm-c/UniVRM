using System;
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

            AssetDatabase.ExportPackage("Assets/VRM", path, ExportPackageOptions.Recurse);
            Debug.LogFormat("exported: {0}", path);
        }
    }
}
