using VRMShaders;

namespace UniGLTF
{
    /// <summary>
    /// 指定の index の glTFMaterial から Import できる Material の生成情報を生成する。
    /// Material の Import は glTFMaterial と 1:1 対応する。
    /// </summary>
    public interface IMaterialImporter
    {
        MaterialImportParam GetMaterialParam(GltfParser parser, int i);
    }
}
