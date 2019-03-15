using NUnit.Framework;
using UnityEngine;

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
            var textureManager = new TextureExportManager(new Texture[] {tex0});

            var material = new Material(Shader.Find("Standard"));
            material.mainTexture = tex0;

            var materialExporter = new MaterialExporter();
            materialExporter.ExportMaterial(material, textureManager);

            var convTex0 = textureManager.GetExportTexture(0);
            var sampler = TextureSamplerUtil.Export(convTex0);

            Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapS);
            Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapT);
            Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.minFilter);
            Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.magFilter);
        }
    }

    public class MetallicRoughnessConverterTests
    {
        [Test]
        public void ExportingColorTest()
        {
            {
                var smoothness = 1.0f;
                var conv = new MetallicRoughnessConverter(smoothness);
                Assert.That(
                    conv.Export(new Color32(255, 255, 255, 255)),
                    // r <- 0   : (Unused)
                    // g <- 0   : ((1 - src.a(as float) * smoothness) ^ 2)(as uint8)
                    // b <- 255 : Same metallic (src.r)
                    // a <- 255 : (Unused)
                    Is.EqualTo(new Color32(0, 0, 255, 255)));
            }

            {
                var smoothness = 0.5f;
                var conv = new MetallicRoughnessConverter(smoothness);
                Assert.That(
                    conv.Export(new Color32(255, 255, 255, 255)),
                    // r <- 0   : (Unused)
                    // g <- 63  : ((1 - src.a(as float) * smoothness) ^ 2)(as uint8)
                    // b <- 255 : Same metallic (src.r)
                    // a <- 255 : (Unused)
                    Is.EqualTo(new Color32(0, 63, 255, 255)));
            }

            {
                var smoothness = 0.0f;
                var conv = new MetallicRoughnessConverter(smoothness);
                Assert.That(
                    conv.Export(new Color32(255, 255, 255, 255)),
                    // r <- 0   : (Unused)
                    // g <- 255 : ((1 - src.a(as float) * smoothness) ^ 2)(as uint8)
                    // b <- 255 : Same metallic (src.r)
                    // a <- 255 : (Unused)
                    Is.EqualTo(new Color32(0, 255, 255, 255)));
            }
        }

        [Test]
        public void ImportingColorTest()
        {
            {
                var roughnessFactor = 1.0f;
                var conv = new MetallicRoughnessConverter(roughnessFactor);
                Assert.That(
                    conv.Import(new Color32(255, 255, 255, 255)),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 0   : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 0)));
            }

            {
                var roughnessFactor = 1.0f;
                var conv = new MetallicRoughnessConverter(roughnessFactor);
                Assert.That(
                    conv.Import(new Color32(255, 63, 255, 255)),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 128 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 128))); // smoothness 0.5 * src.a 1.0
            }

            {
                var roughnessFactor = 0.5f;
                var conv = new MetallicRoughnessConverter(roughnessFactor);
                Assert.That(
                    conv.Import(new Color32(255, 255, 255, 255)),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 74 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 74)));
            }

            {
                var roughnessFactor = 0.0f;
                var conv = new MetallicRoughnessConverter(roughnessFactor);
                Assert.That(
                    conv.Import(new Color32(255, 255, 255, 255)),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 255 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 255)));
            }
        }
    }
}
