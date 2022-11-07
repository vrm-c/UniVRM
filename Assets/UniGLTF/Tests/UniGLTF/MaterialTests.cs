using System;
using NUnit.Framework;
using UnityEngine;
using UniJSON;
using System.Linq;
using VRMShaders;

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

            var textureExporter = new TextureExporter(new EditorTextureSerializer());
            var srcMaterial = new Material(Shader.Find("Standard"));

            var offset = new Vector2(0.3f, 0.2f);
            var scale = new Vector2(0.5f, 0.6f);

            srcMaterial.mainTexture = tex0;
            srcMaterial.mainTextureOffset = offset;
            srcMaterial.mainTextureScale = scale;

            var materialExporter = new BuiltInGltfMaterialExporter();
            var gltfMaterial = materialExporter.ExportMaterial(srcMaterial, textureExporter, new GltfExportSettings());
            gltfMaterial.pbrMetallicRoughness.baseColorTexture.extensions = gltfMaterial.pbrMetallicRoughness.baseColorTexture.extensions.Deserialize();

            Assert.IsTrue(glTF_KHR_texture_transform.TryGet(gltfMaterial.pbrMetallicRoughness.baseColorTexture, out glTF_KHR_texture_transform t));
            Assert.AreEqual(t.offset[0], offset.x, 0.3f);
            Assert.AreEqual(t.offset[1], offset.y, 0.2f);
            Assert.AreEqual(t.scale[0], scale.x, 0.5f);
            Assert.AreEqual(t.scale[1], scale.y, 0.6f);
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
                var extension = glTF_KHR_materials_unlit.ForTest();
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

                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
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
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
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
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
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
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
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
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
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
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
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
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
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
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
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
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
            }

            {
                // default
                var gltfMaterial = new glTFMaterial
                {
                    extensions = glTF_KHR_materials_unlit.ForTest(),
                };
                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(gltfMaterial));
            }
        }

        [Test]
        public void MaterialEmissiveStrengthTest()
        {
            // serialize
            var material = new glTFMaterial();
            glTF_KHR_materials_emissive_strength.Serialize(ref material.extensions, 5.0f);
            var json = material.ToJson();
            var parsed = json.ParseAsJson();
            Assert.AreEqual(parsed["extensions"]["KHR_materials_emissive_strength"]["emissiveStrength"].GetSingle(), 5.0f);

            // deserialize
            var imported = GltfDeserializer.Deserialize_gltf_materials_ITEM(parsed);
            Assert.True(glTF_KHR_materials_emissive_strength.TryGet(imported.extensions, out glTF_KHR_materials_emissive_strength extension));
            Assert.AreEqual(extension.emissiveStrength, 5.0f);
        }

        [Test]
        public void MaterialImportTest()
        {
            Assert.IsFalse(glTF_KHR_materials_unlit.IsEnable(new glTFMaterial { }));
        }

        [Test]
        public void MaterialExportTest()
        {
            var material = new Material(Shader.Find("Standard"));
            material.SetColor("_EmissionColor", new Color(0, 1, 2, 1));
            material.EnableKeyword("_EMISSION");
            var materialExporter = new BuiltInGltfMaterialExporter();
            var textureExporter = new TextureExporter(new EditorTextureSerializer());
            var gltfMaterial = materialExporter.ExportMaterial(material, textureExporter, new GltfExportSettings());

            var maxComponent = Mathf.GammaToLinearSpace(2f);
            Assert.That(gltfMaterial.emissiveFactor, Is.EqualTo(new float[] { 0f, 1f / maxComponent, 1f }).Within(0.5f / 255f));
        }
    }
}
