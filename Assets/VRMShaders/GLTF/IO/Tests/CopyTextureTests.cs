using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ColorSpace = UniGLTF.ColorSpace;

namespace VRMShaders
{
    public sealed class CopyTextureTests
    {
        private static string AssetPath = "Assets/VRMShaders/GLTF/IO/Tests";

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
            var nonReadableTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/4x4_non_readable.png");
            Assert.False(nonReadableTex.isReadable);
            var copiedTex = TextureConverter.CopyTexture(nonReadableTex, ColorSpace.sRGB, null);
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
            var compressedTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/4x4_non_readable_compressed.dds");
            Assert.False(compressedTex.isReadable);
            var copiedTex = TextureConverter.CopyTexture(compressedTex, ColorSpace.sRGB, null);
            var pixels = copiedTex.GetPixels32(miplevel: 0);
            Assert.AreEqual(pixels.Length, DdsTextureValues.Length);
            for (var idx = 0; idx < pixels.Length; ++idx)
            {
                Assert.AreEqual(DdsTextureValues[idx], pixels[idx]);
            }
        }
    }
}
