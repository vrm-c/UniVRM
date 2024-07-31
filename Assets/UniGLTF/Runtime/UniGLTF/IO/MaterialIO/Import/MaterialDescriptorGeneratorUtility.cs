namespace UniGLTF
{
    public static class MaterialDescriptorGeneratorUtility
    {
        public static IMaterialDescriptorGenerator GetValidGltfMaterialDescriptorGenerator()
        {
            return GetGltfMaterialDescriptorGenerator(RenderPipelineUtility.GetRenderPipelineType());
        }

        public static IMaterialDescriptorGenerator GetGltfMaterialDescriptorGenerator(RenderPipelineTypes renderPipelineType)
        {
            return renderPipelineType switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => new UrpGltfMaterialDescriptorGenerator(),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInGltfMaterialDescriptorGenerator(),
                _ => new BuiltInGltfMaterialDescriptorGenerator(),
            };
        }
    }
}

