using System.IO;
using NUnit.Framework;
using UnityEngine;
using UniJSON;
using System;

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

        static ListTreeNode<JsonValue> GetVRM0(byte[] bytes)
        {
            var glb = UniGLTF.Glb.Parse(bytes);
            var json = glb.Json.Bytes.ParseAsJson();
            return json["extensions"]["VRM"];
        }

        T GetExtension<T>(UniGLTF.glTFExtension extensions, UniJSON.Utf8String key, Func<ListTreeNode<JsonValue>, T> deserializer)
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

            var vrm1 = Migration.Migrate(vrm0Bytes);
            var glb = UniGLTF.Glb.Parse(vrm1);
            var json = glb.Json.Bytes.ParseAsJson();
            var gltf = UniGLTF.GltfDeserializer.Deserialize(json);

            Migration.Check(vrm0Json, GetExtension(gltf.extensions, UniGLTF.Extensions.VRMC_vrm.VRMC_vrm.ExtensionNameUtf8,
                UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.Deserialize));
            Migration.Check(vrm0Json, GetExtension(gltf.extensions, UniGLTF.Extensions.VRMC_springBone.VRMC_springBone.ExtensionNameUtf8,
                UniGLTF.Extensions.VRMC_springBone.GltfDeserializer.Deserialize), gltf.nodes);
        }
    }
}
