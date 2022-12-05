using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// 指定の index の glTFMaterial から Import できる Material の生成情報を生成する。
    /// glTFMaterial と Unity Material は 1:1 対応する。
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
        MaterialDescriptor GetGltfDefault();
    }
}
