using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// A class that generates MaterialDescriptor by considering the VRM 0.X extension included in the glTF data to be imported.
    /// </summary>
    public sealed class UrpVrmMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        private readonly glTF_VRM_extensions _vrm;

        public UrpGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public UrpGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();
        public BuiltInGltfUnlitMaterialImporter UnlitMaterialImporter { get; } = new();

        public UrpVrmMaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            _vrm = vrm;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            // mtoon URP "MToon" shader is not ready. import fallback to unlit
            // unlit "UniUnlit" work in URP
            if (UnlitMaterialImporter.TryCreateParam(data, i, out var matDesc)) return matDesc;
            // pbr "Standard" to "Universal Render Pipeline/Lit" 
            if (PbrMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;

            // NOTE: Fallback to default material
            if (Symbols.VRM_DEVELOP)
            {
                UniGLTFLogger.Warning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null) => DefaultMaterialImporter.CreateParam(materialName);
    }
}
