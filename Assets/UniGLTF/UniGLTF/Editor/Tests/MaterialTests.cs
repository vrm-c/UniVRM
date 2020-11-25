using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniJSON;
using System.Linq;

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

            var shaderStore = new ShaderStore(null);
            var materialImporter = new MaterialImporter(shaderStore, (int index) => { return null; });
            var dstMaterial = materialImporter.CreateMaterial(0, gltfMaterial, false);

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

                var material = new glTFMaterial
                {
                    alphaMode = "OPAQUE",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                    },
                    extensions = extension,
                };

                Assert.IsTrue(glTF_KHR_materials_unlit.IsEnable(material));

                var shaderStore = new ShaderStore(null);
                var shader = shaderStore.GetShader(material);
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }
        }

        [Test]
        public void UnlitShaderImportTest()
        {
            var shaderStore = new ShaderStore(null);

            {
                // OPAQUE/Color
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    alphaMode = "OPAQUE",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // OPAQUE/Texture
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    alphaMode = "OPAQUE",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo(),
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // OPAQUE/Color/Texture
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    alphaMode = "OPAQUE",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo(),
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // BLEND/Color
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    alphaMode = "BLEND",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // BLEND/Texture
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    alphaMode = "BLEND",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo(),
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // BLEND/Color/Texture
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    alphaMode = "BLEND",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo(),
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // MASK/Texture
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    alphaMode = "MASK",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo(),
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // MASK/Color/Texture
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    alphaMode = "MASK",
                    pbrMetallicRoughness = new glTFPbrMetallicRoughness
                    {
                        baseColorFactor = new float[] { 1, 0, 0, 1 },
                        baseColorTexture = new glTFMaterialBaseColorTextureInfo(),
                    },
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // default
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    extensions = glTF_KHR_materials_unlit.Serialize().Deserialize(),
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }
        }

        [Test]
        public void MaterialImportTest()
        {
            var shaderStore = new ShaderStore(null);
            var materialImporter = new MaterialImporter(shaderStore, null);

            {
                var material = materialImporter.CreateMaterial(0, new glTFMaterial { }, false);
                Assert.AreEqual("Standard", material.shader.name);
            }
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
