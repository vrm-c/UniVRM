using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// GLTF の MaterialImporter
    /// </summary>
    public sealed class UrpGltfMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public UrpGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public UrpGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();

        public MaterialDescriptor Get(GltfData data, int i)
        {
            if (BuiltInGltfUnlitMaterialImporter.TryCreateParam(data, i, out var param)) return param;
            if (PbrMaterialImporter.TryCreateParam(data, i, out param)) return param;

            // NOTE: Fallback to default material
            if (Symbols.VRM_DEVELOP)
            {
                Debug.LogWarning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null) => DefaultMaterialImporter.CreateParam(materialName);
    }
}