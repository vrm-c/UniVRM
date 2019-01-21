using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

namespace VRM.DevOnly.PackageExporter
{
    public static class StringExtensionsForUnity
    {
        public static bool EndsWithAndMeta(this string str, string terminator)
        {
            if (str.EndsWith(terminator))
            {
                return true;
            }
            return str.EndsWith(terminator + ".meta");
        }
    }

    public static class VRMExportUnityPackage
    {
        const string DATE_FORMAT = "yyyyMMdd";

        static string GetDesktop()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/VRM";
        }

        static string GetProjectRoot()
        {
            return Path.GetFullPath(Application.dataPath + "/..");
        }

        static string System(string workingDir, string fileName, string args)
        {
            // Start the child process.
            using(var p = new System.Diagnostics.Process()) {
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = fileName;
                p.StartInfo.Arguments = args;
                p.StartInfo.WorkingDirectory = workingDir;

                p.Start();

                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                string err = p.StandardError.ReadToEnd();
                p.WaitForExit();

                if (p.ExitCode != 0 || string.IsNullOrEmpty(output)) {
                    throw new Exception(err);
                }

                return output;
            }
        }

        static string GetGitHash(string path)
        {
            return System(path, "git", "rev-parse HEAD").Trim();
        }

        static string MakePackagePathName(string folder, string prefix)
        {
            //var date = DateTime.Today.ToString(DATE_FORMAT);

            var path = string.Format("{0}/{1}-{2}_{3}.unitypackage",
                folder,
                prefix,
                VRMVersion.VERSION,
                GetGitHash(Application.dataPath + "/VRM").Substring(0, 4)
                ).Replace("\\", "/");

            return path;
        }

        static readonly string[] ignoredFilesForGrob = new string[] {
            ".git",
            ".circleci",
            "DevOnly",
            "doc",
            "Profiling",
        };

        static IEnumerable<string> GrobFiles(string path)
        {
            var fileName = Path.GetFileName(path);

            // Domain specific filter logic
            if (ignoredFilesForGrob.Any(f => fileName.EndsWithAndMeta(f))) {
                yield break;
            }

            if (Directory.Exists(path))
            {
                foreach (var child in Directory.GetFileSystemEntries(path))
                {
                    foreach (var x in GrobFiles(child))
                    {
                        yield return x;
                    }
                }
            }
            else
            {
                if (Path.GetExtension(path).ToLower() == ".meta")
                {
                    yield break;
                }

                yield return path.Replace("\\", "/");
            }
        }

        [MenuItem(VRMVersion.MENU + "/Export unitypackage")]
        public static void CreateUnityPackageWithBuild()
        {
            //var folder = GetDesktop();
            var folder = GetProjectRoot();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!BuildTestScene())
            {
                Debug.LogError("Failed to build test scenes");
            }
            CreateUnityPackages(folder);
        }

        public static void CreateUnityPackage()
        {
            CreateUnityPackages(Path.GetFullPath(Path.Combine(Application.dataPath, "..")));
        }

        static bool EndsWith(string path, params string[] exts)
        {
            foreach(var ext in exts)
            {
                if (path.EndsWith(ext))
                {
                    return true;
                }
                if(path.EndsWith(ext + ".meta"))
                {
                    return true;
                }
            }

            return false;
        }

        public static void CreateUnityPackages(string outputDir)
        {
            // UniVRM and sub packages
            {
                var basePath = "Assets/VRM";
                var packages = new Dictionary<string, string[]> () {
                    {"UniVRM", null}, // All
                    {"UniJSON-standalone", new string[] {"UniJSON"}},
                    {"UniHumanoid-standalone", new string[] {"UniHumanoid"}},
                    {"UniGLTF-standalone", new string[] {"UniGLTF", "UniHumanoid", "UniJSON", "UniUnlit", "DepthFirstScheduler"}},
                };

                var fileNames = GrobFiles(basePath).ToArray();
                foreach(var packagePair in packages) {
                    CreateUnityPackage(outputDir, packagePair.Key, packagePair.Value, basePath, fileNames);
                }
            }

            // UniVRM Samples
            {
                var fileNames = GrobFiles("Assets/VRM.Samples")
                    .Concat(GrobFiles("Assets/StreamingAssets/VRM.Samples"))
                    .ToArray();
                CreateUnityPackage(outputDir, "UniVRM-samples", null /*All*/, "", fileNames);
            }
        }

        public static void CreateUnityPackage(string outputDir, string name, string[] containsPath, string basePath, string[] fileNames) {
            var targetFileNames = fileNames;
            if (containsPath != null)
            {
                var containsPathWithBase = containsPath.Select(c => string.Format("{0}/{1}", basePath, c)).ToArray();
                targetFileNames = targetFileNames
                    .Where(fileName => containsPathWithBase.Any(c => fileName.StartsWith(c)))
                    .ToArray();
            }

            Debug.LogFormat("Package '{0}' will include {1} files...", name, targetFileNames.Count());
            Debug.LogFormat("{0}", string.Join("", targetFileNames.Select((x, i) => string.Format("[{0:##0}] {1}\n", i, x)).ToArray()));

            var path = MakePackagePathName(outputDir, name);
            AssetDatabase.ExportPackage(targetFileNames, path, ExportPackageOptions.Default);
        }

        public static bool BuildTestScene()
        {
            var levels = new string[] { "Assets/VRM.Samples/Scenes/VRMRuntimeLoaderSample.unity" };
            return Build(levels);
        }

        public static bool Build(string[] levels)
        {
            var buildPath = Path.GetFullPath(Application.dataPath + "/../build/build.exe");
            Debug.LogFormat("BuildPath: {0}", buildPath);
            var build = BuildPipeline.BuildPlayer(levels,
                buildPath,
                BuildTarget.StandaloneWindows,
                BuildOptions.None
            );
#if UNITY_2018_1_OR_NEWER
            var isSuccess = build.summary.result != BuildResult.Succeeded;
#else
            var isSuccess = !string.IsNullOrEmpty(build);
#endif
            return isSuccess;
        }
    }
}
