using UniGLTF;

namespace VRM
{
    public static class VrmMaterialExporterUtility
    {
        public static IMaterialExporter GetValidVrmMaterialExporter()
        {
            return RenderPipelineUtility.GetRenderPipelineType() switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => throw new System.NotImplementedException(),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInVrmMaterialExporter(),
                _ => new BuiltInVrmMaterialExporter(),
            };
        }
    }
}