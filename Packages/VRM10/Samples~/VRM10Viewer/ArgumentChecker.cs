using System.IO;
using System.Linq;
using UnityEngine;


namespace UniVRM10.VRM10Viewer
{
    static class ArgumentChecker
    {
        static string[] Supported = {
                ".gltf",
                ".glb",
                ".vrm",
                ".zip",
            };

        static string UnityHubPath => System.Environment.GetEnvironmentVariable("ProgramFiles") + "\\Unity\\Hub";

        public static bool IsLoadable(string path)
        {
            if (!File.Exists(path))
            {
                // not exists
                return false;
            }

            if (Application.isEditor)
            {
                // skip editor argument
                // {UnityHub_Resources}\PackageManager\ProjectTemplates\com.unity.template.3d-5.0.4.tgz
                if (path.StartsWith(UnityHubPath))
                {
                    return false;
                }
            }

            var ext = Path.GetExtension(path).ToLower();
            if (!Supported.Contains(ext))
            {
                // unknown extension
                return false;
            }

            return true;
        }

        public static bool TryGetFirstLoadable(out string cmd)
        {
            foreach (var arg in System.Environment.GetCommandLineArgs())
            {
                if (ArgumentChecker.IsLoadable(arg))
                {
                    cmd = arg;
                    return true;
                }
            }

            cmd = default;
            return false;
        }
    }
}