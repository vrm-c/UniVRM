using NUnit.Framework;
using System.IO;
using UniGLTF;
using UniGLTF.MeshUtility;
using UniJSON;
using UnityEngine;
using System;
using VRMShaders;

namespace VRM.Samples
{
    public static class JsonExtensions
    {
        public static void SetValue<T>(this JsonNode node, string key, T value, Action<JsonFormatter, T> serialize)
        {
            var f = new JsonFormatter();
            serialize(f, value);
            var p = Utf8String.From(key);
            var bytes = f.GetStoreBytes();
            node.SetValue(p, bytes);
        }

        public static string ToJson(this glTF self)
        {
            var f = new JsonFormatter();
            GltfSerializer.Serialize(f, self);
            return f.ToString();
        }
    }

    public class VRMImportExportTests
    {
        static string AliciaPath
        {
            get
            {
                return Path.GetFullPath(Application.dataPath + "/../Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm")
                    .Replace("\\", "/");
            }
        }

        [Test]
        public void ImportExportTest()
        {
            var path = AliciaPath;
            var data = new GlbFileParser(path).Parse();

            using (var context = new VRMImporterContext(new VRMData(data)))
            using (var loaded = context.Load())
            {
                loaded.ShowMeshes();
                loaded.EnableUpdateWhenOffscreen();

                // mesh
                {
                    foreach (var renderer in loaded.GetComponentsInChildren<Renderer>())
                    {
                        Mesh mesh = default;
                        if (renderer is MeshRenderer)
                        {
                            var f = renderer.GetComponent<MeshFilter>();
                            mesh = f.sharedMesh;
                        }
                        else if (renderer is SkinnedMeshRenderer smr)
                        {
                            mesh = smr.sharedMesh;
                        }

                        var gltfMesh = data.GLTF.meshes.Find(x => x.name == mesh.name);
                        Assert.AreEqual(gltfMesh.name, mesh.name);

                        // materials
                        foreach (var material in renderer.sharedMaterials)
                        {
                            var gltfMaterial = data.GLTF.materials.Find(x => x.name == material.name);
                            Assert.AreEqual(gltfMaterial.name, material.name);

                            var materialIndex = data.GLTF.materials.IndexOf(gltfMaterial);
                            var vrmMaterial = context.VRM.materialProperties[materialIndex];
                            // Debug.Log($"shaderName: '{vrmMaterial.shader}'");
                            if (vrmMaterial.shader == "VRM/MToon")
                            {
                                // MToon
                                // Debug.Log($"{material.name} is MToon");
                                foreach (var kv in vrmMaterial.textureProperties)
                                {
                                    var texture = material.GetTexture(kv.Key);
                                    // Debug.Log($"{kv.Key}: {texture}");
                                    Assert.NotNull(texture);
                                }
                            }
                            else if (glTF_KHR_materials_unlit.IsEnable(gltfMaterial))
                            {
                                // Unlit
                                // Debug.Log($"{material.name} is unlit");
                                throw new NotImplementedException();
                            }
                            else
                            {
                                // PBR
                                // Debug.Log($"{material.name} is PBR");
                                throw new NotImplementedException();
                            }
                        }
                    }
                }

                // meta
                {
                    var meta = loaded.GetComponent<VRMMeta>();
                }

                // humanoid
                {
                    var animator = loaded.GetComponent<Animator>();
                }


                // blendshape
                {
                    var blendshapeProxy = loaded.GetComponent<VRMBlendShapeProxy>();
                    for (int i = 0; i < context.VRM.blendShapeMaster.blendShapeGroups.Count; ++i)
                    {
                        var gltfBlendShapeClip = context.VRM.blendShapeMaster.blendShapeGroups[i];
                        var unityBlendShapeClip = blendshapeProxy.BlendShapeAvatar.Clips[i];
                        Assert.AreEqual(Enum.Parse(typeof(BlendShapePreset), gltfBlendShapeClip.presetName, true), unityBlendShapeClip.Preset);
                    }
                }

                var importedJson = JsonParser.Parse(context.Json);
                importedJson.SetValue("/extensions/VRM/exporterVersion", VRMVersion.VRM_VERSION, (f, x) => f.Value(x));
                importedJson.SetValue("/asset/generator", UniGLTF.UniGLTFVersion.UNIGLTF_VERSION, (f, x) => f.Value(x));
                importedJson.SetValue("/scene", 0, (f, x) => f.Value(x));
                importedJson.SetValue("/materials/*/doubleSided", false, (f, x) => f.Value(x));
                //importJson.SetValue("/materials/*/pbrMetallicRoughness/roughnessFactor", 0);
                //importJson.SetValue("/materials/*/pbrMetallicRoughness/baseColorFactor", new float[] { 1, 1, 1, 1 });
                importedJson.SetValue("/accessors/*/normalized", false, (f, x) => f.Value(x));
                importedJson.RemoveValue(Utf8String.From("/nodes/*/extras"));
                /*
                importJson.SetValue("/bufferViews/12/byteStride", 4);
                importJson.SetValue("/bufferViews/13/byteStride", 4);
                importJson.SetValue("/bufferViews/14/byteStride", 4);
                importJson.SetValue("/bufferViews/15/byteStride", 4);
                importJson.SetValue("/bufferViews/22/byteStride", 4);
                importJson.SetValue("/bufferViews/29/byteStride", 4);
                importJson.SetValue("/bufferViews/45/byteStride", 4);
                importJson.SetValue("/bufferViews/46/byteStride", 4);
                importJson.SetValue("/bufferViews/47/byteStride", 4);
                importJson.SetValue("/bufferViews/201/byteStride", 4);
                importJson.SetValue("/bufferViews/202/byteStride", 4);
                importJson.SetValue("/bufferViews/203/byteStride", 4);
                importJson.SetValue("/bufferViews/204/byteStride", 4);
                importJson.SetValue("/bufferViews/211/byteStride", 4);
                importJson.SetValue("/bufferViews/212/byteStride", 4);
                importJson.SetValue("/bufferViews/213/byteStride", 4);
                importJson.SetValue("/bufferViews/214/byteStride", 4);
                importJson.SetValue("/bufferViews/215/byteStride", 4);
                importJson.SetValue("/bufferViews/243/byteStride", 4);
                importJson.SetValue("/bufferViews/247/byteStride", 64);
                importJson.SetValue("/bufferViews/248/byteStride", 64);
                importJson.SetValue("/bufferViews/249/byteStride", 64);
                importJson.SetValue("/bufferViews/250/byteStride", 64);
                importJson.SetValue("/bufferViews/251/byteStride", 64);
                importJson.SetValue("/bufferViews/252/byteStride", 64);
                importJson.SetValue("/bufferViews/253/byteStride", 64);
                */
                importedJson.RemoveValue(Utf8String.From("/bufferViews/*/byteStride"));

                var vrm = VRMExporter.Export(new GltfExportSettings(), loaded.gameObject, new EditorTextureSerializer());

                // TODO: Check contents in JSON
                /*var exportJson = */
                JsonParser.Parse(vrm.ToJson());

                // TODO: Check contents in JSON
                /*var newExportedJson = */
                // JsonParser.Parse(JsonSchema.FromType<glTF>().Serialize(vrm));

                /*
                foreach (var kv in importJson.Diff(exportJson))
                {
                    Debug.Log(kv);
                }

                Assert.AreEqual(importJson, exportJson);
                */
            }
        }

