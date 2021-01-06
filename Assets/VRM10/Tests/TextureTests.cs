using NUnit.Framework;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Assert = NUnit.Framework.Assert;

namespace UniVRM10
{
    public class TextureTests
    {
        [Test]
        public void TextureExportTest()
        {
            //// Dummy texture
            //var tex0 = new Texture2D(128, 128)
            //{
            //    wrapMode = TextureWrapMode.Clamp,
            //    filterMode = FilterMode.Trilinear,
            //};
            //var textureManager = new TextureExportManager(new Texture[] {tex0});

            //var material = new Material(Shader.Find("Standard"));
            //material.mainTexture = tex0;

            //var materialExporter = new MaterialExporter();
            //materialExporter.ExportMaterial(material, textureManager);

            //var convTex0 = textureManager.GetExportTexture(0);
            //var sampler = TextureSamplerUtil.Export(convTex0);

            //Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapS);
            //Assert.AreEqual(glWrap.CLAMP_TO_EDGE, sampler.wrapT);
            //Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.minFilter);
            //Assert.AreEqual(glFilter.LINEAR_MIPMAP_LINEAR, sampler.magFilter);
        }
    }

    public class MetallicRoughnessConverterTests
    {
        const float epsilon = 0.005f;
        private static void EqualColor(Color color1, Color color2)
        {
            Assert.AreEqual(color1.r, color2.r, epsilon);
            Assert.AreEqual(color1.g, color2.g, epsilon);
            Assert.AreEqual(color1.b, color2.b, epsilon);
            Assert.AreEqual(color1.a, color2.a, epsilon);
        }

        public static Texture2D CreateMonoTexture(Color color, bool isLinear)
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false, isLinear)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
            };

            Fill(texture, color);
            return texture;
        }

        public static void Fill(Texture2D texture, Color color)
        {
            for (int y = 0; y < texture.height; ++y)
            {
                for (int x = 0; x < texture.width; ++x)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
        }

        public static Color GetColor(Texture2D texture)
        {
            return texture.GetPixel(0, 0);
        }

        [Test]
        public void ExportingColorTest()
        {

            {
                var smoothness = 1.0f;
                var src = CreateMonoTexture(new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f), true);
                var material = UniVRM10.TextureConvertMaterial.GetMetallicRoughnessUnityToGltf(smoothness);
                var dst = UnityTextureUtil.CopyTexture(src, RenderTextureReadWrite.Linear, material);
                // r <- 0   : (Unused)
                // g <- 0   : ((1 - src.a(as float) * smoothness) ^ 2)(as uint8)
                // b <- 255 : Same metallic (src.r)
                // a <- 255 : (Unused)
                EqualColor(GetColor(dst), new Color(0, 0, 1.0f, 1.0f));
            }

            {
                var smoothness = 0.5f;
                var src = CreateMonoTexture(new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f), true);
                var material = UniVRM10.TextureConvertMaterial.GetMetallicRoughnessUnityToGltf(smoothness);
                var dst = UnityTextureUtil.CopyTexture(src, RenderTextureReadWrite.Linear, material);
                // r <- 0   : (Unused)
                // g <- 63  : ((1 - src.a(as float) * smoothness) ^ 2)(as uint8)
                // b <- 255 : Same metallic (src.r)
                // a <- 255 : (Unused)
                EqualColor(GetColor(dst), new Color(0, 0.25f, 1.0f, 1.0f));
            }

            {
                var smoothness = 0.0f;
                var src = CreateMonoTexture(new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f), true);
                var material = UniVRM10.TextureConvertMaterial.GetMetallicRoughnessUnityToGltf(smoothness);
                var dst = UnityTextureUtil.CopyTexture(src, RenderTextureReadWrite.Linear, material);
                // r <- 0   : (Unused)
                // g <- 255 : ((1 - src.a(as float) * smoothness) ^ 2)(as uint8)
                // b <- 255 : Same metallic (src.r)
                // a <- 255 : (Unused)
                EqualColor(GetColor(dst), new Color(0, 1.0f, 1.0f, 1.0f));
            }
        }

        [Test]
        public void ImportingColorTest()
        {
            {
                var roughnessFactor = 1.0f;
                var src = CreateMonoTexture(new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f), true);
                var material = UniVRM10.TextureConvertMaterial.GetMetallicRoughnessGltfToUnity(roughnessFactor);
                var dst = UnityTextureUtil.CopyTexture(src, RenderTextureReadWrite.Linear, material);
                // r <- 255 : Same metallic (src.r)
                // g <- 0   : (Unused)
                // b <- 0   : (Unused)
                // a <- 0   : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                EqualColor(GetColor(dst), new Color(1.0f, 0, 0, 0));
            }

            {
                var roughnessFactor = 1.0f;
                var src = CreateMonoTexture(new UnityEngine.Color(1.0f, 0.25f, 1.0f, 1.0f), true);
                var material = UniVRM10.TextureConvertMaterial.GetMetallicRoughnessGltfToUnity(roughnessFactor);
                var dst = UnityTextureUtil.CopyTexture(src, RenderTextureReadWrite.Linear, material);
                // r <- 255 : Same metallic (src.r)
                // g <- 0   : (Unused)
                // b <- 0   : (Unused)
                // a <- 128 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                EqualColor(GetColor(dst), new Color(1.0f, 0, 0, 0.5f));
            }

            {
                var roughnessFactor = 0.5f;
                var src = CreateMonoTexture(new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f), true);
                var material = UniVRM10.TextureConvertMaterial.GetMetallicRoughnessGltfToUnity(roughnessFactor);
                var dst = UnityTextureUtil.CopyTexture(src, RenderTextureReadWrite.Linear, material);
                // r <- 255 : Same metallic (src.r)
                // g <- 0   : (Unused)
                // b <- 0   : (Unused)
                // a <- 74 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                EqualColor(GetColor(dst), new Color(1.0f, 0, 0, 0.29289f));
            }

            {
                var roughnessFactor = 0.0f;
                var src = CreateMonoTexture(new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f), true);
                var material = UniVRM10.TextureConvertMaterial.GetMetallicRoughnessGltfToUnity(roughnessFactor);
                var dst = UnityTextureUtil.CopyTexture(src, RenderTextureReadWrite.Linear, material);
                // r <- 255 : Same metallic (src.r)
                // g <- 0   : (Unused)
                // b <- 0   : (Unused)
                // a <- 255 : ((1 - sqrt(src.g(as float) * roughnessFactor)))(as uint8)
                EqualColor(GetColor(dst), new Color(1.0f, 0, 0, 1.0f));
            }
        }
    }
}
