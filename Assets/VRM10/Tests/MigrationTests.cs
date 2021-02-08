using System.IO;
using NUnit.Framework;
using UnityEngine;
using UniJSON;
using System;
using UniGLTF;

namespace UniVRM10
{
    public class MigrationTests
    {
        static string AliciaPath
        {
            get
            {
                return Path.GetFullPath(Application.dataPath + "/../Tests/Models/Alicia_vrm-0.51/AliciaSolid_vrm-0.51.vrm")
                    .Replace("\\", "/");
            }
        }

        static JsonNode GetVRM0(byte[] bytes)
        {
            var glb = UniGLTF.Glb.Parse(bytes);
            var json = glb.Json.Bytes.ParseAsJson();
            return json["extensions"]["VRM"];
        }

        T GetExtension<T>(UniGLTF.glTFExtension extensions, UniJSON.Utf8String key, Func<JsonNode, T> deserializer)
        {
            if (extensions is UniGLTF.glTFExtensionImport import)
            {
                foreach (var kv in import.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == key)
                    {
                        return deserializer(kv.Value);
                    }
                }
            }

            return default;
        }

        [Test]
        public void Migrate0to1()
        {
            var vrm0Bytes = File.ReadAllBytes(AliciaPath);
            var vrm0Json = GetVRM0(vrm0Bytes);

            var vrm1 = MigrationVrm.Migrate(vrm0Bytes);
            var glb = UniGLTF.Glb.Parse(vrm1);
            var json = glb.Json.Bytes.ParseAsJson();
            var gltf = UniGLTF.GltfDeserializer.Deserialize(json);

            MigrationVrm.Check(vrm0Json, GetExtension(gltf.extensions, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm.ExtensionNameUtf8,
                UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.Deserialize));
            MigrationVrm.Check(vrm0Json, GetExtension(gltf.extensions, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone.ExtensionNameUtf8,
                UniGLTF.Extensions.VRMC_springBone.GltfDeserializer.Deserialize), gltf.nodes);
        }

        const float EPS = 1e-4f;

        static bool Nearly(float l, float r)
        {
            return Mathf.Abs(l - r) <= EPS;
        }

        static bool Nearly(Vector3 l, Vector3 r)
        {
            if (!Nearly(l.x, r.x)) return false;
            if (!Nearly(l.y, r.y)) return false;
            if (!Nearly(l.z, r.z)) return false;
            return true;
        }

        [Test]
        public void RotateTest()
        {
            var euler = new Vector3(0, 10, 20);
            var r = Quaternion.Euler(euler);
            var node = new glTFNode
            {
                translation = new float[] { 1, 2, 3 },
                rotation = new float[] { r.x, r.y, r.z, r.w },
                scale = new float[] { 1, 2, 3 },
            };
            RotateY180.Rotate(node);

            Assert.AreEqual(new Vector3(-1, 2, 3), node.translation.ToVector3().ToUnityVector3());
            Assert.AreEqual(new Vector3(1, 2, 3), node.scale.ToVector3().ToUnityVector3());

            var result = node.rotation.ToQuaternion().ToUnityQuaternion().eulerAngles;
            Debug.LogFormat($"{result}");

            Assert.True(Nearly(0, result.x));
            Assert.True(Nearly(360 - 10, result.y));
            Assert.True(Nearly(360 - 20, result.z));
        }
    }
}
