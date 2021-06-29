using System;
using System.Collections.Generic;
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
            // mtoon
            if (!VRMMToonMaterialImporter.TryCreateParam(data, m_vrm, i, out MaterialDescriptor matDesc))
            {
                // unlit
                if (!GltfUnlitMaterialImporter.TryCreateParam(data, i, out matDesc))
                {
                    // pbr
                    if (!GltfPbrMaterialImporter.TryCreateParam(data, i, out matDesc))
                    {
                        // fallback
#if VRM_DEVELOP
                        Debug.LogWarning($"material: {i} out of range. fallback");
#endif
                        return new MaterialDescriptor(GltfMaterialDescriptorGenerator.GetMaterialName(i, null), GltfPbrMaterialImporter.ShaderName);
                    }
                }
            }
            return matDesc;
        }
    }
}
