using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    public static class TestVrm10
    {
        public static Vrm10Instance LoadBytesAsBuiltInRP(byte[] bytes, bool canLoadVrm0X = true)
        {
            return Vrm10.LoadBytesAsync(
                bytes,
                canLoadVrm0X: canLoadVrm0X,
                awaitCaller: new ImmediateCaller(),
                materialGenerator: new BuiltInVrm10MaterialDescriptorGenerator()
            ).Result;
        }

        public static Vrm10Instance LoadPathAsBuiltInRP(string path, bool canLoadVrm0X = true)
        {
            return Vrm10.LoadPathAsync(
                path,
                canLoadVrm0X: canLoadVrm0X,
                awaitCaller: new ImmediateCaller(),
                materialGenerator: new BuiltInVrm10MaterialDescriptorGenerator()
            ).Result;
        }

        public static byte[] ExportAsBuiltInRP(GameObject gameObject)
        {
            return Vrm10Exporter.Export(
                gameObject,
                materialExporter: new BuiltInVrm10MaterialExporter(),
                textureSerializer: new EditorTextureSerializer()
            );
        }
    }
}