using UniGLTF;
using VRM;

namespace UniVRM
{
    public class VrmRenderPipelineMaterialDescriptorGeneratorDescriptorUtility : RenderPipelineMaterialDescriptorGeneratorUtility
    {
        public static IMaterialDescriptorGenerator GetValidVrm10MaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            switch (GetRenderPipelineType())
            {
                case RenderPipelineTypes.UniversalRenderPipeline:
                    return new UrpVrmMaterialDescriptorGenerator(vrm);
                case RenderPipelineTypes.BuiltinRenderPipeline:
                    return new BuiltInVrmMaterialDescriptorGenerator(vrm);
            }

            return null;
        }
    }
}