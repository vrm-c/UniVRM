using NUnit.Framework;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class TextureTests
    {
        [Test]
        public void TextureExportTest()
        {
            // Dummy texture
            var tex0 = new Texture2D(128, 128)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
            };
            var textureManager = new TextureExporter(AssetTextureUtil.IsTextureEditorAsset);

            var material = new Material(Shader.Find("Standard"));
            material.mainTexture = tex0;

            var materialExporter = new MaterialExporter();
            materialExporter.ExportMaterial(material, textureManager);

            var convTex0 = textureManager.Exported[0];
            var sampler = TextureSamplerUtil.Export(convTex0);

            Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapS);
            Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapT);
            Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.minFilter);
            Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.magFilter);
        }
    }
}
