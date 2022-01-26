using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;
using VRMShaders;

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

        static GameObject Load(FileInfo gltf, DirectoryInfo root, byte[] bytes = null)
        {
            GltfData data = null;
            try
            {
                if (bytes != null)
                {
                    data = new GlbLowLevelParser(gltf.FullName, bytes).Parse();
                }
                else
                {
                    data = new GlbFileParser(gltf.FullName).Parse();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseError: {gltf}");
                Debug.LogException(ex);
                return null;
            }

            try
            {
                using (data)
                using (var importer = new VRMImporterContext(new VRMData(data)))
                {
                    return importer.Load().gameObject;
                }
            }
            catch (Exception ex)
            {
                Message(gltf.FullName.Substring(root.FullName.Length), ex);
                return null;
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
                // import
                var go = Load(gltf, root);
                try
                {
                    // export
                    var vrm = VRMExporter.Export(new UniGLTF.GltfExportSettings(), go, new EditorTextureSerializer());

                    // re import
                    if (vrm != null)
                    {
                        Load(gltf, root, vrm.ToGlbBytes());
                    }
                }
                finally
                {
                    GameObject.DestroyImmediate(go);
                }
            }
        }
    }
}
