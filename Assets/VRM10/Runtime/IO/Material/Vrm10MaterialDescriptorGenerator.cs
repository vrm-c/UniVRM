using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public sealed class Vrm10MaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public MaterialDescriptor Get(GltfData data, int i)
        {
            // mtoon
            if (!Vrm10MToonMaterialImporter.TryCreateParam(data, i, out MaterialDescriptor matDesc))
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
