using System.IO;
using System.Linq;
using NUnit.Framework;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public class MaterialImporterTests
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
        public void MaterialImporterTest()
        {
            var migratedBytes = MigrationVrm.Migrate(File.ReadAllBytes(AliciaPath));
            var parser = new GltfParser();
            parser.Parse(AliciaPath, migratedBytes);

            var materialParam = Vrm10MaterialImporter.GetMaterialParam(parser, 0);
            Assert.AreEqual("VRM/MToon", materialParam.ShaderName);
            Assert.AreEqual("Alicia_body", materialParam.TextureSlots["_MainTex"].UnityObjectName);

            var (key, value) = materialParam.EnumerateSubAssetKeyValue().First();
            Assert.AreEqual(new SubAssetKey(typeof(Texture2D), "Alicia_body"), key);
        }
    }
}
