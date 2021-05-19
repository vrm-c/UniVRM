using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VRMShaders
{
    public sealed class TextureColorSpaceSpecificationTests
    {
        private static readonly string AssetPath = "Assets/VRMShaders/GLTF/IO/Tests";
        private static readonly Texture2D SrgbGrayTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/4x4_gray_import_as_srgb.png");
        private static readonly Texture2D LinearGrayTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/4x4_gray_import_as_linear.png");
        private static readonly Color32 JustGray = new Color32(127, 127, 127, 255);
        private static readonly Color32 SrgbGrayInSrgb = JustGray;
        private static readonly Color32 SrgbGrayInLinear = ((Color) SrgbGrayInSrgb).linear;
        private static readonly Color32 LinearGrayInLinear = JustGray;
        private static readonly Color32 LinearGrayInSrgb = ((Color) LinearGrayInLinear).gamma;

        [Test]
        public void InputAssetsRawImage()
        {
            Assert.AreEqual(SrgbGrayInSrgb, GetFirstPixelInTexture2D(SrgbGrayTex));

            // Image color space is sRGB even through Texture color space was Linear.
            Assert.AreEqual(SrgbGrayInSrgb, GetFirstPixelInTexture2D(LinearGrayTex));
        }

        [Test]
        public void CopyToSrgbRenderTexture()
        {
            var srgbRt = RenderTexture.GetTemporary(4, 4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

            Graphics.Blit(SrgbGrayTex, srgbRt);
            Assert.AreEqual(SrgbGrayInSrgb, GetFirstPixelInRenderTexture(srgbRt));

            Graphics.Blit(LinearGrayTex, srgbRt);
            Assert.AreEqual(LinearGrayInSrgb, GetFirstPixelInRenderTexture(srgbRt));

            RenderTexture.ReleaseTemporary(srgbRt);
        }

        [Test]
        public void CopyToLinearRenderTexture()
        {
            var linearRt = RenderTexture.GetTemporary(4, 4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            Graphics.Blit(SrgbGrayTex, linearRt);
            Assert.AreEqual(SrgbGrayInLinear, GetFirstPixelInRenderTexture(linearRt));

            Graphics.Blit(LinearGrayTex, linearRt);
            Assert.AreEqual(LinearGrayInLinear, GetFirstPixelInRenderTexture(linearRt));

            RenderTexture.ReleaseTemporary(linearRt);
        }

        private Color32 GetFirstPixelInTexture2D(Texture2D tex)
        {
            return tex.GetPixels32()[0];
        }

        private Color32 GetFirstPixelInRenderTexture(RenderTexture rt)
        {
            // This function copies memory values and just read, so Texture2D's color space is not important.
            var srgbTex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, mipChain: false, linear: false);
            RenderTexture.active = rt;
            srgbTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            srgbTex.Apply();
            RenderTexture.active = null;
            var pixel = GetFirstPixelInTexture2D(srgbTex);
            UnityEngine.Object.DestroyImmediate(srgbTex);
            return pixel;
        }
    }
}
