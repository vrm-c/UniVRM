using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public sealed class VRMMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        readonly glTF_VRM_extensions m_vrm;
        public VRMMaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            m_vrm = vrm;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            MaterialDescriptor matDesc;

            // legacy "VRM/UnlitTransparentZWrite"
            if (VRMUnlitTransparentZWriteMaterialImporter.TryCreateParam(data, m_vrm, i, out matDesc))
            {
                return matDesc;
            }

            // mtoon
            if (VRMMToonMaterialImporter.TryCreateParam(data, m_vrm, i, out matDesc))
            {
                return matDesc;
            }

            // unlit
            if (GltfUnlitMaterialImporter.TryCreateParam(data, i, out matDesc))
            {
                return matDesc;
            }

            // pbr
            if (GltfPbrMaterialImporter.TryCreateParam(data, i, out matDesc))
            {
                return matDesc;
            }

            // fallback
            Debug.LogWarning($"fallback");
            return new MaterialDescriptor(GltfMaterialDescriptorGenerator.GetMaterialName(i, null), GltfPbrMaterialImporter.ShaderName);
        }
    }
}
