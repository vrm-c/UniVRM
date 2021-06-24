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
                occlusionTexture,
                false
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
                    // r <- 255 : metallic (metallicRoughness.b * metallicFactor)
                    // g <- 0 : occlusion (occlusion.r)
                    // b <- 0   : (Unused)
                    // a <- 0 : smoothness (1.0 - (metallicRoughness.g * roughnessFactor))
                    Is.EqualTo(new Color32(255, 0, 0, 0)));
            }

            {
                var roughnessFactor = 1.0f;
                Assert.That(
                    ImportPixel(new Color32(255, 127, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : metallic (metallicRoughness.b * metallicFactor)
                    // g <- 0 : occlusion (occlusion.r)
                    // b <- 0   : (Unused)
                    // a <- 128 : smoothness (1.0 - (metallicRoughness.g * roughnessFactor))
                    Is.EqualTo(new Color32(255, 0, 0, 128))); // A:smoothness = 1.0 - (0.5 * 1.0) = 0.5
            }

            {
                var roughnessFactor = 0.5f;
                Assert.That(
                    ImportPixel(new Color32(255, 127, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : metallic (metallicRoughness.b * metallicFactor)
                    // g <- 0 : occlusion (occlusion.r)
                    // b <- 0   : (Unused)
                    // a <- 191 : smoothness (1.0 - (metallicRoughness.g * roughnessFactor))
                    Is.EqualTo(new Color32(255, 0, 0, 191))); // A:smoothness = 1.0 - (0.5 * 0.5) = 0.75
            }

            {
                var roughnessFactor = 0.0f;
                Assert.That(
                    ImportPixel(new Color32(255, 255, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : metallic (metallicRoughness.b * metallicFactor)
                    // g <- 0 : occlusion (occlusion.r)
                    // b <- 0   : (Unused)
                    // a <- 255 : smoothness (1.0 - (metallicRoughness.g * roughnessFactor))
                    Is.EqualTo(new Color32(255, 0, 0, 255)));
            }

            {
                Assert.That(
                    ImportPixel(new Color32(222, 200, 100, 255), 0.5f, 0.25f, new Color32(127, 0, 0, 0)),
                    // r <- 50 : metallic (metallicRoughness.b * metallicFactor)
                    // g <- 127 : occlusion (occlusion.r)
                    // b <- 0   : (Unused)
                    // a <- 205 : smoothness (1.0 - (metallicRoughness.g * roughnessFactor))
                    Is.EqualTo(new Color32(50, 127, 0, 205)));
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
                Assert.AreEqual(-1, exporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(null, 0, null));
            }
            {
                var exporter = new TextureExporter(new EditorTextureSerializer());
                Assert.AreEqual(0, exporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(null, 0, occlusion));
                Assert.AreEqual(1, exporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(metallic, 0, null));
            }
            {
                var exporter = new TextureExporter(new EditorTextureSerializer());
                Assert.AreEqual(0, exporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(metallic, 0, occlusion));
                Assert.AreEqual(0, exporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(null, 0, occlusion));
                Assert.AreEqual(0, exporter.RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(metallic, 0, null));
            }
        }
    }
}
