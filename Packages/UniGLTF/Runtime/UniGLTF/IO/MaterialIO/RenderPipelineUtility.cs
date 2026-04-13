using UnityEngine.Rendering;

namespace UniGLTF
{
    public static class RenderPipelineUtility
    {
        public static RenderPipelineTypes GetRenderPipelineType()
        {
            RenderPipelineAsset currentRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;

            if (currentRenderPipelineAsset == null)
            {
                return RenderPipelineTypes.BuiltinRenderPipeline;
            }

            if (currentRenderPipelineAsset.GetType().Name.Contains("HDRenderPipeline"))
            {
                return RenderPipelineTypes.HighDefinitionRenderPipeline;
            }

            if (currentRenderPipelineAsset.GetType().Name.Contains("UniversalRenderPipeline"))
            {
                return RenderPipelineTypes.UniversalRenderPipeline;
            }

            return RenderPipelineTypes.Unknown;
        }
    }
}
