using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public sealed class EditorTextureSerializerTests
    {
        private static readonly string AssetPath = "Assets/UniGLTF/Tests/UniGLTF";
        private static readonly string SrgbGrayImageName = "4x4_gray_import_as_srgb";
        private static readonly string LinearGrayImageName = "4x4_gray_import_as_linear";
        private static readonly string NormalMapGrayImageName = "4x4_gray_import_as_normal_map";
        private static readonly Texture2D SrgbGrayTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/{SrgbGrayImageName}.png");
        private static readonly Texture2D LinearGrayTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/{LinearGrayImageName}.png");
        private static readonly Texture2D NormalMapGrayTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{AssetPath}/{NormalMapGrayImageName}.png");
        private static readonly Color32 JustGray = new Color32(127, 127, 127, 255);
        private static readonly Color32 SrgbGrayInSrgb = JustGray;
        private static readonly Color32 SrgbGrayInLinear = ((Color)SrgbGrayInSrgb).linear;
        private static readonly Color32 LinearGrayInLinear = JustGray;
        private static readonly Color32 LinearGrayInSrgb = ((Color)LinearGrayInLinear).gamma;
        private static readonly Color32 NormalizedLinearGrayInLinear = new Color32(127, 127, 255, 255);

        [Test]
        public void InputAssetsRawImage()
        {
            Assert.AreEqual(SrgbGrayInSrgb, GetFirstPixelInTexture2D(SrgbGrayTex));

            // Image color space is sRGB even though Texture color space was Linear.
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

        [Test]
        public void AssignSrgbImageToSrgbTextureProperty()
        {
            var exportedTex = AssignTextureToMaterialPropertyAndExportAndExtract(SrgbGrayTex, SrgbGrayImageName, "_MainTex");
            Assert.AreEqual(SrgbGrayInSrgb, GetFirstPixelInTexture2D(exportedTex));
            UnityEngine.Object.DestroyImmediate(exportedTex);
        }

        [Test]
        public void AssignLinearImageToSrgbTextureProperty()
        {
            var exportedTex = AssignTextureToMaterialPropertyAndExportAndExtract(LinearGrayTex, LinearGrayImageName, "_MainTex");
            Assert.AreEqual(LinearGrayInSrgb, GetFirstPixelInTexture2D(exportedTex));
            UnityEngine.Object.DestroyImmediate(exportedTex);
        }

        [Test]
        public void AssignSrgbImageToLinearTextureProperty()
        {
            var exportedTex = AssignTextureToMaterialPropertyAndExportAndExtract(SrgbGrayTex, SrgbGrayImageName, "_OcclusionMap");
            // R channel is occlusion in glTF spec.
            Assert.AreEqual(SrgbGrayInLinear.r, GetFirstPixelInTexture2D(exportedTex).r);
            UnityEngine.Object.DestroyImmediate(exportedTex);
        }

        [Test]
        public void AssignLinearImageToLinearTextureProperty()
        {
            var exportedTex = AssignTextureToMaterialPropertyAndExportAndExtract(LinearGrayTex, LinearGrayImageName, "_OcclusionMap");
            // R channel is occlusion in glTF spec.
            Assert.AreEqual(LinearGrayInLinear.r, GetFirstPixelInTexture2D(exportedTex).r);
            UnityEngine.Object.DestroyImmediate(exportedTex);
        }

        [Test]
        public void AssignLinearImageToNormalTextureProperty()
        {
            var exportedTex = AssignTextureToMaterialPropertyAndExportAndExtract(LinearGrayTex, LinearGrayImageName, "_BumpMap");
            Assert.AreEqual(NormalizedLinearGrayInLinear, GetFirstPixelInTexture2D(exportedTex));
            // B channel is different from 127. Because it will be normalized as normal vector.
            UnityEngine.Object.DestroyImmediate(exportedTex);
        }

        [Test]
        public void AssignLinearImageAsNormalMapSettingsToNormalTextureProperty()
        {
            var exportedTex = AssignTextureToMaterialPropertyAndExportAndExtract(NormalMapGrayTex, NormalMapGrayImageName, "_BumpMap");
            Assert.AreEqual(NormalizedLinearGrayInLinear, GetFirstPixelInTexture2D(exportedTex));
            // B channel is different from 127. Because it will be normalized as normal vector.
            UnityEngine.Object.DestroyImmediate(exportedTex);
        }

        private static Texture2D AssignTextureToMaterialPropertyAndExportAndExtract(Texture2D srcTex, string srcImageName, string propertyName)
        {
            // Prepare
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mat = new Material(Shader.Find("Standard"));
            mat.SetTexture(propertyName, srcTex);
            root.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Export glTF
            var data = new ExportingGltfData();
            using (var exporter = new gltfExporter(data, new GltfExportSettings
            {
                InverseAxis = Axes.X,
                ExportOnlyBlendShapePosition = false,
                UseSparseAccessorForMorphTarget = false,
                DivideVertexBuffer = false,
            }))
            {
                exporter.Prepare(root);
                exporter.Export(new EditorTextureSerializer());
            }
            var gltf = data.GLTF;
            Assert.AreEqual(1, gltf.images.Count);
            var exportedImage = gltf.images[0];
            Assert.AreEqual("image/png", exportedImage.mimeType);
            Assert.AreEqual(srcImageName, exportedImage.name);

            UnityEngine.Object.DestroyImmediate(mat);
            UnityEngine.Object.DestroyImmediate(root);

            var parsed = GltfData.CreateFromGltfDataForTest(gltf, data.BinBytes);

            // Extract Image to Texture2D
            var exportedBytes = parsed.GetBytesFromBufferView(exportedImage.bufferView).ToArray();
            var exportedTexture = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: false);
            Assert.IsTrue(exportedTexture.LoadImage(exportedBytes)); // Always true ?
            Assert.AreEqual(srcTex.width, exportedTexture.width);
            Assert.AreEqual(srcTex.height, exportedTexture.height);

            return exportedTexture;
        }

        private static Color32 GetFirstPixelInTexture2D(Texture2D tex)
        {
            return tex.GetPixels32()[0];
        }

        private static Color32 GetFirstPixelInRenderTexture(RenderTexture rt)
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
