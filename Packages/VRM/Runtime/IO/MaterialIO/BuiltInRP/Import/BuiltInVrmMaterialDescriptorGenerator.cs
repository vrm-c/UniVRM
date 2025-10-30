using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    /// <summary>
    /// A class that generates MaterialDescriptor by considering the VRM 0.X extension included in the glTF data to be imported.
    /// </summary>
    public sealed class BuiltInVrmMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        private readonly glTF_VRM_extensions _vrm;

        public BuiltInGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public BuiltInGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();
        public BuiltInGltfUnlitMaterialImporter UnlitMaterialImporter { get; } = new();
        public BuiltInVrmMToonMaterialImporter MToonMaterialImporter { get; } = new();
        public BuiltInVrmUnlitTransparentZWriteMaterialImporter UnlitTransparentZWriteMaterialImporter { get; } = new();

        public BuiltInVrmMaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            _vrm = vrm;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            // legacy "VRM/UnlitTransparentZWrite"
            if (UnlitTransparentZWriteMaterialImporter.TryCreateParam(data, _vrm, i, out var matDesc))
            {
                return matDesc;
            }

            // mtoon
            if (MToonMaterialImporter.TryCreateParam(data, _vrm, i, out matDesc))
            {
                return matDesc;
            }

            // unlit
            if (UnlitMaterialImporter.TryCreateParam(data, i, out matDesc))
            {
                return matDesc;
            }

            // pbr
            if (PbrMaterialImporter.TryCreateParam(data, i, out matDesc))
            {
                return matDesc;
            }

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
