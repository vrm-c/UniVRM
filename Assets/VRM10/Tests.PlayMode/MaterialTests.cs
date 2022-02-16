using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UnityEngine;
using UnityEngine.TestTools;
using UniVRM10;
using VrmLib;

namespace UniVRM10.Test
{
    public class MaterialTests
    {
        const string _vrmPath = "Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm";

        private (GameObject, IReadOnlyList<VRMShaders.MaterialFactory.MaterialLoadInfo>) ToUnity(string path)
        {
            var fi = new FileInfo(_vrmPath);
            var bytes = File.ReadAllBytes(fi.FullName);

            // migrate to 1.0
            bytes = MigrationVrm.Migrate(bytes);

            return ToUnity(bytes);
        }

        private (GameObject, IReadOnlyList<VRMShaders.MaterialFactory.MaterialLoadInfo>) ToUnity(byte[] bytes)
        {
            // Vrm => Model
            using (var data = new GlbBinaryParser(bytes, "tmp.vrm").Parse())
            using (var migrated = Vrm10Data.Migrate(data, out Vrm10Data result, out MigrationData migration))
            {
                if (result == null)
                {
                    throw new Exception();
                }
                return ToUnity(result);
            }
        }

        private (GameObject, IReadOnlyList<VRMShaders.MaterialFactory.MaterialLoadInfo>) ToUnity(Vrm10Data data)
        {
            // Model => Unity
            using (var loader = new Vrm10Importer(data))
            {
                var loaded = loader.Load();
                return (loaded.gameObject, loader.MaterialFactory.Materials);
            }
        }

        private Model ToVrmModel(GameObject root)
        {
            using (var arrayManager = new NativeArrayManager())
            {
                var exporter = new UniVRM10.ModelExporter();
                var model = exporter.Export(arrayManager, root);

                model.ConvertCoordinate(VrmLib.Coordinates.Vrm1, ignoreVrm: false);
                return model;
            }
        }

        void EqualColor(Color color1, Color color2)
        {
            Assert.AreEqual(color1.r, color2.r, 0.001f);
            Assert.AreEqual(color1.g, color2.g, 0.001f);
            Assert.AreEqual(color1.b, color2.b, 0.001f);
            Assert.AreEqual(color1.a, color2.a, 0.001f);
        }

        void EqualVector4(Vector4 vec1, Vector4 vec2)
        {
            Assert.AreEqual(vec1.x, vec2.x, 0.001f);
            Assert.AreEqual(vec1.y, vec2.y, 0.001f);
            Assert.AreEqual(vec1.z, vec2.z, 0.001f);
            Assert.AreEqual(vec1.w, vec2.w, 0.001f);
        }

        #region Color

        [UnityTest]
        public IEnumerator ColorSpace_UnityBaseColorToLiner()
        {
            var (root, materials) = ToUnity(_vrmPath);
            var srcMaterial = materials.First();
            var srcColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            var srcGammaColor = srcColor;
            var srclinerColor = srcColor.linear;

            srcMaterial.Asset.color = srcColor;

            var model = ToVrmModel(root);
            var dstMaterial = model.Materials.First(x => x is UnityEngine.Material m && m.name == srcMaterial.Asset.name) as UnityEngine.Material;

            EqualColor(srclinerColor, dstMaterial.color);

            yield return null;
        }

        // [UnityTest]
        // public IEnumerator ColorSpace_GltfBaseColorToGamma()
        // {
        //     var assets = ToUnity(_vrmPath);
        //     var srcMaterial = assets.Map.Materials.First();
        //     var key = srcMaterial.Key;
        //     var srcColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        //     var srclinerColor = srcColor.ToVector4();
        //     var srcGammaColor = srcColor.gamma.ToVector4();


        //     var model = ToVrmModel(assets.Root);
        //     var gltfMaterial = model.Materials.First(x => x.Name == key.Name);
        //     gltfMaterial.BaseColorFactor = new LinearColor
        //     {
        //         RGBA = srclinerColor
        //     };

        //     var bytes = model.ToGlb();

        //     var dstAssets = ToUnity(bytes);
        //     var dstMaterial = dstAssets.Map.Materials.First(x => x.Value.name == key.Name);

        //     EqualColor(srcGammaColor.ToUnityColor(), dstMaterial.Value.color);

        //     yield return null;
        // }

