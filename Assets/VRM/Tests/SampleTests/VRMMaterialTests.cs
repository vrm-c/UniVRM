using NUnit.Framework;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM.Samples
{
    public class VRMMaterialTests
    {
        static UniGLTF.glTFMaterial ExportLoaded(string resourceName)
        {
            var material = Resources.Load<Material>(resourceName);
            var exporter = new BuiltInVrmMaterialExporter();
            var textureExporter = new TextureExporter(new EditorTextureSerializer());
            var exported = exporter.ExportMaterial(material, textureExporter, new GltfExportSettings());

            // parse glTFExtensionExport to glTFExtensionImport
            exported.extensions = exported.extensions.Deserialize();

            return exported;
        }

        [Test]
        public void ExportTest()
        {
            {
                var exported = ExportLoaded("Materials/vrm_unlit_texture");
                Assert.True(UniGLTF.glTF_KHR_materials_unlit.IsEnable(exported));
                Assert.AreEqual("OPAQUE", exported.alphaMode);
            }
            {
                var exported = ExportLoaded("Materials/vrm_unlit_transparent");
                Assert.True(UniGLTF.glTF_KHR_materials_unlit.IsEnable(exported));
                Assert.AreEqual("BLEND", exported.alphaMode);
            }
            {
                var exported = ExportLoaded("Materials/vrm_unlit_cutout");
                Assert.True(UniGLTF.glTF_KHR_materials_unlit.IsEnable(exported));
                Assert.AreEqual("MASK", exported.alphaMode);
            }
            {
                var exported = ExportLoaded("Materials/vrm_unlit_transparent_zwrite");
                Assert.True(UniGLTF.glTF_KHR_materials_unlit.IsEnable(exported));
                Assert.AreEqual("BLEND", exported.alphaMode);
            }
            {
                var exported = ExportLoaded("Materials/mtoon_opaque");
                Assert.True(UniGLTF.glTF_KHR_materials_unlit.IsEnable(exported));
                Assert.AreEqual("OPAQUE", exported.alphaMode);
            }
            {
                var exported = ExportLoaded("Materials/mtoon_transparent");
                Assert.True(UniGLTF.glTF_KHR_materials_unlit.IsEnable(exported));
                Assert.AreEqual("BLEND", exported.alphaMode);
            }
            {
                var exported = ExportLoaded("Materials/mtoon_cutout");
                Assert.True(UniGLTF.glTF_KHR_materials_unlit.IsEnable(exported));
                Assert.AreEqual("MASK", exported.alphaMode);
            }
            {
                var exported = ExportLoaded("Materials/mtoon_culloff");
                Assert.True(UniGLTF.glTF_KHR_materials_unlit.IsEnable(exported));
                Assert.True(exported.doubleSided);
            }
        }
    }
}
