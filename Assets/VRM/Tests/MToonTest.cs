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

            var textureManager = new TextureExporter(AssetTextureUtil.IsTextureEditorAsset );
            var srcMaterial = new Material(Shader.Find("VRM/MToon"));

            var offset = new Vector2(0.3f, 0.2f);
            var scale = new Vector2(0.5f, 0.6f);

            srcMaterial.mainTexture = tex0;
            srcMaterial.mainTextureOffset = offset;
            srcMaterial.mainTextureScale = scale;

            var materialExporter = new VRMMaterialExporter();
            var vrmMaterial = VRMMaterialExporter.CreateFromMaterial(srcMaterial, textureManager);
            Assert.AreEqual(vrmMaterial.vectorProperties["_MainTex"], new float[] { 0.3f, 0.2f, 0.5f, 0.6f });

            var materialImporter = new VRMMtoonMaterialImporter(new glTF_VRM_extensions
            {
                materialProperties = new System.Collections.Generic.List<glTF_VRM_Material> { vrmMaterial }
            });
        }
    }
}
