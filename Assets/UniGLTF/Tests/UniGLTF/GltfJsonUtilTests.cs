using NUnit.Framework;
using UniJSON;
using System.Linq;

namespace UniGLTF
{
    public class GltfJsonUtilTests
    {
        [Test]
        public void Update_extensionUsed()
        {
            var dst = GltfJsonUtil.Update_extensionsUsed(@"{
    ""asset"": {
        ""generator"": ""COLLADA2GLTF"",
        ""version"": ""2.0""
    },
    ""scene"": 0,
    ""scenes"": [
        {
            ""nodes"": [
                0
            ]
    }
    ],
    ""materials"": [
        {
            ""pbrMetallicRoughness"": {
                ""baseColorFactor"": [
                    0.800000011920929,
                    0.0,
                    0.0,
                    1.0
                ],
                ""metallicFactor"": 0.0
            },
            ""name"": ""Red"",
            ""extensions"": {
                ""KHR_materials_unlit"": {}
            }
        }
    ]
}");

            var parsed = dst.ParseAsJson();

            Assert.AreEqual(new string[] { "KHR_materials_unlit" },
            parsed["extensionUsed"].ArrayItems().Select(x => x.GetString()).ToArray());
        }

        [Test]
        public void Replace_extensionUsed()
        {
            var dst = GltfJsonUtil.Update_extensionsUsed(@"{
    ""asset"": {
        ""generator"": ""COLLADA2GLTF"",
        ""version"": ""2.0""
    },
    ""scene"": 0,
    ""scenes"": [
        {
            ""nodes"": [
                0
            ]
    }
    ],
    ""extensionUsed"": [""dummy""],
    ""materials"": [
        {
            ""pbrMetallicRoughness"": {
                ""baseColorFactor"": [
                    0.800000011920929,
                    0.0,
                    0.0,
                    1.0
                ],
                ""metallicFactor"": 0.0
            },
            ""name"": ""Red"",
            ""extensions"": {
                ""KHR_materials_unlit"": {}
            }
        }
    ]
}");

            var parsed = dst.ParseAsJson();

            Assert.AreEqual(new string[] { "KHR_materials_unlit" },
            parsed["extensionUsed"].ArrayItems().Select(x => x.GetString()).ToArray());
        }
    }
}