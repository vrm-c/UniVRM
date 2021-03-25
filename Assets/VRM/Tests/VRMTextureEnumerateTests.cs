using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniGLTF;


namespace VRM
{

    public class VRMTextureEnumerateTests
    {
        /// <summary>
        /// Test uniqueness
        /// </summary>
        [Test]
        public void TextureEnumerationTest()
        {
            {
                var parser = new GltfParser
                {
                    GLTF = new glTF
                    {
                        images = new List<glTFImage>
                        {
                            new glTFImage{
                                mimeType = "image/png",
                            }
                        },
                        textures = new List<glTFTexture>
                        {
                            new glTFTexture{
                                source = 0,
                            }
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
                                        index = 0,
                                    }
                                }
                            },
                        }
                    }
                };
                var vrm = new glTF_VRM_extensions
                {
                    materialProperties = new List<glTF_VRM_Material>
                    {
                        new glTF_VRM_Material
                        {
                            textureProperties = new Dictionary<string, int>
                            {
                                {"_MainTex", 0},
                            }
                        },
                        new glTF_VRM_Material
                        {
                            textureProperties = new Dictionary<string, int>
                            {
                                {"_MainTex", 0},
                            }
                        },
                     }
                };
                var items = new VRMTextureEnumerator(vrm).Enumerate(parser).ToArray();
                Assert.AreEqual(1, items.Length);
            }
        }
    }
}
