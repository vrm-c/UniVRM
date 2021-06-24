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
        /// 指定の Texture を、 sRGB 色空間の値を持つ Texture に出力するように指示する。
        /// </summary>
        int RegisterExportingAsSRgb(Texture src, bool needsAlpha);

        /// <summary>
        /// 指定の Texture を、 Linear の値を持つ Texture に出力するように指示する。
        /// </summary>
        int RegisterExportingAsLinear(Texture src, bool needsAlpha);

        /// <summary>
        /// Unity Standard Shader の Metallic, Roughness, Occlusion 情報を、 glTF 仕様に準拠した 1 枚の合成テクスチャとして出力するように指示する。
        /// </summary>
        int RegisterExportingAsCombinedGltfPbrParameterTextureFromUnityStandardTextures(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture);

        /// <summary>
        /// 指定の Texture を、glTF 仕様に準拠した Normal Texture に出力するように指示する。
        /// </summary>
        int RegisterExportingAsNormal(Texture src);
    }
}
