using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// GLTF の MaterialImporter
    /// </summary>
    public sealed class BuiltInGltfMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public MaterialDescriptor Get(GltfData data, int i)
        {
            if (BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out var param)) return param;
            if (BuiltInGltfPbrMaterialImporter.TryCreateParam(data, i, out param)) return param;

            // fallback
            if (Symbols.VRM_DEVELOP)
            {
                Debug.LogWarning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null) => BuiltInGltfDefaultMaterialImporter.CreateParam(materialName);
    }
}
