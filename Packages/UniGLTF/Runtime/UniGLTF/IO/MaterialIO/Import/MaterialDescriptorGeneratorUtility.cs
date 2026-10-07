namespace UniGLTF
{
    public static class MaterialDescriptorGeneratorUtility
    {
        public static IMaterialDescriptorGenerator GetValidGltfMaterialDescriptorGenerator(PbrMaterialImportType pbrMaterialImportType = PbrMaterialImportType.UnityStandard)
        {
            return GetGltfMaterialDescriptorGenerator(RenderPipelineUtility.GetRenderPipelineType(), pbrMaterialImportType);
        }

        /// <summary>
        /// <paramref name="pbrMaterialImportType"/> が GltfCompatible の場合、renderPipelineType によらず
        /// RenderPipeline 非依存の InvariantGltfMaterialDescriptorGenerator を返す (HDRP Target は無い)。
        /// </summary>
        public static IMaterialDescriptorGenerator GetGltfMaterialDescriptorGenerator(RenderPipelineTypes renderPipelineType, PbrMaterialImportType pbrMaterialImportType = PbrMaterialImportType.UnityStandard)
        {
            if (pbrMaterialImportType == PbrMaterialImportType.GltfCompatible)
            {
                // NOTE: glTF 互換 PBR (ShaderGraph) は 1 つのシェーダで Built-In / URP 両対応のため
                //       RenderPipeline による分岐を行わない。
                return new InvariantGltfMaterialDescriptorGenerator();
            }

            return renderPipelineType switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => new UrpGltfMaterialDescriptorGenerator(),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInGltfMaterialDescriptorGenerator(),
                _ => new BuiltInGltfMaterialDescriptorGenerator(),
            };
        }
    }
}

