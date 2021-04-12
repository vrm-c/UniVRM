using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UniJSON;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    static class ToJsonExtensions
    {
        public static string ToJson(this glTF self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFMesh self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_meshes_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFPrimitives self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_meshes__primitives_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFAttributes self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_meshes__primitives__attributes(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFMaterialBaseColorTextureInfo self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_materials__pbrMetallicRoughness_baseColorTexture(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFMaterial self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_materials_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFNode self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_nodes_ITEM(f, self);
            return f.ToString();
        }
        public static string ToJson(this glTFSkin self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize_gltf_skins_ITEM(f, self);
            return f.ToString();
        }
    }

    public class UniGLTFTests
    {
        static GameObject CreateSimpleScene()
        {
            var root = new GameObject("gltfRoot").transform;

            var scene = new GameObject("scene0").transform;
            scene.SetParent(root, false);
            scene.localPosition = new Vector3(1, 2, 3);

            return root.gameObject;
        }

        void AssertAreEqual(Transform go, Transform other)
        {
            var lt = go.Traverse().GetEnumerator();
            var rt = go.Traverse().GetEnumerator();

            while (lt.MoveNext())
            {
                if (!rt.MoveNext())
                {
                    throw new Exception("rt shorter");
                }

                MonoBehaviourComparator.AssertAreEquals(lt.Current.gameObject, rt.Current.gameObject);
            }

            if (rt.MoveNext())
            {
                throw new Exception("rt longer");
            }
        }

        [Test]
        public void UniGLTFSimpleSceneTest()
        {
            var go = CreateSimpleScene();

            // export
            var gltf = new glTF();

            string json = null;
            using (var exporter = new gltfExporter(gltf))
            {
                exporter.Prepare(go);
                exporter.Export(MeshExportSettings.Default, AssetTextureUtil.IsTextureEditorAsset, AssetTextureUtil.GetTextureBytesWithMime);

                // remove empty buffer
                gltf.buffers.Clear();

                json = gltf.ToJson();
            }

            // parse
            var parser = new GltfParser();
            parser.ParseJson(json, new SimpleStorage(new ArraySegment<byte>()));

            // import
            using (var context = new ImporterContext(parser))
            {
                context.Load();
                AssertAreEqual(go.transform, context.Root.transform);
            }
        }

        void BufferTest(int init, params int[] size)
        {
            var initBytes = init == 0 ? null : new byte[init];
            var storage = new ArrayByteBuffer(initBytes);
            var buffer = new glTFBuffer(storage);

            var values = new List<byte>();
            int offset = 0;
            foreach (var x in size)
            {
                var nums = Enumerable.Range(offset, x).Select(y => (Byte)y).ToArray();
                values.AddRange(nums);
                var bytes = new ArraySegment<Byte>(nums);
                offset += x;
                buffer.Append(bytes, glBufferTarget.NONE);
            }

            Assert.AreEqual(values.Count, buffer.byteLength);
            Assert.True(Enumerable.SequenceEqual(values, buffer.GetBytes().ToArray()));
        }

        [Test]
        public void BufferTest()
        {
            BufferTest(0, 0, 100, 200);
            BufferTest(0, 128);
            BufferTest(0, 256);

            BufferTest(1024, 0);
            BufferTest(1024, 128);
            BufferTest(1024, 2048);
            BufferTest(1024, 900, 900);
        }

        [Test]
        public void UnityPathTest()
        {
            var root = UnityPath.FromUnityPath(".");
            Assert.IsFalse(root.IsNull);
            Assert.IsFalse(root.IsUnderAssetsFolder);
            Assert.AreEqual(UnityPath.FromUnityPath("."), root);

            var assets = UnityPath.FromUnityPath("Assets");
            Assert.IsFalse(assets.IsNull);
            Assert.IsTrue(assets.IsUnderAssetsFolder);

            var rootChild = root.Child("Assets");
            Assert.AreEqual(assets, rootChild);

            var assetsChild = assets.Child("Hoge");
            var hoge = UnityPath.FromUnityPath("Assets/Hoge");
            Assert.AreEqual(assetsChild, hoge);

            //var children = root.TraverseDir().ToArray();
        }

        [Test]
        public void VersionChecker()
        {
            Assert.False(glTFExtensions.IsGeneratedUniGLTFAndOlderThan("hoge", 1, 16));
            Assert.False(glTFExtensions.IsGeneratedUniGLTFAndOlderThan("UniGLTF-1.16", 1, 16));
            Assert.True(glTFExtensions.IsGeneratedUniGLTFAndOlderThan("UniGLTF-1.15", 1, 16));
            Assert.False(glTFExtensions.IsGeneratedUniGLTFAndOlderThan("UniGLTF-11.16", 1, 16));
            Assert.True(glTFExtensions.IsGeneratedUniGLTFAndOlderThan("UniGLTF-0.16", 1, 16));
            Assert.True(glTFExtensions.IsGeneratedUniGLTFAndOlderThan("UniGLTF", 1, 16));
        }

        [Test]
        public void MeshTest()
        {
            var model = new glTFMesh("mesh")
            {
                primitives = new List<glTFPrimitives>
                {
                    new glTFPrimitives
                    {
                        indices = 0,
                        attributes = new glTFAttributes
                        {
                            POSITION = 1,
                        }
                    }
                },
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""name"":""mesh"",""primitives"":[{""mode"":0,""indices"":0,""attributes"":{""POSITION"":1},""material"":0}]}", json);
            Debug.Log(json);
        }

        [Test]
        public void PrimitiveTest()
        {
            var model = new glTFPrimitives
            {
                indices = 0,
                attributes = new glTFAttributes
                {
                    POSITION = 1,
                },
                extras = gltf_mesh_extras_targetNames.Serialize("aaa"),
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""mode"":0,""indices"":0,""attributes"":{""POSITION"":1},""material"":0,""extras"":{""targetNames"":[""aaa""]}}", json);
            Debug.Log(json);
        }

        [Test]
        public void AttributesTest()
        {
            var model = new glTFAttributes
            {
                POSITION = 0,
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""POSITION"":0}", json);
            Debug.Log(json);
        }

        [Test]
        public void TextureInfoTest()
        {
            var model = new glTFMaterialBaseColorTextureInfo()
            {
                index = 1,
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""index"":1,""texCoord"":0}", json);
            Debug.Log(json);
        }

        [Test]
        public void TextureInfoTestError()
        {
            var model = new glTFMaterialBaseColorTextureInfo();
        }

        [Test]
        public void MaterialTest()
        {
            var model = new glTFMaterial()
            {
                name = "a",
                emissiveFactor = new float[] { 0.5f, 0.5f, 0.5f },
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""name"":""a"",""pbrMetallicRoughness"":{""baseColorFactor"":[1,1,1,1],""metallicFactor"":1,""roughnessFactor"":1},""emissiveFactor"":[0.5,0.5,0.5],""doubleSided"":false}", json);
            Debug.Log(json);
        }

        [Test]
        public void MaterialAlphaTest()
        {
            var model = new glTFMaterial()
            {
                name = "a",
                emissiveFactor = new float[] { 0.5f, 0.5f, 0.5f },
                alphaMode = "MASK",
            };
        }

        [Test]
        public void GlTFToJsonTest()
        {
            var gltf = new glTF();
            using (var exporter = new gltfExporter(gltf))
            {
                exporter.Prepare(CreateSimpleScene());
                exporter.Export(MeshExportSettings.Default, AssetTextureUtil.IsTextureEditorAsset, AssetTextureUtil.GetTextureBytesWithMime);
            }

            var expected = gltf.ToJson().ParseAsJson();
            expected.AddKey(Utf8String.From("meshes"));
            expected.AddValue(default(ArraySegment<byte>), ValueNodeType.Array);
            expected["meshes"].AddValue(default(ArraySegment<byte>), ValueNodeType.Object);

            var mesh = expected["meshes"][0];
            mesh.AddKey(Utf8String.From("name"));
            mesh.AddValue(Utf8String.From(JsonString.Quote("test")).Bytes, ValueNodeType.String);
            mesh.AddKey(Utf8String.From("primitives"));
            mesh.AddValue(default(ArraySegment<byte>), ValueNodeType.Array);
            mesh["primitives"].AddValue(default(ArraySegment<byte>), ValueNodeType.Object);

            var primitive = mesh["primitives"][0];
            primitive.AddKey(Utf8String.From("mode"));
            primitive.AddValue(Utf8String.From("0").Bytes, ValueNodeType.Integer);
            primitive.AddKey(Utf8String.From("indices"));
            primitive.AddValue(Utf8String.From("0").Bytes, ValueNodeType.Integer);
            primitive.AddKey(Utf8String.From("material"));
            primitive.AddValue(Utf8String.From("0").Bytes, ValueNodeType.Integer);
            primitive.AddKey(Utf8String.From("attributes"));
            primitive.AddValue(default(ArraySegment<byte>), ValueNodeType.Object);
            primitive["attributes"].AddKey(Utf8String.From("POSITION"));
            primitive["attributes"].AddValue(Utf8String.From("0").Bytes, ValueNodeType.Integer);
            primitive.AddKey(Utf8String.From("targets"));
            primitive.AddValue(default(ArraySegment<byte>), ValueNodeType.Array);
            primitive["targets"].AddValue(default(ArraySegment<byte>), ValueNodeType.Object);
            primitive["targets"][0].AddKey(Utf8String.From("POSITION"));
            primitive["targets"][0].AddValue(Utf8String.From("1").Bytes, ValueNodeType.Integer);
            primitive["targets"].AddValue(default(ArraySegment<byte>), ValueNodeType.Object);
            primitive["targets"][1].AddKey(Utf8String.From("POSITION"));
            primitive["targets"][1].AddValue(Utf8String.From("2").Bytes, ValueNodeType.Integer);
            primitive["targets"][1].AddKey(Utf8String.From("TANGENT"));
            primitive["targets"][1].AddValue(Utf8String.From("0").Bytes, ValueNodeType.Integer);

            gltf.meshes.Add(new glTFMesh("test")
            {
                primitives = new List<glTFPrimitives>
                {
                    new glTFPrimitives
                    {
                        indices = 0,
                        attributes = new glTFAttributes
                        {
                            POSITION = 0,
                            TANGENT = -1 // should be removed
                        },
                        targets = new List<gltfMorphTarget>
                        {
                            new gltfMorphTarget
                            {
                                POSITION = 1,
                                TANGENT = -1 // should be removed
                            },
                            new gltfMorphTarget
                            {
                                POSITION = 2,
                                TANGENT = 0
                            }
                        }
                    }
                }
            });
            var actual = gltf.ToJson().ParseAsJson();

            Assert.AreEqual(expected, actual);
        }

        public void MaterialTestError()
        {
            var model = new glTFMaterial()
            {
                name = "b",
                emissiveFactor = new float[] { 1.5f, 0.5f, 0.5f },
            };
        }

        [Test]
        public void NodeTest()
        {
            var model = new glTFNode()
            {
                name = "a",
                skin = 0,
                camera = -1,
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""name"":""a"",""skin"":0}", json);
            Debug.Log(json);
        }

        [Test]
        public void NodeMeshTest()
        {
            var model = new glTFNode()
            {
                name = "a",
                mesh = 2,
                skin = 0,
                camera = -1,
            };
        }

        [Test]
        public void NodeTestError()
        {
            var model = new glTFNode()
            {
                name = "a",
                camera = -2,
            };
        }

        [Test]
        public void SkinTest()
        {
            var model = new glTFSkin()
            {
                inverseBindMatrices = 0,
                name = "b",
                joints = new int[] { 1 },
            };

            var json = model.ToJson();
            Assert.AreEqual(@"{""inverseBindMatrices"":0,""joints"":[1],""name"":""b""}", json);
            Debug.Log(json);
        }

        [Test]
        public void SkinTestEmptyName()
        {
            var model = new glTFSkin()
            {
                name = "",
                inverseBindMatrices = 4,
                joints = new int[] { 1 },
            };

            var json = model.ToJson();
            // "name" = "", not excluded
            Assert.AreEqual(@"{""inverseBindMatrices"":4,""joints"":[1]}", json);
            Debug.Log(json);
        }

        [Test]
        public void SkinTestErrorNull()
        {
            var model = new glTFSkin()
            {
                name = "b",
                joints = null,
            };
        }

        [Test]
        public void SkinTestError()
        {
            var model = new glTFSkin()
            {
                name = "b",
                joints = new int[] { },
            };
        }

        [Test]
        public void AssetsTest()
        {
            var model = new glTFAssets()
            {
                version = "0.49",
            };

            //var json = model.ToJson();
            //Assert.AreEqual(@"{""inverseBindMatrices"":-1,""joints"":[1]}", json);
            //Debug.Log(json);
        }

        [Test]
        public void AssetsTestError()
        {
            var model = new glTFAssets();

            //var json = model.ToJson();
            //Assert.AreEqual(@"{""inverseBindMatrices"":-1,""joints"":[1]}", json);
            //Debug.Log(json);
        }

        [Test]
        public void GLTFTest()
        {
            var model = new glTF()
            {
                asset = new glTFAssets()
                {
                    version = "0.49",
                },
            };
        }

        [Test]
        public void SameMeshButDifferentMaterialExport()
        {
            var go = new GameObject("same_mesh");
            try
            {
                var shader = Shader.Find("Unlit/Color");

                var cubeA = GameObject.CreatePrimitive(PrimitiveType.Cube);
                {
                    cubeA.transform.SetParent(go.transform);
                    var material = new Material(shader);
                    material.name = "red";
                    material.color = Color.red;
                    cubeA.GetComponent<Renderer>().sharedMaterial = material;
                }

                {
                    var cubeB = GameObject.Instantiate(cubeA);
                    cubeB.transform.SetParent(go.transform);
                    var material = new Material(shader);
                    material.color = Color.blue;
                    material.name = "blue";
                    cubeB.GetComponent<Renderer>().sharedMaterial = material;

                    Assert.AreEqual(cubeB.GetComponent<MeshFilter>().sharedMesh, cubeA.GetComponent<MeshFilter>().sharedMesh);
                }

                // export
                var gltf = new glTF();
                var json = default(string);
                using (var exporter = new gltfExporter(gltf))
                {
                    exporter.Prepare(go);
                    exporter.Export(UniGLTF.MeshExportSettings.Default, AssetTextureUtil.IsTextureEditorAsset, AssetTextureUtil.GetTextureBytesWithMime);

                    json = gltf.ToJson();
                }

                Assert.AreEqual(2, gltf.meshes.Count);

                var red = gltf.materials[gltf.meshes[0].primitives[0].material];
                Assert.AreEqual(new float[] { 1, 0, 0, 1 }, red.pbrMetallicRoughness.baseColorFactor);

                var blue = gltf.materials[gltf.meshes[1].primitives[0].material];
                Assert.AreEqual(new float[] { 0, 0, 1, 1 }, blue.pbrMetallicRoughness.baseColorFactor);

                Assert.AreEqual(2, gltf.nodes.Count);

                Assert.AreNotEqual(gltf.nodes[0].mesh, gltf.nodes[1].mesh);

                // import
                {
                    var parser = new GltfParser();
                    parser.ParseJson(json, new SimpleStorage(new ArraySegment<byte>(new byte[1024 * 1024])));

                    using (var context = new ImporterContext(parser))
                    {
                        //Debug.LogFormat("{0}", context.Json);
                        context.Load();

                        var importedRed = context.Root.transform.GetChild(0);
                        var importedRedMaterial = importedRed.GetComponent<Renderer>().sharedMaterial;
                        Assert.AreEqual("red", importedRedMaterial.name);
                        Assert.AreEqual(Color.red, importedRedMaterial.color);

                        var importedBlue = context.Root.transform.GetChild(1);
                        var importedBlueMaterial = importedBlue.GetComponent<Renderer>().sharedMaterial;
                        Assert.AreEqual("blue", importedBlueMaterial.name);
                        Assert.AreEqual(Color.blue, importedBlueMaterial.color);
                    }
                }

                // import new version
                {
                    var parser = new GltfParser();
                    parser.ParseJson(json, new SimpleStorage(new ArraySegment<byte>(new byte[1024 * 1024])));
                    //Debug.LogFormat("{0}", context.Json);
                    using (var context = new ImporterContext(parser))
                    {
                        context.Load();

                        var importedRed = context.Root.transform.GetChild(0);
                        var importedRedMaterial = importedRed.GetComponent<Renderer>().sharedMaterial;
                        Assert.AreEqual("red", importedRedMaterial.name);
                        Assert.AreEqual(Color.red, importedRedMaterial.color);

                        var importedBlue = context.Root.transform.GetChild(1);
                        var importedBlueMaterial = importedBlue.GetComponent<Renderer>().sharedMaterial;
                        Assert.AreEqual("blue", importedBlueMaterial.name);
                        Assert.AreEqual(Color.blue, importedBlueMaterial.color);
                    }
                }
            }
            finally
            {
                GameObject.DestroyImmediate(go);
            }
        }

        [Serializable]
        class CantConstruct
        {
            public bool Value = true;

            public CantConstruct(bool value)
            {
                throw new Exception();
            }
        }

        [Serializable]
        class Dummy
        {
            public CantConstruct Value = default;
        }

        [Test]
        public void JsonUtilityTest()
        {
            var dummy = JsonUtility.FromJson<Dummy>("{}");
            Assert.NotNull(dummy.Value);
            Assert.False(dummy.Value.Value);
        }
    }
}
