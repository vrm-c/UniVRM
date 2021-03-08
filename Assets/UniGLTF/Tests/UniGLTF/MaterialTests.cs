using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniJSON;
using System.Linq;
using System.Threading.Tasks;

namespace UniGLTF
{
    public class MaterialTests
    {

        [Test]
        public void TextureTransformTest()
        {
            var tex0 = new Texture2D(128, 128)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };

            var textureManager = new TextureExportManager(new Texture[] { tex0 });
            var srcMaterial = new Material(Shader.Find("Standard"));

            var offset = new Vector2(0.3f, 0.2f);
            var scale = new Vector2(0.5f, 0.6f);

            srcMaterial.mainTexture = tex0;
            srcMaterial.mainTextureOffset = offset;
            srcMaterial.mainTextureScale = scale;

            var materialExporter = new MaterialExporter();
            var gltfMaterial = materialExporter.ExportMaterial(srcMaterial, textureManager);
            gltfMaterial.pbrMetallicRoughness.baseColorTexture.extensions = gltfMaterial.pbrMetallicRoughness.baseColorTexture.extensions.Deserialize();

            var dstMaterial = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);

            Assert.AreEqual(dstMaterial.mainTextureOffset.x, offset.x, 0.3f);
            Assert.AreEqual(dstMaterial.mainTextureOffset.y, offset.y, 0.2f);
            Assert.AreEqual(dstMaterial.mainTextureScale.x, scale.x, 0.5f);
            Assert.AreEqual(dstMaterial.mainTextureScale.y, scale.y, 0.6f);
        }

        [Test]
        public void glTF_KHR_materials_unlit_Test()
        {
            {
                var value = "{}".ParseAsJson();
                Assert.AreEqual(value.Value.ValueType, ValueNodeType.Object);
                Assert.AreEqual(0, value.GetObjectCount());
                var list = value.ObjectItems().ToArray();
                Assert.AreEqual(0, list.Length);
            }

            {
                var value = "{\"unlit\":{}}".ParseAsJson();
                Assert.AreEqual(value.Value.ValueType, ValueNodeType.Object);
                Assert.AreEqual(1, value.GetObjectCount());
                var list = value.ObjectItems().ToArray();
                Assert.AreEqual(1, list.Length);
                Assert.AreEqual("unlit", list[0].Key.GetString());
                Assert.AreEqual(0, list[0].Value.GetObjectCount());
            }

            {
                var extension = glTF_KHR_materials_unlit.Serialize().Deserialize();
                var list = extension.ObjectItems().ToArray();
                Assert.AreEqual(1, list.Length);
                Assert.AreEqual(glTF_KHR_materials_unlit.ExtensionNameUtf8, list[0].Key.GetUtf8String());
                Assert.AreEqual(0, list[0].Value.GetObjectCount());

                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "OPAQUE",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                    },
                    extensions = extension,
                };

                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));

                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }
        }

        [Test]
        public void UnlitShaderImportTest()
        {
            {
                // OPAQUE/Color
                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "OPAQUE",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }

            {
                // OPAQUE/Texture
                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "OPAQUE",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo
                        {
                            index = 0,
                        },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }

            {
                // OPAQUE/Color/Texture
                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "OPAQUE",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo
                        {
                            index = 0
                        },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }

            {
                // BLEND/Color
                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "BLEND",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }

            {
                // BLEND/Texture
                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "BLEND",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo
                        {
                            index = 0,
                        },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }

            {
                // BLEND/Color/Texture
                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "BLEND",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo
                        {
                            index = 0,
                        },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }

            {
                // MASK/Texture
                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "MASK",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo
                        {
                            index = 0,
                        },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }

            {
                // MASK/Color/Texture
                var gltfMaterial = new glTFMaterial
                {
                    alphaMode = "MASK",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo
                        {
                            index = 0,
                        },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }

            {
                // default
                var gltfMaterial = new glTFMaterial
                {
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                };
                var material = MaterialFactory.CreateMaterialForTest(0, gltfMaterial);
                Assert.AreEqual("UniGLTF/UniUnlit", material.shader.name);
            }
        }

        [Test]
        public void MaterialImportTest()
        {
            var material = MaterialFactory.CreateMaterialForTest(0, new glTFMaterial { });
            Assert.AreEqual("Standard", material.shader.name);
        }

        [Test]
        public void MaterialExportTest()
        {
            var material = new Material(Shader.Find("Standard"));
            material.SetColor("_EmissionColor", new Color(0, 1, 2, 1));
            material.EnableKeyword("_EMISSION");
            var materialExporter = new MaterialExporter();
            var textureExportManager = new TextureExportManager(new Texture[] { });
            var gltfMaterial = materialExporter.ExportMaterial(material, textureExportManager);

            Assert.AreEqual(gltfMaterial.emissiveFactor, new float[] { 0, 0.5f, 1 });
        }
    }
}
