using System;
using NUnit.Framework;

namespace UniGLTF
{
    public sealed class GlbParserTests
    {
        [Test]
        public void TextureNameUniqueness()
        {
            var gltf = new glTF();
            gltf.asset.version = "2.0";
            gltf.buffers.Add(new glTFBuffer(new ArrayByteBuffer(Array.Empty<byte>())));
            gltf.textures.Add(new glTFTexture
            {
                name = "FooBar",
                source = 0,
            });
            gltf.textures.Add(new glTFTexture
            {
                name = "foobar",
                source = 1,
            });
            gltf.images.Add(new glTFImage
            {
                name = "HogeFuga",
            });
            gltf.images.Add(new glTFImage
            {
                name = "hogefuga",
            });

            var parser = new GlbLowLevelParser("Test", new GltfBufferWriter(gltf).ToGlbBytes());
            var data = parser.Parse();

            Assert.AreEqual("FooBar", data.GLTF.textures[0].name);
            // NOTE: 大文字小文字が違うだけの名前は、同一としてみなされ、Suffix が付く。
            Assert.AreEqual("foobar__UNIGLTF__DUPLICATED__2", data.GLTF.textures[1].name);
        }
    }
}