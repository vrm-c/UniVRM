using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public class TextureTests
    {
        [Test]
        public void TextureExportTest()
        {
            // Dummy texture
            var tex0 = new Texture2D(128, 128)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
            };
            var textureExporter = new TextureExporter(new EditorTextureSerializer());

            var material = new Material(Shader.Find("Standard"));
            material.mainTexture = tex0;

            var materialExporter = new MaterialExporter();
            materialExporter.ExportMaterial(material, textureExporter, new GltfExportSettings());

            var exported = textureExporter.Export();

            var (convTex0, colorSpace) = exported[0];
            var sampler = TextureSamplerUtil.Export(convTex0);

            Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapS);
            Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapT);

            Assert.AreEqual(FilterMode.Trilinear, convTex0.filterMode);
            Assert.IsTrue(convTex0.mipmapCount > 1);
            // Tirilinear => LINEAR_MIPMAP_LINEAR
            Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.minFilter);
            Assert.AreEqual(glFilter.LINEAR, sampler.magFilter);
        }

        static FileInfo Find(DirectoryInfo current, string target)
        {
            foreach (var child in current.EnumerateFiles())
            {
                if (child.Name == target)
                {
                    return child;
                }
            }

            foreach (var child in current.EnumerateDirectories())
            {
                var found = Find(child, target);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        static FileInfo GetGltfTestModelPath(string name)
        {
            var env = System.Environment.GetEnvironmentVariable("GLTF_SAMPLE_MODELS");
            if (string.IsNullOrEmpty(env))
            {
                return null;
            }
            var root = new DirectoryInfo($"{env}/2.0");
            if (!root.Exists)
            {
                return null;
            }

            return Find(root, name);
        }

        [Test]
        public void TextureExtractTest()
        {
            var path = GetGltfTestModelPath("BoomBox.glb");
            if (path == null)
            {
                return;
            }

            using (var data = new GlbFileParser(path.FullName).Parse())
            using (var context = new ImporterContext(data))
            {
                var instance = context.Load();
                var textureMap = instance.RuntimeResources
                    .Select(kv => (kv.Item1, kv.Item2 as Texture))
                    .Where(kv => kv.Item2 != null)
                    .ToDictionary(kv => kv.Item1, kv => kv.Item2)
                    ;

                // extractor
                var extractor = new TextureExtractor(data, UnityPath.FromUnityPath(""), textureMap);
                var m = context.TextureDescriptorGenerator.Get().GetEnumerable()
                    .FirstOrDefault(x => x.SubAssetKey.Name == "texture_1.standard");

                Assert.Catch<NotImplementedException>(() => extractor.Extract(m.SubAssetKey, m));
            }
        }
    }
}
