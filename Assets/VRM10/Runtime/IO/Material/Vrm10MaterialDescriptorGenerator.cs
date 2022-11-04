using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    public sealed class Vrm10MaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public MaterialDescriptor Get(GltfData data, int i)
        {
            // mtoon
            if (Vrm10MToonMaterialImporter.TryCreateParam(data, i, out MaterialDescriptor matDesc)) return matDesc;
            // unlit
            if (GltfUnlitMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;
            // pbr
            if (GltfPbrMaterialImporter.TryCreateParam(data, i, out matDesc)) return matDesc;
            // fallback
            if (Symbols.VRM_DEVELOP)
            {
                Debug.LogWarning($"material: {i} out of range. fallback");
            }
            return new MaterialDescriptor(
                GltfMaterialDescriptorGenerator.GetMaterialName(i, null), GltfPbrMaterialImporter.ShaderName, 
                null, 
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new Action<Material>[]{});
        }

    }
}
