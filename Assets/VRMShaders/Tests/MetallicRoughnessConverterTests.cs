using NUnit.Framework;
using UnityEngine;

namespace VRMShaders
{
    public class MetallicRoughnessConverterTests
    {
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
                    OcclusionMetallicRoughnessConverter.ImportPixel(new Color32(255, 255, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 0   : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 0)));
            }

            {
                var roughnessFactor = 1.0f;
                Assert.That(
                    OcclusionMetallicRoughnessConverter.ImportPixel(new Color32(255, 128, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 128 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 127))); // smoothness 0.5 * src.a 1.0
            }

            {
                var roughnessFactor = 0.5f;
                Assert.That(
                    OcclusionMetallicRoughnessConverter.ImportPixel(new Color32(255, 255, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 74 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 127)));
            }

            {
                var roughnessFactor = 0.0f;
                Assert.That(
                    OcclusionMetallicRoughnessConverter.ImportPixel(new Color32(255, 255, 255, 255), 1.0f, roughnessFactor, default),
                    // r <- 255 : Same metallic (src.r)
                    // g <- 0   : (Unused)
                    // b <- 0   : (Unused)
                    // a <- 255 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                    Is.EqualTo(new Color32(255, 0, 0, 255)));
            }
        }
    }
}
