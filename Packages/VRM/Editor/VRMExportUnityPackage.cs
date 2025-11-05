using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UniGLTF;
using System.Reflection;
using NUnit.Framework.Constraints;





#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

namespace VRM.DevOnly.PackageExporter
{
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
                GetGitHash(Application.dataPath + "/../Packages/VRM").Substring(0, 4)
                ).Replace("\\", "/");

            return path;
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



        private static void CreateUnityPackages(string outputDir)
        {
            if (!VRMSampleCopy.Validate())
            {
                throw new Exception("SampleCopy is not same !");
            }

            UnityPackageFromPackages(outputDir, "UniVRM", new[] { "com.vrmc.gltf", "com.vrmc.univrm" });
            UnityPackageFromPackages(outputDir, "VRM", new[] { "com.vrmc.gltf", "com.vrmc.vrm" });
        }

        private delegate string[] CollectAllChildren(string guid, string[] collection);
        private static CollectAllChildren GetInternal_AssetDatabase_CollectAllChildren()
        {
            var mi = typeof(AssetDatabase).GetMethod("CollectAllChildren", BindingFlags.NonPublic | BindingFlags.Static);
            return (CollectAllChildren)mi.CreateDelegate(typeof(CollectAllChildren));
        }

        private delegate void ExportPackage(string[] guids, string fileName);
        private static ExportPackage GetInternal_PackageUtility_ExportPackage()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var mi = assembly.GetType("UnityEditor.PackageUtility").GetMethod("ExportPackage");
            return (ExportPackage)mi.CreateDelegate(typeof(ExportPackage));
        }

        /// <summary>
        /// Packages から UnityPackage を作成する。
        /// UnityEditor.PackageUtility.ExportPackage に帰結する。
        /// UnityEditor.PackageUtility は internal。
        /// `v0.131.0`
        /// </summary>
        /// <param name="outputDir"></param>
        /// <param name="name">NAME-x.y.z.unitypackage</param>
        /// <param name="packages">com.vrmc.gltf etc</param>
        private static void UnityPackageFromPackages(string outputDir, string name, string[] packages)
        {
            // Packages asset path is `Package/pkg_name`. not file path !
            // if use file path, return ""
            var rootGuids = packages.Select(x => AssetDatabase.AssetPathToGUID("Packages/" + x));

            var collectAllChildren = GetInternal_AssetDatabase_CollectAllChildren();
            var collection = new string[0];
            foreach (var guid in rootGuids)
            {
                collection = collectAllChildren(guid, collection);
            }

            var path = MakePackagePathName(outputDir, name);
            UniGLTFLogger.Log($"'{Path.GetFileName(path)}' will include {string.Join(", ", packages)}. {collection.Length} files...");

            var exportPackage = GetInternal_PackageUtility_ExportPackage();
            exportPackage(collection, path);
        }

        public static UnityEditor.PackageManager.PackageInfo GetPackageInfo(string packageName)
        {
            var request = UnityEditor.PackageManager.Client.List(true, true);
            while (!request.IsCompleted) { }
            if (request.Status == UnityEditor.PackageManager.StatusCode.Success) { return request.Result.FirstOrDefault(pkg => pkg.name == packageName); }
            return null;
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
