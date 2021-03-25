using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


namespace UniGLTF
{

    public class TextureEnumerateTests
    {
        static glTF TwoTexture()
        {
            return new glTF
            {
                images = new List<glTFImage>
                    {
                        new glTFImage{
                            name = "image_0",
                            mimeType = "image/png",
                        },
                        new glTFImage{
                            name = "image_1",
                            mimeType = "image/png",
                        },
                     },
                textures = new List<glTFTexture>
                    {
                        new glTFTexture{
                            name = "texture_0",
                            source = 0,
                        },
                        new glTFTexture{
                            name = "texture_1",
                            source = 1,
                        },
                     },
                materials = new List<glTFMaterial>
                    {
                        new glTFMaterial{
                            pbrMetallicRoughness = new glTFPbrMetallicRoughness{
                                baseColorTexture = new glTFMaterialBaseColorTextureInfo{
                                    index = 0,
                                }
                            }
                        },
                        new glTFMaterial{
                            pbrMetallicRoughness = new glTFPbrMetallicRoughness{
                                baseColorTexture = new glTFMaterialBaseColorTextureInfo{
                                    index = 1,
                                }
                            }
                        },
                        new glTFMaterial{
                            pbrMetallicRoughness = new glTFPbrMetallicRoughness{
                                baseColorTexture = new glTFMaterialBaseColorTextureInfo{
                                    index = 0,
                                }
                            }
                        },
                      },
            };
        }        
        
        static glTF TwoTextureOneUri()
        {
            return new glTF
            {
                images = new List<glTFImage>
                    {
                        new glTFImage{
                            name = "image_0",
                            mimeType = "image/png",
                            uri = "some.png",
                        },
                        new glTFImage{
                            name = "image_1",
                            mimeType = "image/png",
                            uri = "some.png",
                        },
                     },
                textures = new List<glTFTexture>
                    {
                        new glTFTexture{
                            name = "texture_0",
                            source = 0,
                        },
                        new glTFTexture{
                            name = "texture_1",
                            source = 1,
                        },
                     },
                materials = new List<glTFMaterial>
                    {
                        new glTFMaterial{
                            pbrMetallicRoughness = new glTFPbrMetallicRoughness{
                                baseColorTexture = new glTFMaterialBaseColorTextureInfo{
                                    index = 0,
                                }
                            }
                        },
                        new glTFMaterial{
                            pbrMetallicRoughness = new glTFPbrMetallicRoughness{
                                baseColorTexture = new glTFMaterialBaseColorTextureInfo{
                                    index = 1,
                                }
                            }
                        },
                     },
            };
        }

        static glTF TwoTextureOneImage()
        {
            return new glTF
            {
                images = new List<glTFImage>
                    {
                        new glTFImage{
                            name = "image_0",
                            mimeType = "image/png",
                            uri = "some.png",
                        },
                     },
                textures = new List<glTFTexture>
                    {
                        new glTFTexture{
                            name = "texture_0",
                            source = 0,
                        },
                        new glTFTexture{
                            name = "texture_1",
                            source = 0,
                        },
                     },
                materials = new List<glTFMaterial>
                    {
                        new glTFMaterial{
                            pbrMetallicRoughness = new glTFPbrMetallicRoughness{
                                baseColorTexture = new glTFMaterialBaseColorTextureInfo{
                                    index = 0,
                                }
                            }
                        },
                        new glTFMaterial{
                            pbrMetallicRoughness = new glTFPbrMetallicRoughness{
                                baseColorTexture = new glTFMaterialBaseColorTextureInfo{
                                    index = 1,
                                }
                            }
                        },
                     },
            };
        }

        static glTF CombineMetallicSmoothOcclusion()
        {
            return new glTF
            {
                images = new List<glTFImage>
                    {
                        new glTFImage{
                            name = "image_0",
                            mimeType = "image/png",
                            uri = "metallicSmoothness.png",
                        },
                        new glTFImage{
                            name = "image_1",
                            mimeType = "image/png",
                            uri = "occlusion.png",
                        },
                      },
                textures = new List<glTFTexture>
                    {
                        new glTFTexture{
                            name = "texture_0",
                            source = 0,
                        },
                        new glTFTexture{
                            name = "texture_1",
                            source = 1,
                        },
                     },
                materials = new List<glTFMaterial>
                    {
                        new glTFMaterial{
                            pbrMetallicRoughness = new glTFPbrMetallicRoughness{
                                metallicRoughnessTexture = new glTFMaterialMetallicRoughnessTextureInfo{
                                    index = 0,
                                }
                            },
                            occlusionTexture = new glTFMaterialOcclusionTextureInfo{
                                index = 1,
                            }
                        },
                    },
            };
        }

        /// <summary>
        /// Test uniqueness
        /// </summary>
        [Test]
        public void TextureEnumerationTest()
        {
            {
                var parser = new GltfParser
                {
                    GLTF = TwoTexture(),
                };
                var items = GltfTextureEnumerator.Enumerate(parser).ToArray();
                Assert.AreEqual(2, items.Length);
            }

            {
                var parser = new GltfParser
                {
                    GLTF = TwoTextureOneUri(),
                };
                var items = GltfTextureEnumerator.Enumerate(parser).ToArray();
                Assert.AreEqual(1, items.Length);
            }

            {
                var parser = new GltfParser
                {
                    GLTF = TwoTextureOneImage(),
                };
                var items = GltfTextureEnumerator.Enumerate(parser).ToArray();
                Assert.AreEqual(1, items.Length);
            }

            {
                var parser = new GltfParser
                {
                    GLTF = CombineMetallicSmoothOcclusion(),
                };
                var items = GltfTextureEnumerator.Enumerate(parser).ToArray();
                Assert.AreEqual(1, items.Length);
            }
        }
    }
}
