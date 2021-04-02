using System.IO;
using NUnit.Framework;
using UnityEngine;
using UniJSON;
using System;
using UniGLTF;
using System.Runtime.InteropServices;
using System.Collections.Generic;

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
        public void RotateY180Test()
        {
            var euler = new Vector3(0, 10, 20);
            var r = Quaternion.Euler(euler);
            var node = new glTFNode
            {
                translation = new float[] { 1, 2, 3 },
                // rotation = new float[] { r.x, r.y, r.z, r.w },
                scale = new float[] { 1, 2, 3 },
            };
            RotateY180.Rotate(node);

            Assert.AreEqual(new Vector3(-1, 2, -3), node.translation.ToVector3().ToUnityVector3());
            Assert.AreEqual(new Vector3(1, 2, 3), node.scale.ToVector3().ToUnityVector3());

            // var result = node.rotation.ToQuaternion().ToUnityQuaternion().eulerAngles;
            // Debug.LogFormat($"{result}");

            // Assert.True(Nearly(0, result.x));
            // Assert.True(Nearly(10, result.y));
            // Assert.True(Nearly(20, result.z));
        }

        [Test]
        public void UnityEngineMatrixTest()
        {
            var u = new UnityEngine.Matrix4x4();
            u.m00 = 0;
            u.m01 = 1;
            u.m02 = 2;
            u.m03 = 3;
            u.m10 = 4;
            u.m11 = 5;
            u.m12 = 6;
            u.m13 = 7;
            u.m20 = 8;
            u.m21 = 9;
            u.m22 = 10;
            u.m23 = 11;
            u.m30 = 12;
            u.m31 = 13;
            u.m32 = 14;
            u.m33 = 15;
            Assert.AreEqual(new UnityEngine.Vector4(0, 1, 2, 3), u.GetRow(0));
            var bytes = new Byte[64];
            using (var pin = Pin.Create(new[] { u }))
            {
                Marshal.Copy(pin.Ptr, bytes, 0, 64);
            }
            Assert.AreEqual(1.0f, BitConverter.ToSingle(bytes, 16));
        }

        [Test]
        public void NumericMatrixTest()
        {
            var u = new System.Numerics.Matrix4x4();
            u.M11 = 0;
            u.M12 = 1;
            u.M13 = 2;
            u.M14 = 3;
            u.M21 = 4;
            u.M22 = 5;
            u.M23 = 6;
            u.M24 = 7;
            u.M31 = 8;
            u.M32 = 9;
            u.M33 = 10;
            u.M34 = 11;
            u.M41 = 12;
            u.M42 = 13;
            u.M43 = 14;
            u.M44 = 15;
            var bytes = new Byte[64];
            using (var pin = Pin.Create(new[] { u }))
            {
                Marshal.Copy(pin.Ptr, bytes, 0, 64);
            }
            Assert.AreEqual(1.0f, BitConverter.ToSingle(bytes, 4));
        }

        static IEnumerable<FileInfo> EnumerateGltfFiles(DirectoryInfo dir)
        {
            if (dir.Name == ".git")
            {
                yield break;
            }

            foreach (var child in dir.EnumerateDirectories())
            {
                foreach (var x in EnumerateGltfFiles(child))
                {
                    yield return x;
                }
            }

            foreach (var child in dir.EnumerateFiles())
            {
                switch (child.Extension.ToLower())
                {
                    case ".vrm":
                        yield return child;
                        break;
                }
            }
        }

        [Test]
        public void Migrate_VrmTestModels()
        {
            var env = System.Environment.GetEnvironmentVariable("VRM_TEST_MODELS");
            if (string.IsNullOrEmpty(env))
            {
                return;
            }
            var root = new DirectoryInfo(env);
            if (!root.Exists)
            {
                return;
            }

            foreach (var gltf in EnumerateGltfFiles(root))
            {
                var bytes = File.ReadAllBytes(gltf.FullName);
                try
                {
                    var migrated = MigrationVrm.Migrate(bytes);
                    var parser = new GltfParser();
                    parser.Parse(gltf.FullName, migrated);
                    UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(parser.GLTF.extensions, out UniGLTF.Extensions.VRMC_vrm.VRMC_vrm vrm);
                    Assert.NotNull(vrm);
                }
                catch (UnNormalizedException)
                {
                    Debug.LogWarning($"[Not Normalized] {gltf}");
                }
            }
        }
    }
}
