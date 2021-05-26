using System.Collections.Generic;
using UnityEngine;
using VRMShaders;


namespace UniGLTF
{
    /// <summary>
    /// GLTF の MaterialImporter
    /// </summary>
    public sealed class GltfMaterialImporter : IMaterialImporter
    {
        public MaterialImportParam GetMaterialParam(GltfParser parser, int i)
        {
            if (!GltfUnlitMaterial.TryCreateParam(parser, i, out var param))
            {
                if (!GltfPBRMaterial.TryCreateParam(parser, i, out param))
                {
                    // fallback
#if VRM_DEVELOP
                    Debug.LogWarning($"material: {i} out of range. fallback");
#endif
                    return new MaterialImportParam(GetMaterialName(i, null), GltfPBRMaterial.ShaderName);
                }
            }

            return param;
        }

        public static string GetMaterialName(int index, glTFMaterial src)
        {
            if (src != null && !string.IsNullOrEmpty(src.name))
            {
                return src.name;
            }
            return $"material_{index:00}";
        }
    }
}
