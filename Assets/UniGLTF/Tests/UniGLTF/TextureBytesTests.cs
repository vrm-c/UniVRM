using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VRMShaders
{
    public class TextureBytesTests
    {
        static string AssetPath = "Assets/VRMShaders/GLTF/IO/Tests";

        [Test]
        public void NonReadablePng()
        {
            var nonReadableTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/4x4_non_readable.png");
            Assert.False(nonReadableTex.isReadable);
            var (bytes, mime) = new EditorTextureSerializer().ExportBytesWithMime(nonReadableTex, ColorSpace.sRGB);
            Assert.NotNull(bytes);
        }

        [Test]
        public void NonReadableDds()
        {
            var readonlyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/4x4_non_readable_compressed.dds");
            Assert.False(readonlyTexture.isReadable);
            var (bytes, mime) = new EditorTextureSerializer().ExportBytesWithMime(readonlyTexture, ColorSpace.sRGB);
            Assert.NotNull(bytes);
        }
    }
}
