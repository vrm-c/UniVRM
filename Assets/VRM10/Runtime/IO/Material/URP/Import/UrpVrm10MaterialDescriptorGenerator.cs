using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    public sealed class UrpVrm10MaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public UrpGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public UrpGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();

        public MaterialDescriptor Get(GltfData data, int i)
        {
            // mtoon
            if (UrpVrm10MToonMaterialImporter.TryCreateParam(data, i, out var matDesc)) return matDesc;
            // unlit
            if (BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;
            // pbr
            if (PbrMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;

            // NOTE: Fallback to default material
            if (Symbols.VRM_DEVELOP)
            {
                Debug.LogWarning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null) => DefaultMaterialImporter.CreateParam(materialName);
    }
}
