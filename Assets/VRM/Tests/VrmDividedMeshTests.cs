using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public class DividedMeshTests
    {
        static string AliciaPath
        {
            get
            {
                return Path.GetFullPath(Application.dataPath + "/../Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm")
                    .Replace("\\", "/");
            }
        }

        static GameObject Load(byte[] bytes, string path)
        {
            var parser = new GltfParser();
            parser.Parse(path, bytes);

            using (var loader = new VRMImporterContext(parser))
            {
                loader.Load();
                loader.ShowMeshes();
                return loader.DisposeOnGameObjectDestroyed().gameObject;
            }
        }

        static IEnumerable<Mesh> GetMeshes(GameObject gameObject)
        {
            foreach (var r in gameObject.GetComponentsInChildren<Renderer>())
            {
                if (r is SkinnedMeshRenderer smr)
                {
                    yield return smr.sharedMesh;
                }
                else if (r is MeshRenderer mr)
                {
                    yield return r.GetComponent<MeshFilter>().sharedMesh;
                }
            }
        }

        /// <summary>
        /// positions: [
        ///   {1, 1, 0}
        ///   {1, 1, 1}
        ///   {1, 1, 2}
        ///   {1, 1, 3}
        ///   {1, 1, 4}
        ///   {1, 1, 5}
        /// ]
        /// submesh
        ///     0 1 2
        /// submesh
        ///     3 4 5
        /// </summary>
        [Test]
        public void ExportDividedMeshTest()
        {
            var path = AliciaPath;
            var loaded = Load(File.ReadAllBytes(path), path);

            var exported = VRMExporter.Export(new UniGLTF.MeshExportSettings
            {
                DivideVertexBuffer = true, // test this
                ExportOnlyBlendShapePosition = true,
                ExportTangents = false,
                UseSparseAccessorForMorphTarget = true,
            }, loaded, new EditorTextureSerializer());
            var bytes = exported.ToGlbBytes();
            var divided = Load(bytes, path);

            var src = GetMeshes(loaded).ToArray();
            var div = GetMeshes(divided).ToArray();

            Assert.AreEqual(src[0].triangles.Length, div[0].triangles.Length);
        }
    }
}
