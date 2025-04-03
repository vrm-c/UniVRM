using UniGLTF;

namespace UniVRM10.VRM10Viewer
{
    /// <summary>
    /// VRM10/Universal Render Pipeline/MToon10
    /// </summary>
    public class UrpMToonMaterialImporter : IMaterialImporter
    {
        public bool TryCreateParam(GltfData data, int i, out MaterialDescriptor matDesc)
        {
            if (UrpVrm10MToonMaterialImporter.TryCreateParam(data, i, out matDesc))
            {
                return true;
            }
            return false;
        }
    }
}