using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VRMShaders
{
    public class TextureBytesTests
    {
        static string AssetPath = "Assets/VRMShaders/GLTF/IO/Tests";

        [Test]
        public void NotReadable()
        {
            var readonlyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/4x4.png");
            Assert.False(readonlyTexture.isReadable);
            var (bytes, mime) = AssetTextureUtil.GetTextureBytesWithMime(readonlyTexture);
            Assert.NotNull(bytes);
        }

        [Test]
        public void Compressed()
        {
            var readonlyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/4x4compressed.dds");
            Assert.False(readonlyTexture.isReadable);
            var (bytes, mime) = AssetTextureUtil.GetTextureBytesWithMime(readonlyTexture);
            Assert.NotNull(bytes);
        }
    }
}
