using System;
using System.Collections.Generic;
using UnityEngine;
using VRMShaders;


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
