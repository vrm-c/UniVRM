using System;
using System.IO;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;
using UnityEngine.TestTools;

namespace UniVRM10.Test
{
    public class ApiSampleTests
    {
        VrmLib.Model ReadModel(string path)
        {
            var bytes = MigrationVrm.Migrate(File.ReadAllBytes(path));

            var parser = new GltfParser();
            parser.Parse("migrated", bytes);

            var model = UniVRM10.VrmLoader.CreateVrmModel(parser);
            return model;
        }

        GameObject BuildGameObject(GltfParser parser, bool showMesh)
        {
            using (var loader = new RuntimeUnityBuilder(parser))
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

        VrmLib.Model ToModel(UnityEngine.GameObject target)
        {
            var exporter = new UniVRM10.RuntimeVrmConverter();
            var model = exporter.ToModelFrom10(target);
            return model;
        }

        byte[] ToVrm10(VrmLib.Model model)
        {
            // 右手系に変換
            VrmLib.ModelExtensionsForCoordinates.ConvertCoordinate(model, VrmLib.Coordinates.Vrm1);
            var bytes = UniVRM10.ModelExtensions.ToGlb(model);
            return bytes;
        }

        [Test]
        public void Sample()
        {
            var path = "Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm";
            Debug.Log($"load: {path}");

            var migrated = MigrationVrm.Migrate(File.ReadAllBytes(path));

            var parser = new GltfParser();
            parser.Parse(path, migrated);

            var asset = BuildGameObject(parser, true);
            Debug.Log(asset);

            // export
            var dstModel = ToModel(asset);
            Debug.Log(dstModel);

            var vrmBytes = ToVrm10(dstModel);
            Debug.Log($"export {vrmBytes.Length} bytes");
        }
    }
}
