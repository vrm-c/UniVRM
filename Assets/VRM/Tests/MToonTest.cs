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

            var textureManager = new TextureExporter(AssetTextureUtil.IsTextureEditorAsset);
            var srcMaterial = new Material(Shader.Find("VRM/MToon"));

            var offset = new Vector2(0.3f, 0.2f);
            var scale = new Vector2(0.5f, 0.6f);

            srcMaterial.mainTexture = tex0;
            srcMaterial.mainTextureOffset = offset;
            srcMaterial.mainTextureScale = scale;

            var materialExporter = new VRMMaterialExporter();
            var vrmMaterial = VRMMaterialExporter.CreateFromMaterial(srcMaterial, textureManager);
            Assert.AreEqual(vrmMaterial.vectorProperties["_MainTex"], new float[] { 0.3f, 0.2f, 0.5f, 0.6f });

            var materialImporter = new VRMMaterialImporter(new glTF_VRM_extensions
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

            var parser = new GltfParser();
            parser.ParsePath(path);

            var importer = new VRMImporterContext(parser, null);

            var materialImporter = new VRMMaterialImporter(importer.VRM);

            Assert.AreEqual(73, parser.GLTF.materials.Count);
            Assert.True(materialImporter.TryCreateParam(parser, 0, out MaterialImportParam param));
        }
    }
}
