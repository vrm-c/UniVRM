using UnityEngine.Rendering;

namespace UniGLTF
{
    public static class RenderPipelineUtility
    {
        public static RenderPipelineTypes GetRenderPipelineType()
        {
            RenderPipeline currentPipeline = RenderPipelineManager.currentPipeline;

            if (currentPipeline == null)
            {
                return RenderPipelineTypes.BuiltinRenderPipeline;
            }

            if (currentPipeline.GetType().Name.Contains("HDRenderPipeline"))
            {
                return RenderPipelineTypes.HighDefinitionRenderPipeline;
            }

            if (currentPipeline.GetType().Name.Contains("UniversalRenderPipeline"))
            {
                return RenderPipelineTypes.UniversalRenderPipeline;
            }

            return RenderPipelineTypes.Unknown;
        }
    }
}