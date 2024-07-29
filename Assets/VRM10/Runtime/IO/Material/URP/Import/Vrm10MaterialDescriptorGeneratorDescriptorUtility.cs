using UniGLTF;

namespace UniVRM10
{
    public static class Vrm10MaterialDescriptorGeneratorDescriptorUtility
    {
        public static IMaterialDescriptorGenerator GetValidVrm10MaterialDescriptorGenerator()
        {
            return RenderPipelineUtility.GetRenderPipelineType() switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => new UrpVrm10MaterialDescriptorGenerator(),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInVrm10MaterialDescriptorGenerator(),
                _ => new BuiltInVrm10MaterialDescriptorGenerator(),
            };
        }
    }
}