using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace VRMShaders
{
    public class MetallicRoughnessConverterTests
    {
        private Color32 ImportPixel(Color32 metallicRoughnessPixel, float metallicFactor, float roughnessFactor, Color32 occlusionPixel)
        {
            var metallicRoughnessTexture = new Texture2D(4, 4, TextureFormat.ARGB32, mipChain: false, linear: true);
            metallicRoughnessTexture.SetPixels32(Enumerable.Range(0, 16).Select(_ => metallicRoughnessPixel).ToArray());
            metallicRoughnessTexture.Apply();

            var occlusionTexture = new Texture2D(4, 4, TextureFormat.ARGB32, mipChain: false, linear: true);
            occlusionTexture.SetPixels32(Enumerable.Range(0, 16).Select(_ => occlusionPixel).ToArray());
            occlusionTexture.Apply();

            var converted = OcclusionMetallicRoughnessConverter.Import(
                metallicRoughnessTexture,
                metallicFactor,
                roughnessFactor,
                occlusionTexture
            );

            var result = converted.GetPixels32()[0];

            UnityEngine.Object.DestroyImmediate(metallicRoughnessTexture);
            UnityEngine.Object.DestroyImmediate(occlusionTexture);
            UnityEngine.Object.DestroyImmediate(converted);

            return result;
        }

        [Test]
        public void ExportingColorTest()
        {
            {
                var smoothness = 1.0f;
                Assert.That(
                    OcclusionMetallicRoughnessConverter.ExportPixel(new Color32(255, 255, 255, 255), smoothness, default),
                    // r <- 0   : (Unused)
                    // g <- 0   : ((1 - src.a(as float) * smoothness) ^ 2)(as uint8)
                    // b <- 255 : Same metallic (src.r)
                    // a <- 255 : (Unused)
                    Is.EqualTo(new Color32(0, 0, 255, 255)));
            }

            {
                var smoothness = 0.5f;
                Assert.That(
                    OcclusionMetallicRoughnessConverter.ExportPixel(new Color32(255, 255, 255, 255), smoothness, default),
                    // r <- 0   : (Unused)
                    // g <- 63  : ((1 - src.a(as float) * smoothness) ^ 2)(as uint8)
                    // b <- 255 : Same metallic (src.r)
                    // a <- 255 : (Unused)
                    Is.EqualTo(new Color32(0, 127, 255, 255)));
            }

            {
                var smoothness = 0.0f;
                Assert.That(
                    OcclusionMetallicRoughnessConverter.ExportPixel(new Color32(255, 255, 255, 255), smoothness, default),
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
                Assert.That(
                    ImportPixel(new Color32(255, 255, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 0   : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 0)));
            }

            {
                var roughnessFactor = 1.0f;
                Assert.That(
                    ImportPixel(new Color32(255, 128, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 128 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 127))); // smoothness 0.5 * src.a 1.0
            }

            {
                var roughnessFactor = 0.5f;
                Assert.That(
                    ImportPixel(new Color32(255, 255, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 74 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 127)));
            }

            {
                var roughnessFactor = 0.0f;
                Assert.That(
                    ImportPixel(new Color32(255, 255, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 255 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 255)));
            }
        }

        [Test]
        public void ExportMetallicSmoothnessOcclusion_Test()
        {
            var textureSerializer = new EditorTextureSerializer();
            var metallic = new Texture2D(4, 4, TextureFormat.ARGB32, false, true);
            var occlusion = new Texture2D(4, 4, TextureFormat.ARGB32, false, true);

            {
                var exporter = new TextureExporter(new EditorTextureSerializer());
                Assert.AreEqual(-1, exporter.ExportAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(null, 0, null));
            }
            {
                var exporter = new TextureExporter(new EditorTextureSerializer());
                Assert.AreEqual(0, exporter.ExportAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(null, 0, occlusion));
                Assert.AreEqual(1, exporter.ExportAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(metallic, 0, null));
            }
            {
                var exporter = new TextureExporter(new EditorTextureSerializer());
                Assert.AreEqual(0, exporter.ExportAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(metallic, 0, occlusion));
                Assert.AreEqual(0, exporter.ExportAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(null, 0, occlusion));
                Assert.AreEqual(0, exporter.ExportAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(metallic, 0, null));
            }
        }
    }
}
