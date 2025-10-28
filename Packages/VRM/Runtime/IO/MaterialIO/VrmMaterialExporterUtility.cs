using UniGLTF;

namespace VRM
{
    public static class VrmMaterialExporterUtility
    {
        public static IMaterialExporter GetValidVrmMaterialExporter()
        {
            return RenderPipelineUtility.GetRenderPipelineType() switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => throw new System.NotImplementedException("URP exporter not implemented"),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInVrmMaterialExporter(),
                _ => new BuiltInVrmMaterialExporter(),
            };
        }
    }
}