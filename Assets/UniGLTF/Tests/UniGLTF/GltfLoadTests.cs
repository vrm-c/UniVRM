using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

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

        static void RuntimeLoad(FileInfo gltf, int subStrStart)
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
            catch (Exception ex)
            {
                Message(gltf.FullName.Substring(subStrStart), ex);
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
            var gltfTextures = GltfTextureEnumerator.Enumerate(parser).ToArray();
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
                RuntimeLoad(gltf, root.FullName.Length);

                EditorLoad(gltf, root.FullName.Length);
            }
        }
    }
}
