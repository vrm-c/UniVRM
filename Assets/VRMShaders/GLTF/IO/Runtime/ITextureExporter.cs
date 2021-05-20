using System.Collections.Generic;
using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// Texture を用途別に変換の要不要を判断して gltf.textures の index に対応させる機能。
    ///
    /// glTF 拡張で Texture の用途を増やす必要がある場合は、この interface を継承して実装すればよい。
    /// </summary>
    public interface ITextureExporter
    {
        /// <summary>
        /// Export する Texture2D のリスト。これが gltf.textures になる
        /// </summary>
        IReadOnlyList<(Texture2D, UniGLTF.ColorSpace)> Exported { get; }

        int ExportSRGB(Texture src);
        int ExportLinear(Texture src);
        int ExportMetallicSmoothnessOcclusion(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture);
        int ExportNormal(Texture src);
    }
}
