using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// A class that generates MaterialDescriptor by considering the extensions included in the glTF data to be imported.
    /// </summary>
    public sealed class BuiltInGltfMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public BuiltInGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public BuiltInGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();
        public BuiltInGltfUnlitMaterialImporter UnlitMaterialImporter { get; } = new();

        public MaterialDescriptor Get(GltfData data, int i)
        {
            if (UnlitMaterialImporter.TryCreateParam(data, i, out var param)) return param;
            if (PbrMaterialImporter.TryCreateParam(data, i, out param)) return param;

            // fallback
            if (Symbols.VRM_DEVELOP)
            {
                UniGLTFLogger.Warning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null) => DefaultMaterialImporter.CreateParam(materialName);
    }
}
