using UniGLTF;

namespace UniVRM10.VRM10Viewer
{
    class UnlitMaterialImporter : IMaterialImporter
    {
        bool IMaterialImporter.TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            return BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out matDesc);
        }
    }
}