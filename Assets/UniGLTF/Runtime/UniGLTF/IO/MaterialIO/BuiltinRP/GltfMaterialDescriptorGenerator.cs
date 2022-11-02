using System;
using System.Collections.Generic;
using UnityEngine;
using VRMShaders;


namespace UniGLTF
{
    /// <summary>
    /// GLTF の MaterialImporter
    /// </summary>
    public sealed class GltfMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public MaterialDescriptor Get(GltfData data, int i)
        {
            if (GltfUnlitMaterialImporter.TryCreateParam(data, i, out var param)) return param;
            if (GltfPbrMaterialImporter.TryCreateParam(data, i, out param)) return param;
            // fallback
#if VRM_DEVELOP
            Debug.LogWarning($"material: {i} out of range. fallback");
#endif
            return new MaterialDescriptor(
                GetMaterialName(i, null),
                GltfPbrMaterialImporter.ShaderName, 
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new Action<Material>[]{});

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
