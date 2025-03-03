namespace UniGLTF
{
    /// <summary>
    /// generate a unity Material from a glTFMaterial.
    /// </summary>
    public interface IMaterialDescriptorGenerator
    {
        /// <summary>
        /// Generate the MaterialDescriptor generated from the index i.
        /// </summary>
        MaterialDescriptor Get(GltfData data, int i);

        /// <summary>
        /// Generate the MaterialDescriptor for the non-specified glTF material.
        /// </summary>
        MaterialDescriptor GetGltfDefault(string materialName = null);
    }
}
