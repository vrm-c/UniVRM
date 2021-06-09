using System.IO;
using NUnit.Framework;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEngine;
using VRMShaders;

namespace UniVRM10.Test
{
    public class ApiSampleTests
    {
        VrmLib.Model ReadModel(string path)
        {
            var bytes = MigrationVrm.Migrate(File.ReadAllBytes(path));

            var parser = new GltfParser();
            parser.Parse("migrated", bytes);

            var model = ModelReader.Read(parser);
            return model;
        }

        GameObject BuildGameObject(GltfParser parser, VRMC_vrm vrm, bool showMesh)
        {
            using (var loader = new Vrm10Importer(parser, vrm))
            {
                loader.Load();
                if (showMesh)
                {
                    loader.ShowMeshes();
                }
                loader.EnableUpdateWhenOffscreen();
                return loader.DisposeOnGameObjectDestroyed().gameObject;
            }
        }

        [Test]
        public void Sample()
        {
            var path = "Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm";
            Debug.Log($"load: {path}");

            Assert.IsTrue(Vrm10Parser.TryParseOrMigrate(path, true, out Vrm10Parser.Result result, out string error));

            var go = BuildGameObject(result.Parser, result.Vrm, true);
            Debug.Log(go);

            // export
            var vrmBytes = Vrm10Exporter.Export(go, new EditorTextureSerializer());

            Debug.Log($"export {vrmBytes.Length} bytes");
        }
    }
}