        [Test]
        public void MeshCopyTest()
        {
            var path = AliciaPath;
            var data = new GlbFileParser(path).Parse();

            using (var context = new VRMImporterContext(new VRMData(data)))
            using (var loaded = context.Load())
            {
                loaded.ShowMeshes();
                loaded.EnableUpdateWhenOffscreen();
                foreach (var mesh in context.Meshes)
                {
                    var src = mesh.Mesh;
                    var dst = src.Copy(true);
                    MeshTests.MeshEquals(src, dst);
                }
            }
        }

        [Test]
        public void SerializerCompare()
        {
            // Aliciaを古いデシリアライザでロードする
            var path = AliciaPath;
            var data = new GlbFileParser(path).Parse();

            using (var context = new VRMImporterContext(new VRMData(data)))
            {
                var oldJson = context.GLTF.ToJson().ParseAsJson().ToString("  ");

                // 生成シリアライザでJSON化する
                var f = new JsonFormatter();
                GltfSerializer.Serialize(f, context.GLTF);
                var parsed = f.ToString().ParseAsJson();
                var newJson = parsed.ToString("  ");

                // File.WriteAllText("old.json", oldJson);
                // File.WriteAllText("new.json", newJson);

                // 比較
                Assert.AreEqual(oldJson.ParseAsJson().ToString(), newJson.ParseAsJson().ToString());

                // 生成デシリアライザでロードする
                var ff = new JsonFormatter();
                var des = GltfDeserializer.Deserialize(parsed);
                ff.Clear();
                GltfSerializer.Serialize(ff, des);
                var desJson = ff.ToString().ParseAsJson().ToString("  ");
                Assert.AreEqual(oldJson.ParseAsJson().ToString(), desJson.ParseAsJson().ToString());
            }
        }
    }
}
