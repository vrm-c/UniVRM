using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class GltfLoadTests
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

        static void Message(string path, Exception exception)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            if (exception is UniGLTFNotSupportedException ex)
            {
                Debug.LogWarning($"LoadError: {path}: {ex}");
            }
            else
            {
                Debug.LogError($"LoadError: {path}");
                Debug.LogException(exception);
            }
        }

        static Byte[] Export(GameObject root)
        {
            var gltf = new glTF();
            using (var exporter = new gltfExporter(gltf))
            {
                exporter.Prepare(root);
                exporter.Export(MeshExportSettings.Default, AssetTextureUtil.IsTextureEditorAsset, AssetTextureUtil.GetTextureBytesWithMime);
                return gltf.ToGlbBytes();
            }
        }

        // Unsolved Animation Export issue
        //
        // QuaternionToEuler: Input quaternion was not normalized
        //
        static string[] Skip = new string[]
        {
            "BrainStem",
            "RiggedSimple"
        };

        static void RuntimeLoadExport(FileInfo gltf, int subStrStart)
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

            using (var loader = new ImporterContext(parser))
            {
                try
                {
                    loader.Load();
                }
                catch (Exception ex)
                {
                    Message(gltf.FullName.Substring(subStrStart), ex);
                }

                if (Skip.Contains(gltf.Directory.Parent.Name))
                {
                    // Export issue:                   
                    // skip
                    return;
                }

                if (loader.Root == null)
                {
                    Debug.LogWarning($"root is null: ${gltf}");
                    return;
                }

                Export(loader.Root);
            }
        }

        /// <summary>
        /// Extract をテスト
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="root"></param>
        static void EditorLoad(FileInfo gltf, int subStrStart)
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

            // should unique
            var gltfTextures = GltfTextureEnumerator.EnumerateAllTexturesDistinct(parser).ToArray();
            var distinct = gltfTextures.Distinct().ToArray();
            Assert.True(gltfTextures.SequenceEqual(distinct));
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
                RuntimeLoadExport(gltf, root.FullName.Length);

                EditorLoad(gltf, root.FullName.Length);
            }
        }

        // [Test]
        public void GltfSampleModelsTest_BrainStem()
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

            // foreach (var gltf in EnumerateGltfFiles(root))
            {
                var gltf = new FileInfo(Path.Combine(root.FullName, "BrainStem/glTF-Binary/BrainStem.glb"));
                RuntimeLoadExport(gltf, root.FullName.Length);

                EditorLoad(gltf, root.FullName.Length);
            }
        }
    }
}
