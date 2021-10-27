using System;
using NUnit.Framework;

namespace UniGLTF
{
    public sealed class GlbParserTests
    {
        [Test]
        public void TextureNameUniqueness()
        {
            var data = new ExportingGltfData();
            var gltf = data.GLTF;
            gltf.asset.version = "2.0";
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

            var parser = new GlbLowLevelParser("Test", data.ToGlbBytes());
            var parsed = parser.Parse();

            Assert.AreEqual("FooBar", parsed.GLTF.textures[0].name);
            // NOTE: 大文字小文字が違うだけの名前は、同一としてみなされ、Suffix が付く。
            Assert.AreEqual("foobar__UNIGLTF__DUPLICATED__2", parsed.GLTF.textures[1].name);
        }
    }
}