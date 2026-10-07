using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// glTF 準拠 PBR ShaderGraph (UniGltfPbr) を用いる IMaterialDescriptorGenerator。
    ///
    /// オプトイン用。ImporterContext の materialGenerator 引数に渡して使う。
    /// RenderPipeline 非依存 (1 つの ShaderGraph が Built-In / URP の両 Target を持つ) なので
    /// 既定の RenderPipeline 分岐 (MaterialDescriptorGeneratorUtility) は経由しない。
    ///
    /// 分岐順は既存 generator と同じ unlit -> pbr -> default。
    /// unlit は既存の BuiltInGltfUnlitMaterialImporter (UniGLTF/UniUnlit) を再利用する。
    /// </summary>
    public sealed class InvariantGltfMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        public InvariantRpUniGltfPbrMaterialImporter PbrMaterialImporter { get; }
        public BuiltInGltfUnlitMaterialImporter UnlitMaterialImporter { get; } = new();

        public InvariantGltfMaterialDescriptorGenerator(Shader pbrShader = null)
        {
            PbrMaterialImporter = new InvariantRpUniGltfPbrMaterialImporter(pbrShader);
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            if (UnlitMaterialImporter.TryCreateParam(data, i, out var param)) return param;
            if (PbrMaterialImporter.TryCreateParam(data, i, out param)) return param;

            // NOTE: Fallback to default material
            if (Symbols.VRM_DEVELOP)
            {
                UniGLTFLogger.Warning($"material: {i} out of range. fallback");
            }
            return GetGltfDefault(GltfMaterialImportUtils.ImportMaterialName(i, null));
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null)
            => PbrMaterialImporter.CreateDefaultParam(materialName);
    }
}
