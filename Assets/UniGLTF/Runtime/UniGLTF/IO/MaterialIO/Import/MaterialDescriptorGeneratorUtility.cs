namespace UniGLTF
{
    public static class MaterialDescriptorGeneratorUtility
    {
        public static IMaterialDescriptorGenerator GetValidGltfMaterialDescriptorGenerator()
        {
            return RenderPipelineUtility.GetRenderPipelineType() switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => new UrpGltfMaterialDescriptorGenerator(),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInGltfMaterialDescriptorGenerator(),
                _ => new BuiltInGltfMaterialDescriptorGenerator(),
            };
        }
    }
}

