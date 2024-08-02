using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public sealed class BuiltInVrmMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        private readonly glTF_VRM_extensions _vrm;

        public BuiltInVrmMaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            _vrm = vrm;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            // legacy "VRM/UnlitTransparentZWrite"
            if (BuiltInVrmUnlitTransparentZWriteMaterialImporter.TryCreateParam(data, _vrm, i, out var matDesc))
            {
                return matDesc;
            }

            // mtoon
            if (BuiltInVrmMToonMaterialImporter.TryCreateParam(data, _vrm, i, out matDesc))
            {
                return matDesc;
            }

            // unlit
            if (BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out matDesc))
            {
                return matDesc;
            }

            // pbr
            if (BuiltInGltfPbrMaterialImporter.TryCreateParam(data, i, out matDesc))
            {
                return matDesc;
            }

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
