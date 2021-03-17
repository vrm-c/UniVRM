using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public class LoadTests
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
                    case ".gltf":
                    case ".glb":
                        yield return child;
                        break;
                }
            }
        }

        static void Load(FileInfo gltf)
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
                using (var importer = new ImporterContext(parser))
                {
                    importer.Load();
                }
            }
            catch (UniGLTFNotSupportedException)
            {
                // skip
            }
            catch (Exception ex)
            {
                Debug.LogError($"LoadError: {gltf}");
                Debug.LogException(ex);
            }
        }

        [Test]
        public void GltfSampleModelsTests()
        {
            var env = System.Environment.GetEnvironmentVariable("GLTF_SAMPLE_MODELS");
            if (string.IsNullOrEmpty(env))
            {
                return;
            }
            var root = new DirectoryInfo($"{env}/2.0");
            if (!root.Exists)
            {
                return;
            }

            foreach (var gltf in EnumerateGltfFiles(root))
            {
                Load(gltf);
            }
        }
    }
}
