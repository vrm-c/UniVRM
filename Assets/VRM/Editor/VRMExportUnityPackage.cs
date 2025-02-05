using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UniGLTF;

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

    /// <summary>
    /// TODO: 本来このクラスは「パッケージとしての UniVRM」のスコープのクラスであるが、「UPM Package VRM」のスコープにコードがあるので変
    /// </summary>
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
                UniGLTF.PackageVersion.VERSION,
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
        /// -batchMode -silent-crashes -projectPath . -executeMethod VRM.DevOnly.PackageExporter.VRMExportUnityPackage.CreateUnityPackageWithBuild
        /// </summary>
        public static void CreateUnityPackageWithBuild()
        {
            try
            {
                UniGLTFLogger.Log($"[{nameof(VRMExportUnityPackage)}] Start CreateUnityPackageWithBuild...");
                var folder = GetProjectRoot();
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                UniGLTFLogger.Log($"[{nameof(VRMExportUnityPackage)}] Try to build test scenes...");
                BuildTestScene();

                UniGLTFLogger.Log($"[{nameof(VRMExportUnityPackage)}] Create UnityPackages...");
                CreateUnityPackages(folder);

                UniGLTFLogger.Log($"[{nameof(VRMExportUnityPackage)}] Finish CreateUnityPackageWithBuild");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception e)
            {
                UniGLTFLogger.Exception(e);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
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

        private static void CreateUnityPackages(string outputDir)
        {
            if (!VRMSampleCopy.Validate())
            {
                throw new Exception("SampleCopy is not same !");
            }

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

        private static void CreateUnityPackage(
            string outputDir,
            PackageInfo package
            )
        {
            var targetFileNames = package.List.SelectMany(x => x.Files).ToArray();

            UniGLTFLogger.Log($"Package '{package.Name}' will include {targetFileNames.Count()} files...");
            UniGLTFLogger.Log($"{string.Join("", targetFileNames.Select((x, i) => string.Format("[{0:##0}] {1}\n", i, x)).ToArray())}");

            var path = MakePackagePathName(outputDir, package.Name);
            AssetDatabase.ExportPackage(targetFileNames, path, ExportPackageOptions.Default);
        }

        private static void BuildTestScene()
        {
            var levels = new string[]
            {
                "Assets/UniGLTF_Samples/GltfViewer/GltfViewer.unity",
                "Assets/VRM_Samples/SimpleViewer/SimpleViewer.unity",
                "Assets/VRM10_Samples/VRM10Viewer/VRM10Viewer.unity",
            };
            Build(levels);
        }

        private static void Build(string[] levels)
        {
            var buildPath = Path.GetFullPath(Application.dataPath + "/../build/build.exe");
            UniGLTFLogger.Log($"BuildPath: {buildPath}");
            var build = BuildPipeline.BuildPlayer(levels,
                buildPath,
                BuildTarget.StandaloneWindows,
                BuildOptions.None
            );
            if (build.summary.result != BuildResult.Succeeded)
            {
                throw new Exception("Failed to build scenes");
            }
        }
    }
}
