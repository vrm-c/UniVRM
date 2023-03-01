using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using VRMShaders;


namespace UniGLTF
{
    /// <summary>
    /// GLTF の MaterialImporter
    /// </summary>
    public sealed class UrpGltfMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public MaterialDescriptor Get(GltfData data, int i)
        {
            if (BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out var param)) return param;
            if (UrpGltfPbrMaterialImporter.TryCreateParam(data, i, out param)) return param;
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
                new Collection<Action<Material>>());
        }

        public MaterialDescriptor GetGltfDefault()
        {
            return UrpGltfDefaultMaterialImporter.CreateParam();
        }
    }
}