using NUnit.Framework;
using UnityEngine;
using UniJSON;

namespace UniGLTF
{
    /// <summary>
    /// glTF 準拠 PBR ShaderGraph (UniGltfPbr) の export パススルー検証。
    ///
    /// ShaderGraph アセット (UniGltfPbr.shadergraph) が契約プロパティを持たない環境では
    /// Assert.Ignore でスキップする。仕様は Packages/UniGLTF/UniGltfPbr/SPEC.md を参照。
    /// </summary>
    public class InvariantRpUniGltfPbrMaterialTests
    {
        private static Material TryCreateMaterialOrIgnore()
        {
            var material = new Material(UniGltfPbrContext.GetShader());
            if (!material.HasProperty("_metallicFactor") || !material.HasProperty("_roughnessFactor"))
            {
                Assert.Ignore("UniGltfPbr shader lacks glTF-native properties (_metallicFactor / _roughnessFactor). See README.md.");
            }
            return material;
        }

        [Test]
        public void MetallicRoughnessFactorPassthroughTest()
        {
            var material = TryCreateMaterialOrIgnore();
            var context = new UniGltfPbrContext(material)
            {
                MetallicFactor = 0.3f,
                RoughnessFactor = 0.7f,
            };

            var textureExporter = new TextureExporter(new EditorTextureSerializer());
            Assert.IsTrue(new InvariantRpUniGltfPbrMaterialExporter().TryExportMaterial(material, textureExporter, out var dst));

            // 反転しない / 1.0 に固定しない (glTF セマンティクスそのまま)
            Assert.AreEqual(0.3f, dst.pbrMetallicRoughness.metallicFactor, 1e-4f);
            Assert.AreEqual(0.7f, dst.pbrMetallicRoughness.roughnessFactor, 1e-4f);
        }

        [Test]
        public void EmissiveStrengthExportTest()
        {
            var material = TryCreateMaterialOrIgnore();
            var context = new UniGltfPbrContext(material)
            {
                EmissiveFactorLinear = new Color(0f, 0.5f, 0f),
                EmissiveStrength = 4.0f,
            };

            var textureExporter = new TextureExporter(new EditorTextureSerializer());
            Assert.IsTrue(new InvariantRpUniGltfPbrMaterialExporter().TryExportMaterial(material, textureExporter, out var dst));

            // emissiveFactor * strength = (0, 2, 0) -> 正規化して factor=(0,1,0), strength=2
            Assert.AreEqual(0f, dst.emissiveFactor[0], 1e-4f);
            Assert.AreEqual(1f, dst.emissiveFactor[1], 1e-4f);
            Assert.AreEqual(0f, dst.emissiveFactor[2], 1e-4f);

            var json = dst.ToJson();
            var parsed = json.ParseAsJson();
            Assert.AreEqual(
                2.0f,
                parsed["extensions"]["KHR_materials_emissive_strength"]["emissiveStrength"].GetSingle(),
                1e-4f);
        }
    }
}
