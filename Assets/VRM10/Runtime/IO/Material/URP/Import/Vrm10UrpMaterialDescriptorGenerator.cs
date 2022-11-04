using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public sealed class Vrm10UrpMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public MaterialDescriptor Get(GltfData data, int i)
        {
            // unlit
            if (GltfUnlitMaterialImporter.TryCreateParam(data, i, out MaterialDescriptor matDesc)) return matDesc;
            // pbr
            if (GltfPbrUrpMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;
            // fallback
            Debug.LogWarning($"material: {i} out of range. fallback");
            return new MaterialDescriptor(
                GltfMaterialDescriptorGenerator.GetMaterialName(i, null),
                GltfPbrMaterialImporter.ShaderName,
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new Action<Material>[]{});
        }

    }
}
