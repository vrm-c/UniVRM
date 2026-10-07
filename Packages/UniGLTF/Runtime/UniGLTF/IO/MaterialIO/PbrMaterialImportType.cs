namespace UniGLTF
{
    /// <summary>
    /// glTF PBR マテリアルの import 実装の選択。
    ///
    /// RenderPipeline や具体的なシェーダ実装 (ShaderGraph 等) に依存しない名前とする。
    /// </summary>
    public enum PbrMaterialImportType
    {
        /// <summary>
        /// 既定。Unity 標準の PBR マテリアル実装を用いる。
        /// (Built-In: Unity "Standard" / URP: "Universal Render Pipeline/Lit")
        /// metallicRoughness/occlusion は import 時にテクスチャ変換される。
        /// </summary>
        UnityStandard = 0,

        /// <summary>
        /// glTF 仕様に最適化 PBR マテリアル実装。
        /// glTF の ORM を無変換で取り込み、単一シェーダで複数 RenderPipeline をカバーする。
        /// (現状の実装は glTF 準拠 PBR ShaderGraph "UniGltfPbr")
        /// </summary>
        GltfCompatible = 1,
    }
}
