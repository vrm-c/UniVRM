using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// 指定の Unity Material から glTFMaterial を生成する。
    /// </summary>
    public interface IMaterialExporter
    {
        glTFMaterial ExportMaterial(Material m, ITextureExporter textureExporter);
    }
}