        // [UnityTest]
        // public IEnumerator MToonUnityColorToGltf()
        // {
        //     var assets = ToUnity(_vrmPath);
        //     var srcMaterial = assets.Map.Materials.First();
        //     var key = srcMaterial.Key;
        //     var srcColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        //     var srcGammaColor = srcColor;
        //     var srclinerColor = srcColor.linear;

        //     srcMaterial.Value.SetColor(VrmLib.MToon.Utils.PropColor, srcColor);
        //     srcMaterial.Value.SetColor(VrmLib.MToon.Utils.PropShadeColor, srcColor);
        //     srcMaterial.Value.SetColor(VrmLib.MToon.Utils.PropEmissionColor, srcColor);
        //     srcMaterial.Value.SetColor(VrmLib.MToon.Utils.PropRimColor, srcColor);
        //     srcMaterial.Value.SetColor(VrmLib.MToon.Utils.PropOutlineColor, srcColor);

        //     var model = ToVrmModel(assets.Root);
        //     var dstMaterial = model.Materials.First(x => x.Name == key.Name) as VrmLib.MToonMaterial;

        //     // sRGB
        //     EqualColor(srclinerColor, dstMaterial.Definition.Color.LitColor.RGBA.ToUnityColor());
        //     EqualColor(srclinerColor, dstMaterial.Definition.Color.ShadeColor.RGBA.ToUnityColor());
        //     EqualColor(srclinerColor, dstMaterial.Definition.Outline.OutlineColor.RGBA.ToUnityColor());
        //     // HDR Color
        //     EqualColor(srcColor, dstMaterial.Definition.Emission.EmissionColor.RGBA.ToUnityColor());
        //     EqualColor(srcColor, dstMaterial.Definition.Rim.RimColor.RGBA.ToUnityColor());

        //     yield return null;
        // }

        // [UnityTest]
        // public IEnumerator MtoonGltfColorToUnity()
        // {
        //     var assets = ToUnity(_vrmPath);
        //     var srcMaterial = assets.Map.Materials.First();
        //     var key = srcMaterial.Key;
        //     var srcColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        //     var srclinerColor = srcColor.ToVector4();
        //     var srcGammaColor = srcColor.gamma.ToVector4();

        //     var model = ToVrmModel(assets.Root);
        //     var gltfMaterial = model.Materials.First(x => x.Name == key.Name) as VrmLib.MToonMaterial;
        //     if (gltfMaterial == null)
        //     {
        //         throw new NotImplementedException();
        //     }

        //     gltfMaterial.Definition = new VrmLib.MToon.MToonDefinition
        //     {
        //         Color = new VrmLib.MToon.ColorDefinition
        //         {
        //             LitColor = new LinearColor { RGBA = srclinerColor },
        //             ShadeColor = new LinearColor { RGBA = srclinerColor },
        //         },
        //         Outline = new VrmLib.MToon.OutlineDefinition
        //         {
        //             OutlineColor = new LinearColor { RGBA = srclinerColor },
        //         },
        //         Emission = new VrmLib.MToon.EmissionDefinition
        //         {
        //             EmissionColor = new LinearColor { RGBA = srclinerColor },
        //         },
        //         Rim = new VrmLib.MToon.RimDefinition
        //         {
        //             RimColor = new LinearColor { RGBA = srclinerColor },
        //         }
        //     };

        //     var bytes = model.ToGlb();

        //     var dstAssets = ToUnity(bytes);
        //     var dstMaterial = dstAssets.Map.Materials.First(x => x.Value.name == key.Name).Value;
        //     // sRGB
        //     EqualColor(srcGammaColor.ToUnityColor(), dstMaterial.GetColor(VrmLib.MToon.Utils.PropColor));
        //     EqualColor(srcGammaColor.ToUnityColor(), dstMaterial.GetColor(VrmLib.MToon.Utils.PropShadeColor));
        //     EqualColor(srcGammaColor.ToUnityColor(), dstMaterial.GetColor(VrmLib.MToon.Utils.PropOutlineColor));
        //     // HDR Color
        //     EqualColor(srcColor, dstMaterial.GetColor(VrmLib.MToon.Utils.PropEmissionColor));
        //     EqualColor(srcColor, dstMaterial.GetColor(VrmLib.MToon.Utils.PropRimColor));

        //     yield return null;
        // }
        // #endregion

        // #region Texture

