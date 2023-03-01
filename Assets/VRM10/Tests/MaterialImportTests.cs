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
            using (var data = new GlbLowLevelParser(AliciaPath, migratedBytes).Parse())
            {

                var matDesc = new BuiltInVrm10MaterialDescriptorGenerator().Get(data, 0);
                Assert.AreEqual("Alicia_body", matDesc.Name);
                Assert.AreEqual("VRM10/MToon10", matDesc.Shader.name);
                Assert.AreEqual("Alicia_body", matDesc.TextureSlots["_MainTex"].UnityObjectName);
                Assert.AreEqual("Alicia_body", matDesc.TextureSlots["_ShadeTex"].UnityObjectName);

                AreColorEqualApproximately(new Color(1, 1, 1, 1), matDesc.Colors["_Color"]);
                ColorUtility.TryParseHtmlString("#FFDDD6", out var shadeColor);
                AreColorEqualApproximately(shadeColor, matDesc.Colors["_ShadeColor"]);

                Assert.AreEqual(1.0f - 0.1f, matDesc.FloatValues["_GiEqualization"]);

                var (key, value) = matDesc.EnumerateSubAssetKeyValue().First();
                Assert.AreEqual(new SubAssetKey(typeof(Texture2D), "Alicia_body"), key);
            }
        }

        private void AreColorEqualApproximately(Color expected, Color actual)
        {
            Assert.AreEqual(Mathf.RoundToInt(expected.r * 255), Mathf.RoundToInt(actual.r * 255));
            Assert.AreEqual(Mathf.RoundToInt(expected.g * 255), Mathf.RoundToInt(actual.g * 255));
            Assert.AreEqual(Mathf.RoundToInt(expected.b * 255), Mathf.RoundToInt(actual.b * 255));
            Assert.AreEqual(Mathf.RoundToInt(expected.a * 255), Mathf.RoundToInt(actual.a * 255));
        }
    }
}
