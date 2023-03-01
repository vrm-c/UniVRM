using System.IO;
using System.Linq;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public class MToonTests
    {
        [Test]
        public void TextureTransformTest()
        {
            var tex0 = new Texture2D(128, 128)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };

            var textureExporter = new TextureExporter(new EditorTextureSerializer());
            var srcMaterial = new Material(Shader.Find("VRM/MToon"));

            var offset = new Vector2(0.3f, 0.2f);
            var scale = new Vector2(0.5f, 0.6f);

            srcMaterial.mainTexture = tex0;
            srcMaterial.mainTextureOffset = offset;
            srcMaterial.mainTextureScale = scale;

            var vrmMaterial = BuiltInVrmExtensionMaterialPropertyExporter.ExportMaterial(srcMaterial, textureExporter);
            Assert.AreEqual(vrmMaterial.vectorProperties["_MainTex"], new float[] { 0.3f, 0.2f, 0.5f, 0.6f });

            var materialImporter = new BuiltInVrmMaterialDescriptorGenerator(new glTF_VRM_extensions
            {
                materialProperties = new System.Collections.Generic.List<glTF_VRM_Material> { vrmMaterial }
            });
        }

        [Test]
        public void MToonMaterialParamTest()
        {
            if (!VRMTestAssets.TryGetPath("Models/VRoid/VictoriaRubin/VictoriaRubin.vrm", out string path))
            {
                return;
            }

            using (var data = new GlbFileParser(path).Parse())
            {
                var vrm = new VRMData(data);
                var importer = new VRMImporterContext(vrm, null);

                Assert.AreEqual(73, vrm.Data.GLTF.materials.Count);
                Assert.True(BuiltInVrmMToonMaterialImporter.TryCreateParam(vrm.Data, importer.VRM, 0, out MaterialDescriptor matDesc));
            }
        }

        static string AliciaPath
        {
            get
            {
                return Path.GetFullPath(Application.dataPath + "/../Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm")
                    .Replace("\\", "/");
            }
        }

        [Test]
        public void MaterialImporterTest()
        {
            var path = AliciaPath;
            using (var data = new GlbFileParser(path).Parse())
            {
                var vrmImporter = new VRMImporterContext(new VRMData(data), null);
                var materialParam = new BuiltInVrmMaterialDescriptorGenerator(vrmImporter.VRM).Get(data, 0);
                Assert.AreEqual("VRM/MToon", materialParam.Shader.name);
                Assert.AreEqual("Alicia_body", materialParam.TextureSlots["_MainTex"].UnityObjectName);

                var (key, value) = materialParam.EnumerateSubAssetKeyValue().First();
                Assert.AreEqual(new SubAssetKey(typeof(Texture), "Alicia_body"), key);
            }
        }
    }
}
