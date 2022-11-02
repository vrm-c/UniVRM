using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// 非対応のシェーダでも空のマテリアルを出力する.
    /// </summary>
    public static class BuiltinRPFallbackMaterialExporter
    {
        public static glTFMaterial ExportMaterial(Material src, ITextureExporter textureExporter)
        {
            var dst = new glTFMaterial
            {
                name = src.name,
                pbrMetallicRoughness = new glTFPbrMetallicRoughness(),
            };

            return dst;
        }
    }
}