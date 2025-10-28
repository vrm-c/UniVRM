using NUnit.Framework;
using System.IO;
using UniGLTF;
using UniGLTF.MeshUtility;
using UniJSON;
using UnityEngine;
using System;

namespace VRM.Samples
{
    public static class JsonExtensions
    {
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
            using var data = new GlbFileParser(path).Parse();
            var vrmData = new VRMData(data);
            var materialGenerator = new BuiltInVrmMaterialDescriptorGenerator(vrmData.VrmExtension);
            using var context = new VRMImporterContext(vrmData, materialGenerator: materialGenerator);
            using var loaded = context.Load();
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
                        if (unityBlendShapeClip == null)
                        {
                            continue;
                        }
                        Assert.AreEqual(Enum.Parse(typeof(BlendShapePreset), gltfBlendShapeClip.presetName, true), unityBlendShapeClip.Preset);
                    }
                }
            }
        }

        [Test]
        public void MeshCopyTest()
        {
            var path = AliciaPath;
            using var data = new GlbFileParser(path).Parse();
            var vrmData = new VRMData(data);
            var materialGenerator = new BuiltInVrmMaterialDescriptorGenerator(vrmData.VrmExtension);
            using var context = new VRMImporterContext(vrmData, materialGenerator: materialGenerator);
            using var loaded = context.Load();
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

            using var data = new GlbFileParser(path).Parse();
            var vrmData = new VRMData(data);
            var materialGenerator = new BuiltInVrmMaterialDescriptorGenerator(vrmData.VrmExtension);
            using var context = new VRMImporterContext(vrmData, materialGenerator: materialGenerator);
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
