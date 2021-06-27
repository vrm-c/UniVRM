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

            var parser = new IGltfData();
            parser.Parse("migrated", bytes);

            var model = ModelReader.Read(parser);
            return model;
        }

        GameObject BuildGameObject(IGltfData data, VRMC_vrm vrm, bool showMesh)
        {
            using (var loader = new Vrm10Importer(data, vrm))
            {
                var loaded = loader.Load();
                if (showMesh)
                {
                    loaded.ShowMeshes();
                }
                loaded.EnableUpdateWhenOffscreen();
                return loaded.gameObject;
            }
        }

        [Test]
        public void Sample()
        {
            var path = "Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm";
            Debug.Log($"load: {path}");

            Assert.IsTrue(Vrm10Parser.TryParseOrMigrate(path, true, out Vrm10Parser.Result result));

            var go = BuildGameObject(result.Data, result.Vrm, true);
            Debug.Log(go);

            // export
            var vrmBytes = Vrm10Exporter.Export(go, new EditorTextureSerializer());

            Debug.Log($"export {vrmBytes.Length} bytes");
        }
    }
}
