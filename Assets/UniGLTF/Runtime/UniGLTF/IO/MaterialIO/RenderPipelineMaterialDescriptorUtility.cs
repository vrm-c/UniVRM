using UniGLTF;
using UnityEngine.Rendering;

public class RenderPipelineMaterialDescriptorGeneratorUtility
{
    protected static RenderPipelineTypes GetRenderPipelineType()
    {
        RenderPipeline currentPipeline = RenderPipelineManager.currentPipeline;

        if (currentPipeline == null)
        {
            return RenderPipelineTypes.Unknown;
        }
        else if (currentPipeline.GetType().Name.Contains("HDRenderPipeline"))
        {
            return RenderPipelineTypes.HighDefinitionRenderPipeline;
        }
        else if (currentPipeline.GetType().Name.Contains("UniversalRenderPipeline"))
        {
            return RenderPipelineTypes.UniversalRenderPipeline;
        }
        else if (currentPipeline.GetType().Name.Contains("RenderPipeline"))
        {
            return RenderPipelineTypes.BuiltinRenderPipeline;
        }
        else
        {
            return RenderPipelineTypes.Unknown;
        }
    }
    
    public static IMaterialDescriptorGenerator GetValidGLTFMaterialDescriptorGenerator()
    {
        switch (GetRenderPipelineType())
        {
            case RenderPipelineTypes.UniversalRenderPipeline:
                return new UrpGltfMaterialDescriptorGenerator();
            case RenderPipelineTypes.BuiltinRenderPipeline:
                return new BuiltInGltfMaterialDescriptorGenerator();
        }

        return null;
    }
}

