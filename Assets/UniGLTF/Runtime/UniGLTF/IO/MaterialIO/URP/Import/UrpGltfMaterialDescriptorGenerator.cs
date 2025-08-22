using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// A class that generates MaterialDescriptor by considering the extensions included in the glTF data to be imported.
    /// </summary>
    public sealed class UrpGltfMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public UrpGltfPbrMaterialImporter PbrMaterialImporter { get; } = new();
        public UrpGltfDefaultMaterialImporter DefaultMaterialImporter { get; } = new();
        public BuiltInGltfUnlitMaterialImporter UnlitMaterialImporter { get; } = new();

        public MaterialDescriptor Get(GltfData data, int i)
        {
            if (UnlitMaterialImporter.TryCreateParam(data, i, out var param)) return param;
            if (PbrMaterialImporter.TryCreateParam(data, i, out param)) return param;

            // NOTE: Fallback to default material
            if (Symbols.VRM_DEVELOP)
            {
                UniGLTFLogger.Warning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null) => DefaultMaterialImporter.CreateParam(materialName);
    }
}