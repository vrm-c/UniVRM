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
        static string GetProjectRoot()
        {
            return Path.GetFullPath(Application.dataPath + "/..");
        }

        static string System(string workingDir, string fileName, string args)
        {
            // Start the child process.
            using (var p = new System.Diagnostics.Process())
            {
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

                if (p.ExitCode != 0 || string.IsNullOrEmpty(output))
                {
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

        static readonly string[] ignoredFilesForGlob = new string[] {
            ".git",
            ".circleci",
            "DevOnly",
            "doc",
            "Profiling",
        };

        static IEnumerable<string> GlobFiles(string path)
        {
            var fileName = Path.GetFileName(path);

            // Domain specific filter logic
            if (ignoredFilesForGlob.Any(f => fileName.EndsWithAndMeta(f)))
            {
                yield break;
            }

            if (Directory.Exists(path))
            {
                // folder
                yield return path.Replace("\\", "/");

                foreach (var child in Directory.GetFileSystemEntries(path))
                {
                    foreach (var x in GlobFiles(child))
                    {
                        yield return x;
                    }
                }
            }
            else
            {
                // file
                if (Path.GetExtension(path).ToLower() == ".meta")
                {
                    yield break;
                }

                yield return path.Replace("\\", "/");
            }
        }

        public static void CreateUnityPackageWithoutBuild()
        {
            var folder = GetProjectRoot();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            CreateUnityPackages(folder);
        }

        /// <summary>
        /// This is call from Jenkins build
        ///
        /// -quit -batchMode -executeMethod VRM.DevOnly.PackageExporter.VRMExportUnityPackage.CreateUnityPackageWithBuild
        /// </summary>
        public static void CreateUnityPackageWithBuild()
        {
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

        public class GlobList
        {
            public readonly string[] Files;

            public GlobList(string root, params string[] filters)
            {
                var files = GlobFiles(root);
                if (filters.Any())
                {
                    var filtersWithRoot = filters.Select(x => $"{root}/{x}").ToArray();
                    // filtering
                    Files = files.Where(x => filtersWithRoot.Any(y => x.StartsWith(y))).ToArray();
                }
                else
                {
                    // no filter. all files
                    Files = files.ToArray();
                }
            }
        }

        public class PackageInfo
        {
            public readonly string Name;
            public GlobList[] List;

            public PackageInfo(string name)
            {
                Name = name;
            }
        }

        public static void CreateUnityPackages(string outputDir)
        {
            {
                var packages = new[]{
                    // VRM
                    new PackageInfo("UniVRM")
                    {
                        List = new []{
                            new GlobList("Assets/VRMShaders"),
                            new GlobList("Assets/UniGLTF"),
                            new GlobList("Assets/VRM"),
                        }
                    },
                    // VRM_Samples
                    new PackageInfo("UniVRM_Samples")
                    {
                        List = new []{
                            new GlobList("Assets/VRM_Samples"),
                        }
                    },
                    // VRM-1.0
                    new PackageInfo("VRM")
                    {
                        List = new []{
                            new GlobList("Assets/VRMShaders"),
                            new GlobList("Assets/UniGLTF"),
                            new GlobList("Assets/VRM10"),
                        }
                    },
                    // VRM-1.0_Samples
                    new PackageInfo("VRM_Samples")
                    {
                        List = new []{
                            new GlobList("Assets/VRM10_Samples"),
                        }
                    },
                };
                foreach (var package in packages)
                {
                    CreateUnityPackage(outputDir, package);
                }
            }
        }

        public static void CreateUnityPackage(
            string outputDir,
            PackageInfo package
            )
        {
            var targetFileNames = package.List.SelectMany(x => x.Files).ToArray();

            Debug.LogFormat("Package '{0}' will include {1} files...", package.Name, targetFileNames.Count());
            Debug.LogFormat("{0}", string.Join("", targetFileNames.Select((x, i) => string.Format("[{0:##0}] {1}\n", i, x)).ToArray()));

            var path = MakePackagePathName(outputDir, package.Name);
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
            var isSuccess = build.summary.result == BuildResult.Succeeded;
#else
            var isSuccess = !string.IsNullOrEmpty(build);
#endif
            return isSuccess;
        }
    }
}
