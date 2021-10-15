using System;
using NUnit.Framework;

namespace UniGLTF
{
    public sealed class GlbParserTests
    {
        [Test]
        public void TextureNameUniqueness()
        {
            var gltfData = new glTF();
            gltfData.asset.version = "2.0";
            gltfData.buffers.Add(new glTFBuffer(new ArrayByteBuffer(Array.Empty<byte>())));
            gltfData.textures.Add(new glTFTexture
            {
                name = "FooBar",
                source = 0,
            });
            gltfData.textures.Add(new glTFTexture
            {
                name = "foobar",
                source = 1,
            });
            gltfData.images.Add(new glTFImage
            {
                name = "HogeFuga",
            });
            gltfData.images.Add(new glTFImage
            {
                name = "hogefuga",
            });

            var parser = new GlbLowLevelParser("Test", gltfData.ToGlbBytes());
            var data = parser.Parse();

            Assert.AreEqual("FooBar", data.GLTF.textures[0].name);
            // NOTE: 大文字小文字が違うだけの名前は、同一としてみなされ、Suffix が付く。
            Assert.AreEqual("foobar__UNIGLTF__DUPLICATED__2", data.GLTF.textures[1].name);
        }
    }
}