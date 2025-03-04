using UniGLTF;

namespace UniVRM10.VRM10Viewer
{
    public class UrpPbrMaterialImporter : IMaterialImporter
    {
        public UrpGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();

        public bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            if (PbrMaterialImporter.TryCreateParam(data, i, out matDesc))
            {
                return true;
            }
            return false;
        }
    }
}