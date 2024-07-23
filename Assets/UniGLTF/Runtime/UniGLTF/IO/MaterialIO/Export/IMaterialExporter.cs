using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// 指定の Unity Material から glTFMaterial を生成する。
    /// glTFMaterial と Unity Material は 1:1 対応する。
    /// </summary>
    public interface IMaterialExporter
    {
        glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter, GltfExportSettings settings);
    }
}
