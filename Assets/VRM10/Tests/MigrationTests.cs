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

        UniGLTF.Extensions.VRMC_vrm.VRMC_vrm GetVRMC_vrm(UniGLTF.glTFExtension extensions)
        {
            if (extensions is UniGLTF.glTFExtensionImport import)
            {
                foreach (var kv in import.ObjectItems())
                {
                    if (kv.Key.GetUtf8String() == UniGLTF.Extensions.VRMC_vrm.VRMC_vrm.ExtensionNameUtf8)
                    {
                        return UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.Deserialize(kv.Value);
                    }
                }
            }

            return default;
        }

        static ListTreeNode<JsonValue> GetVRM0(byte[] bytes)
        {
            var glb = UniGLTF.Glb.Parse(bytes);
            var json = glb.Json.Bytes.ParseAsJson();
            return json["extensions"]["VRM"];
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
            var VRMC_vrm = GetVRMC_vrm(gltf.extensions);
            Assert.NotNull(VRMC_vrm);

            Migration.Check(vrm0Json, VRMC_vrm);
        }
    }
}
