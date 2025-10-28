using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public sealed class CopyTextureTests
    {
        private static readonly Color32 Black = new Color32(0, 0, 0, 255);
        private static readonly Color32 Gray = new Color32(127, 127, 127, 255);
        private static readonly Color32 White = new Color32(255, 255, 255, 255);

        private static readonly Color32[] PngTextureValues = new Color32[]
        {
            White, White, White, White,
            Gray, Gray, Gray, Gray,
            Gray, Gray, Gray, Gray,
            Black, Black, Black, Black,
        };

        // DDSは圧縮で微妙に色が変わるので単色にした
        private static readonly Color32[] DdsTextureValues = new Color32[]
        {
            Gray, Gray, Gray, Gray,
            Gray, Gray, Gray, Gray,
            Gray, Gray, Gray, Gray,
            Gray, Gray, Gray, Gray,
        };

        [Test]
        public void CopyFromNonReadableSRgbPng()
        {
            var nonReadableTex = TestAssets.LoadAsset<Texture2D>("4x4_non_readable.png");
            Assert.False(nonReadableTex.isReadable);
            var copiedTex = TextureConverter.CopyTexture(nonReadableTex, ColorSpace.sRGB, true, null);
            var pixels = copiedTex.GetPixels32(miplevel: 0);
            Assert.AreEqual(pixels.Length, PngTextureValues.Length);
            for (var idx = 0; idx < pixels.Length; ++idx)
            {
                Assert.AreEqual(PngTextureValues[idx], pixels[idx]);
            }
        }

        [Test]
        public void CopyFromNonReadableSRgbDds()
        {
            var compressedTex = TestAssets.LoadAsset<Texture2D>("4x4_non_readable_compressed.dds");
            Assert.False(compressedTex.isReadable);
            var copiedTex = TextureConverter.CopyTexture(compressedTex, ColorSpace.sRGB, true, null);
            var pixels = copiedTex.GetPixels32(miplevel: 0);
            Assert.AreEqual(pixels.Length, DdsTextureValues.Length);
            for (var idx = 0; idx < pixels.Length; ++idx)
            {
                Assert.AreEqual(DdsTextureValues[idx], pixels[idx]);
            }
        }

        [Test]
        public void CopyAttributes()
        {
            var src = TestAssets.LoadAsset<Texture2D>("4x4_non_readable.png");
            var dst = TextureConverter.CopyTexture(src, ColorSpace.sRGB, false, null);
            Assert.AreEqual(src.name, dst.name);
            Assert.AreEqual(src.anisoLevel, dst.anisoLevel);
            Assert.AreEqual(src.filterMode, dst.filterMode);
            Assert.AreEqual(src.mipMapBias, dst.mipMapBias);
            Assert.AreEqual(src.wrapMode, dst.wrapMode);
            Assert.AreEqual(src.wrapModeU, dst.wrapModeU);
            Assert.AreEqual(src.wrapModeV, dst.wrapModeV);
            Assert.AreEqual(src.wrapModeW, dst.wrapModeW);
            Assert.AreEqual(src.mipmapCount, dst.mipmapCount);
            Assert.AreEqual(src.width, dst.width);
            Assert.AreEqual(src.height, dst.height);
            Assert.AreEqual(src.format, dst.format);
            Assert.AreEqual(src.imageContentsHash, dst.imageContentsHash);
        }
    }
}
