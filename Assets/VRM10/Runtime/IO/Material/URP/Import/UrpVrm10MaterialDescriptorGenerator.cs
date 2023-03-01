using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public sealed class UrpVrm10MaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public MaterialDescriptor Get(GltfData data, int i)
        {
            // unlit
            if (BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out MaterialDescriptor matDesc)) return matDesc;
            // pbr
            if (UrpGltfPbrMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;

            // fallback
            Debug.LogWarning($"material: {i} out of range. fallback");
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
