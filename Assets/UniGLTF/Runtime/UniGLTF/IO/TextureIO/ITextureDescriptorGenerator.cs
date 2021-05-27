using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// glTF から Import できる、すべての Texture の生成情報を生成する。
    ///
    /// glTF Texture と Unity Texture の対応関係は N:M である。
    /// </summary>
    public interface ITextureDescriptorGenerator
    {
        TextureDescriptorSet Get();
    }
}
