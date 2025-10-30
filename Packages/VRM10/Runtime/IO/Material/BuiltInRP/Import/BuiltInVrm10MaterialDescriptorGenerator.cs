using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// A class that generates MaterialDescriptor by considering the VRM 1.0 extension included in the glTF data to be imported.
    /// </summary>
    public sealed class BuiltInVrm10MaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public BuiltInGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public BuiltInGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();
        public BuiltInGltfUnlitMaterialImporter UnlitMaterialImporter { get; } = new();
        public BuiltInVrm10MToonMaterialImporter MToonMaterialImporter { get; } = new();

        public MaterialDescriptor Get(GltfData data, int i)
        {
            // mtoon
            if (MToonMaterialImporter.TryCreateParam(data, i, out MaterialDescriptor matDesc)) return matDesc;
            // unlit
            if (UnlitMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;
            // pbr
            if (PbrMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;

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
