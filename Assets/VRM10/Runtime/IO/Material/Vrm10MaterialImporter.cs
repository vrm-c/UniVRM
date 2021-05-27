using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public sealed class Vrm10MaterialImporter : IMaterialImporter
    {
        public MaterialDescriptor GetMaterialParam(GltfParser parser, int i)
        {
            // mtoon
            if (!Vrm10MToonMaterialImporter.TryCreateParam(parser, i, out MaterialDescriptor matDesc))
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
