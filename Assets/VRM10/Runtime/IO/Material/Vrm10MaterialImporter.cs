using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public sealed class Vrm10MaterialImporter : IMaterialImporter
    {
        public MaterialImportParam GetMaterialParam(GltfParser parser, int i)
        {
            // mtoon
            if (!Vrm10MToonMaterialImporter.TryCreateParam(parser, i, out MaterialImportParam param))
            {
                // unlit
                if (!GltfUnlitMaterialImporter.TryCreateParam(parser, i, out param))
                {
                    // pbr
                    if (!GltfPbrMaterialImporter.TryCreateParam(parser, i, out param))
                    {
                        // fallback
#if VRM_DEVELOP
                        Debug.LogWarning($"material: {i} out of range. fallback");
#endif
                        return new MaterialImportParam(GltfMaterialImporter.GetMaterialName(i, null), GltfPbrMaterialImporter.ShaderName);
                    }
                }
            }
            return param;
        }

    }
}
