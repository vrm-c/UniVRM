using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public sealed class VRMMaterialImporter : IMaterialImporter
    {
        readonly glTF_VRM_extensions m_vrm;
        public VRMMaterialImporter(glTF_VRM_extensions vrm)
        {
            m_vrm = vrm;
        }

        public MaterialDescriptor GetMaterialParam(GltfParser parser, int i)
        {
            // mtoon
            if (!VRMMToonMaterialImporter.TryCreateParam(parser, m_vrm, i, out MaterialDescriptor matDesc))
            {
                // unlit
                if (!GltfUnlitMaterialImporter.TryCreateParam(parser, i, out matDesc))
                {
                    // pbr
                    if (!GltfPbrMaterialImporter.TryCreateParam(parser, i, out matDesc))
                    {
                        // fallback
#if VRM_DEVELOP
                        Debug.LogWarning($"material: {i} out of range. fallback");
#endif
                        return new MaterialDescriptor(GltfMaterialImporter.GetMaterialName(i, null), GltfPbrMaterialImporter.ShaderName);
                    }
                }
            }
            return matDesc;
        }
    }
}
