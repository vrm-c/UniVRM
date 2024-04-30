using UniGLTF;

namespace VRM
{
    public static class VrmMaterialDescriptorGeneratorUtility
    {
        public static IMaterialDescriptorGenerator GetValidVrmMaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            return MaterialDescriptorGeneratorUtility.GetRenderPipelineType() switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => new UrpVrmMaterialDescriptorGenerator(vrm),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInVrmMaterialDescriptorGenerator(vrm),
                _ => new BuiltInVrmMaterialDescriptorGenerator(vrm),
            };
        }
    }
}