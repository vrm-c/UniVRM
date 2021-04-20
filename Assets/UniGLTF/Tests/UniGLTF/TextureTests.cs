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
            var textureManager = new TextureExporter(AssetTextureUtil.IsTextureEditorAsset);

            var material = new Material(Shader.Find("Standard"));
            material.mainTexture = tex0;

            var materialExporter = new MaterialExporter();
            materialExporter.ExportMaterial(material, textureManager);

            var convTex0 = textureManager.Exported[0];
            var sampler = TextureSamplerUtil.Export(convTex0);

            Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapS);
            Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapT);
            Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.minFilter);
            Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.magFilter);
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

            // parse
            var parser = new GltfParser();
            parser.ParsePath(path.FullName);

            // load
            var loader = new ImporterContext(parser);
            loader.Load();

            // extractor
            var extractor = new TextureExtractor(parser, UnityPath.FromUnityPath(""), loader.TextureFactory.Textures.Select(x => (new SubAssetKey(typeof(Texture2D), x.Texture.name), x.Texture)).ToArray());
            var m = GltfTextureEnumerator.EnumerateTexturesForMaterial(parser, 0).FirstOrDefault(x => x.Item1.Name == "texture_1.standard");

            Assert.Catch<NotImplementedException>(() => extractor.Extract(m.Item1, m.Item2));
        }
    }
}
