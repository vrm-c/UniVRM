using System.IO;
using System.Linq;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;

namespace UniVRM10.Test
{
    public class LoadTests
    {
        [Test]
        public void EmptyThumbnailName()
        {
            using (var data = new GlbFileParser(TestAsset.AliciaPath).Parse())
            using (var migrated = Vrm10Data.Migrate(data, out var vrm1Data, out var migration))
            {
                // Vrm10Data.ParseOrMigrate(TestAsset.AliciaPath, true, out Vrm10Data vrm, out MigrationData migration))
                Assert.NotNull(vrm1Data);

                var index = vrm1Data.VrmExtension.Meta.ThumbnailImage.Value;

                // empty thumbnail name
                vrm1Data.Data.GLTF.images[index].name = null;

                using (var loader = new Vrm10Importer(vrm1Data))
                {
                    loader.LoadAsync(new ImmediateCaller()).Wait();
                }
            }
        }

        static string ModelPath
        {
            get
            {
                // submodule
                return Path.GetFullPath(Application.dataPath + "/../vrm-specification/samples/Seed-san/vrm/Seed-san.vrm")
                    .Replace("\\", "/");
            }
        }

        static int getByteLength(glTF gltf, int accessorIndex)
        {
            if (accessorIndex < 0) { return 0; }
            var accessor = gltf.accessors[accessorIndex];
            return accessor.CalcByteSize();
        }

        static int getByteLength(glTF gltf, glTFPrimitives prim)
        {
            var l = 0;
            l += getByteLength(gltf, prim.indices);
            l += getByteLength(gltf, prim.attributes.POSITION);
            l += getByteLength(gltf, prim.attributes.NORMAL);
            l += getByteLength(gltf, prim.attributes.TANGENT);
            l += getByteLength(gltf, prim.attributes.TEXCOORD_0);
            l += getByteLength(gltf, prim.attributes.JOINTS_0);
            l += getByteLength(gltf, prim.attributes.WEIGHTS_0);
            foreach (var morph in prim.targets)
            {
                l += getByteLength(gltf, morph.POSITION);
                l += getByteLength(gltf, morph.NORMAL);
                l += getByteLength(gltf, morph.TANGENT);
            }
            return l;
        }

        static int getByteLength(glTF gltf, glTFMesh mesh)
        {
            var l = 0;
            foreach (var prim in mesh.primitives)
            {
                l += getByteLength(gltf, prim);
            }
            return l;
        }

        [Test]
        public void NoTexture()
        {
            if (!File.Exists(ModelPath))
            {
                return;
            }

            // load model
            var task = Vrm10.LoadPathAsync(ModelPath, awaitCaller: new ImmediateCaller());
            var instance = task.Result;

            // remove textures
            instance.Vrm.Meta.Thumbnail = null;

            foreach (var r in instance.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterials = r.sharedMaterials.Select(x =>
                {
                    var m = new Material(Shader.Find("UniGLTF/UniUnlit"));
                    return m;
                }).ToArray();
            }

            var settings = new GltfExportSettings();

            // export as vrm1
            using (var arrayManager = new NativeArrayManager())
            {
                var converter = new UniVRM10.ModelExporter();
                var model = converter.Export(settings, arrayManager, instance.gameObject);

                // 右手系に変換
                Debug.Log($"convert to right handed coordinate...");
                model.ConvertCoordinate(VrmLib.Coordinates.Vrm1, ignoreVrm: false);

                // export vrm-1.0
                var exporter = new Vrm10Exporter(settings);
                exporter.Export(instance.gameObject, model, converter, new VrmLib.ExportArgs
                {
                    sparse = false,
                });

                var gltf = exporter.Storage.Gltf;

                var last = gltf.bufferViews.Last();
                // https://github.com/vrm-c/UniVRM/pull/2413
                Assert.AreEqual(last.byteOffset + last.byteLength, exporter.Storage.Gltf.buffers[0].byteLength);

                // check byteLength
                var expected = 0;
                foreach (var mesh in gltf.meshes)
                {
                    expected += getByteLength(gltf, mesh);
                }
                foreach (var skin in gltf.skins)
                {
                    expected += getByteLength(gltf, skin.inverseBindMatrices);
                }
                // alighment ?
                // Assert.AreEqual(expected, exporter.Storage.Gltf.buffers[0].byteLength);
            }
        }
    }
}
