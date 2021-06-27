using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// 指定の index の glTFMaterial から Import できる Material の生成情報を生成する。
    /// glTFMaterial と Unity Material は 1:1 対応する。
    /// </summary>
    public interface IMaterialDescriptorGenerator
    {
        MaterialDescriptor Get(GltfData data, int i);
    }
}
