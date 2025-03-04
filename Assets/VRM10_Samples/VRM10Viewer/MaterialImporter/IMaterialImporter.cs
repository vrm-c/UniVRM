using UniGLTF;

namespace UniVRM10.VRM10Viewer
{
    public interface IMaterialImporter
    {
        bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc);
    }
}