using System.IO;
using NUnit.Framework;
using UnityEngine;
using UniJSON;

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

        UniGLTF.Extensions.VRMC_vrm.VRMC_vrm GetVRM(UniGLTF.glTFExtension extensions)
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

        [Test]
        public void Migrate0to1()
        {
            var vrm0 = File.ReadAllBytes(AliciaPath);
            var vrm1 = Migration.Migrate(vrm0);
            var glb = UniGLTF.Glb.Parse(vrm1);
            var json = glb.Json.Bytes.ParseAsJson();
            var gltf = UniGLTF.GltfDeserializer.Deserialize(json);
            var vrm = GetVRM(gltf.extensions);
            Assert.NotNull(vrm);
        }
    }
}