        // Texture2D CreateMonoTexture(float mono, float alpha, bool isLinear)
        // {
        //     Texture2D texture = new Texture2D(128, 128, TextureFormat.ARGB32, mipChain: false, linear: isLinear);
        //     Color col = new Color(mono, mono, mono, alpha);
        //     for (int y = 0; y < texture.height; y++)
        //     {
        //         for (int x = 0; x < texture.width; x++)
        //         {
        //             texture.SetPixel(x, y, col);
        //         }
        //     }
        //     texture.Apply();
        //     return texture;
        // }

        // void EqualTextureColor(Texture2D texture, VrmLib.ImageTexture imageTexture, bool isLinear)
        // {
        //     var srcColor = texture.GetPixel(0, 0);

        //     var dstTexture = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: isLinear);
        //     dstTexture.LoadImage(imageTexture.Image.Bytes.ToArray());
        //     var dstColor = dstTexture.GetPixel(0, 0);

        //     Debug.LogFormat("src:{0}, dst{1}", srcColor, dstColor);
        //     EqualColor(srcColor, dstColor);
        // }

        // [UnityTest]
        // public IEnumerator MToonTextureToGltf_BaseColorTexture()
        // {
        //     var assets = ToUnity(_vrmPath);
        //     var srcMaterial = assets.Map.Materials.First();
        //     var key = srcMaterial.Key;
        //     var srcSrgbTexture = CreateMonoTexture(0.5f, 0.5f, false);

        //     srcMaterial.Value.SetTexture(VrmLib.MToon.Utils.PropMainTex, srcSrgbTexture);

        //     var model = ToVrmModel(assets.Root);
        //     var dstMaterial = model.Materials.First(x => x.Name == key.Name) as VrmLib.MToonMaterial;

        //     var imageTexture = dstMaterial.Definition.Color.LitMultiplyTexture.Texture as VrmLib.ImageTexture;
        //     EqualTextureColor(srcSrgbTexture, imageTexture, false);

        //     yield return null;
        // }

        // [UnityTest]
        // public IEnumerator MToonTextureToGltf_OutlineWidthTexture()
        // {
        //     var assets = ToUnity(_vrmPath);
        //     var srcMaterial = assets.Map.Materials.First();
        //     var key = srcMaterial.Key;
        //     var srcLinearTexture = CreateMonoTexture(0.5f, 0.5f, true);

        //     srcMaterial.Value.SetTexture(VrmLib.MToon.Utils.PropOutlineWidthTexture, srcLinearTexture);

        //     var model = ToVrmModel(assets.Root);
        //     var dstMaterial = model.Materials.First(x => x.Name == key.Name) as VrmLib.MToonMaterial;

        //     var imageTexture = dstMaterial.Definition.Outline.OutlineWidthMultiplyTexture.Texture as VrmLib.ImageTexture;
        //     EqualTextureColor(srcLinearTexture, imageTexture, true);

        //     yield return null;
        // }

        // [UnityTest]
        // public IEnumerator GetOrCreateTextureTest()
        // {
        //     var converter = new RuntimeVrmConverter();
        //     var material = new UnityEngine.Material(Shader.Find("VRM/MToon"));
        //     var srcLinearTexture = CreateMonoTexture(0.5f, 1.0f, true);
        //     var srcSRGBTexture = CreateMonoTexture(0.5f, 1.0f, false);

        //     {
        //         material.SetTexture(VrmLib.MToon.Utils.PropOutlineWidthTexture, srcSRGBTexture);
        //         var textureInfo = converter.GetOrCreateTexture(material, srcSRGBTexture, VrmLib.Texture.ColorSpaceTypes.Srgb, VrmLib.Texture.TextureTypes.Default);
        //         var imageTexture = textureInfo.Texture as VrmLib.ImageTexture;
        //         EqualTextureColor(srcSRGBTexture, imageTexture, false);
        //     }

        //     {
        //         material.SetTexture(VrmLib.MToon.Utils.PropOutlineWidthTexture, srcLinearTexture);
        //         var textureInfo = converter.GetOrCreateTexture(material, srcLinearTexture, VrmLib.Texture.ColorSpaceTypes.Linear, VrmLib.Texture.TextureTypes.Default);
        //         var imageTexture = textureInfo.Texture as VrmLib.ImageTexture;
        //         EqualTextureColor(srcLinearTexture, imageTexture, true);
        //     }

        //     yield return null;
        // }

        // [UnityTest]
        // public IEnumerator MToonTextureToUnity()
        // {
        //     yield return null;
        // }
        #endregion
    }
}