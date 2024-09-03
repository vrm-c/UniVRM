using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public sealed class UrpVrmMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        private readonly glTF_VRM_extensions _vrm;

        public UrpGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public UrpGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();

        public UrpVrmMaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            _vrm = vrm;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            // mtoon URP "MToon" shader is not ready. import fallback to unlit
            // unlit "UniUnlit" work in URP
            if (BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out var matDesc)) return matDesc;
            // pbr "Standard" to "Universal Render Pipeline/Lit" 
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
