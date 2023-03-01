using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public sealed class UrpVrmMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        private readonly glTF_VRM_extensions _vrm;

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
            if (UrpGltfPbrMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;
            // fallback
            if (Symbols.VRM_DEVELOP)
            {
                Debug.LogWarning($"material: {i} out of range. fallback");
            }
            return new MaterialDescriptor(
                GltfMaterialImportUtils.ImportMaterialName(i, null),
                UrpGltfPbrMaterialImporter.Shader,
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new Action<Material>[]{});
        }

        public MaterialDescriptor GetGltfDefault()
        {
            return UrpGltfDefaultMaterialImporter.CreateParam();
        }
    }
}
