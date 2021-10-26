using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public sealed class VRMUrpMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        readonly glTF_VRM_extensions m_vrm;
        public VRMUrpMaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            m_vrm = vrm;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            // mtoon URP "MToon" shader is not ready. import fallback to unlit
            // unlit "UniUnlit" work in URP
            if (GltfUnlitMaterialImporter.TryCreateParam(data, i, out var matDesc)) return matDesc;
            // pbr "Standard" to "Universal Render Pipeline/Lit" 
            if (GltfPbrUrpMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;
            // fallback
#if VRM_DEVELOP
            Debug.LogWarning($"material: {i} out of range. fallback");
#endif
            return new MaterialDescriptor(
                GltfMaterialDescriptorGenerator.GetMaterialName(i, null),
                GltfPbrMaterialImporter.ShaderName,
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new Action<Material>[]{});
        }
    }
}
