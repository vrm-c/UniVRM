using UniGLTF;

namespace UniVRM10.VRM10Viewer
{
    class UnlitMaterialImporter : IMaterialImporter
    {
        public BuiltInGltfUnlitMaterialImporter GltfUnlitMaterialImporter { get; } = new();

        bool IMaterialImporter.TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            return GltfUnlitMaterialImporter.TryCreateParam(data, i, out matDesc);
        }
    }
}