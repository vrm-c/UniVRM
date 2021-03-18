using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public class VRMLoadTest
    {
        static IEnumerable<FileInfo> EnumerateGltfFiles(DirectoryInfo dir)
        {
            if (dir.Name == ".git")
            {
                yield break;
            }

            foreach (var child in dir.EnumerateDirectories())
            {
                foreach (var x in EnumerateGltfFiles(child))
                {
                    yield return x;
                }
            }

            foreach (var child in dir.EnumerateFiles())
            {
                switch (child.Extension.ToLower())
                {
                    case ".vrm":
                        yield return child;
                        break;
                }
            }
        }

        static void Message(string path, Exception exception)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            {
                Debug.LogError($"LoadError: {path}");
                Debug.LogException(exception);
            }
        }

        static void Load(FileInfo gltf, DirectoryInfo root)
        {
            var parser = new GltfParser();
            try
            {
                parser.ParsePath(gltf.FullName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseError: {gltf}");
                Debug.LogException(ex);
            }

            try
            {
                using (var importer = new VRMImporterContext(parser))
                {
                    importer.Load();
                }
            }
            catch (Exception ex)
            {
                Message(gltf.FullName.Substring(root.FullName.Length), ex);
            }
        }

        [Test]
        public void VrmTestModelsTests()
        {
            var env = System.Environment.GetEnvironmentVariable("VRM_TEST_MODELS");
            if (string.IsNullOrEmpty(env))
            {
                return;
            }
            var root = new DirectoryInfo(env);
            if (!root.Exists)
            {
                return;
            }

            foreach (var gltf in EnumerateGltfFiles(root))
            {
                Load(gltf, root);
            }
        }
    }
}
