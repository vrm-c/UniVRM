using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

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
            Debug.LogWarning($"fallback");
            return new MaterialDescriptor(
                GltfMaterialImportUtils.ImportMaterialName(i, null),
                BuiltInGltfPbrMaterialImporter.Shader,
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new Action<Material>[]{});
        }

        public MaterialDescriptor GetGltfDefault()
        {
            return BuiltInGltfDefaultMaterialImporter.CreateParam();
        }
    }
}
