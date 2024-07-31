using UniGLTF;

namespace UniVRM10
{
    public static class Vrm10MaterialDescriptorGeneratorUtility
    {
        public static IMaterialDescriptorGenerator GetValidVrm10MaterialDescriptorGenerator()
        {
            return GetVrm10MaterialDescriptorGenerator(RenderPipelineUtility.GetRenderPipelineType());
        }

        public static IMaterialDescriptorGenerator GetVrm10MaterialDescriptorGenerator(RenderPipelineTypes renderPipelineType)
        {
            return renderPipelineType switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => new UrpVrm10MaterialDescriptorGenerator(),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInVrm10MaterialDescriptorGenerator(),
                _ => new BuiltInVrm10MaterialDescriptorGenerator(),
            };
        }
    }
}