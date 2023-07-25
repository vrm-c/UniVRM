using UniGLTF;

namespace UniVRM10
{
    public class Vrm10RenderPipelineMaterialDescriptorGeneratorDescriptorUtility : RenderPipelineMaterialDescriptorGeneratorUtility
    {
        public static IMaterialDescriptorGenerator GetValidVrm10MaterialDescriptorGenerator()
        {
            switch (GetRenderPipelineType())
            {
                case RenderPipelineTypes.UniversalRenderPipeline:
                    return new UrpVrm10MaterialDescriptorGenerator();
                case RenderPipelineTypes.BuiltinRenderPipeline:
                    return new BuiltInVrm10MaterialDescriptorGenerator();
            }

            return null;
        }
    }
}