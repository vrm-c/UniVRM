using UniGLTF;
using UnityEngine;

namespace VRM
{
    public static class TestVrm0X
    {
        public static RuntimeGltfInstance LoadBytesAsBuiltInRP(byte[] bytes)
        {
            return VrmUtility.LoadBytesAsync(
                "",
                bytes,
                awaitCaller: new ImmediateCaller(),
                materialGeneratorCallback: x => new BuiltInVrmMaterialDescriptorGenerator(x)
            ).Result;
        }

        public static RuntimeGltfInstance LoadPathAsBuiltInRP(string path)
        {
            return VrmUtility.LoadAsync(
                path,
                awaitCaller: new ImmediateCaller(),
                materialGeneratorCallback: x => new BuiltInVrmMaterialDescriptorGenerator(x)
            ).Result;
        }

        public static byte[] ExportAsBuiltInRP(GameObject gameObject, VRMExportSettings exportSettings)
        {
            return VRMEditorExporter.Export(gameObject, null, exportSettings, new BuiltInVrmMaterialExporter());
        }
    }
}