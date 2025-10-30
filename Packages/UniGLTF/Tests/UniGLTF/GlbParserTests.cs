using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace UniGLTF
{
    public sealed class GlbParserTests
    {
        [Test]
        public void TextureNameUniqueness()
        {
            var data = new ExportingGltfData();
            var gltf = data.Gltf;
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
            using (var parsed = parser.Parse())
            {
                Assert.AreEqual("FooBar", parsed.GLTF.textures[0].name);
                // NOTE: 大文字小文字が違うだけの名前は、同一としてみなされ、Suffix が付く。
                Assert.AreEqual("foobar__UNIGLTF__DUPLICATED__2", parsed.GLTF.textures[1].name);
            }
        }

        /// <summary>
        /// ヘッダが正しいが、後ろが切れている場合に throw する
        /// </summary>
        [Test]
        public void GlbLengthTest()
        {
            var env = System.Environment.GetEnvironmentVariable("GLTF_SAMPLE_MODELS");
            if (string.IsNullOrEmpty(env))
            {
                return;
            }
            var root = new DirectoryInfo($"{env}/2.0");
            if (!root.Exists)
            {
                return;
            }

            var path = Path.Combine(root.ToString(), "DamagedHelmet\\glTF-Binary\\DamagedHelmet.glb");
            Assert.True(File.Exists(path));

            var bytes = File.ReadAllBytes(path);
            using (var data = new GlbBinaryParser(bytes, Path.GetFileNameWithoutExtension(path)).Parse())
            {

                // glb header + 1st chunk only
                var mod = bytes.Take(12 + 8 + data.Chunks[0].Bytes.Count).ToArray();

                Assert.Throws<GlbParseException>(() =>
                {
                // 再パース
                var data2 = new GlbBinaryParser(mod, Path.GetFileNameWithoutExtension(path)).Parse();
                });
            }
        }
    }
}
