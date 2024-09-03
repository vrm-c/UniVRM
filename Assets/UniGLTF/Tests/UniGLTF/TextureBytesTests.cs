using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public class TextureBytesTests
    {
        [Test]
        public void NonReadablePng()
        {
            var nonReadableTex = TestAssets.LoadAsset<Texture2D>("4x4_non_readable.png");
            Assert.False(nonReadableTex.isReadable);
            var (bytes, mime) = new EditorTextureSerializer().ExportBytesWithMime(nonReadableTex, ColorSpace.sRGB);
            Assert.NotNull(bytes);
        }

        [Test]
        public void NonReadableDds()
        {
            var readonlyTexture = TestAssets.LoadAsset<Texture2D>("4x4_non_readable_compressed.dds");
            Assert.False(readonlyTexture.isReadable);
            var (bytes, mime) = new EditorTextureSerializer().ExportBytesWithMime(readonlyTexture, ColorSpace.sRGB);
            Assert.NotNull(bytes);
        }
    }
}
