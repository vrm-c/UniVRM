using UniGLTF;

namespace VRM
{
    public class VRMData
    {
        public GltfData Data { get; }
        public glTF_VRM_extensions VrmExtension { get; }

        public VRMData(GltfData data)
        {
            Data = data;

            if (!glTF_VRM_extensions.TryDeserialize(data.GLTF.extensions, out VRM.glTF_VRM_extensions vrm))
            {
                throw new NotVrm0Exception();
            }
            VrmExtension = vrm;
        }
    }
}
