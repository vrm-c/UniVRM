using System.Collections.Generic;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// glTF から Import できる Texture の生成情報のリストを生成する。
    ///
    /// glTF Texture と Unity Texture の対応関係は N:M である。
    /// </summary>
    public interface ITextureImporter
    {
        IReadOnlyDictionary<SubAssetKey, TextureImportParam> GetTextureParams(GltfParser parser);
    }
}
