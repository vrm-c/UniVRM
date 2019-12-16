using NUnit.Framework;
using UnityEngine;


namespace UniGLTF
{
    public class MaterialTests
    {
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
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
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
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
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
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
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
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
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
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
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
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
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
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
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
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
                });
                Assert.AreEqual("UniGLTF/UniUnlit", shader.name);
            }

            {
                // default
                var shader = shaderStore.GetShader(new glTFMaterial
                {
                    extensions = new glTFMaterial_extensions
                    {
                        KHR_materials_unlit = new glTF_KHR_materials_unlit { }
                    }
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
