using System;
using System.IO;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;
using UnityEngine.TestTools;
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

        byte[] ToVrm10(GameObject root, RuntimeVrmConverter converter, VrmLib.Model model)
        {
            // 右手系に変換
            VrmLib.ModelExtensionsForCoordinates.ConvertCoordinate(model, VrmLib.Coordinates.Vrm1);
            var bytes = UniVRM10.ModelExtensions.ToGlb(model, root, converter, AssetTextureUtil.GetTextureBytesWithMime);
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

            var go = BuildGameObject(parser, true);
            Debug.Log(go);

            // export
            // var dstModel = ToModel(go);
            var exporter = new UniVRM10.RuntimeVrmConverter();
            var dstModel = exporter.ToModelFrom10(go);
            // return model;

            Debug.Log(dstModel);

            var vrmBytes = ToVrm10(go, exporter, dstModel);
            Debug.Log($"export {vrmBytes.Length} bytes");
        }
    }
}
