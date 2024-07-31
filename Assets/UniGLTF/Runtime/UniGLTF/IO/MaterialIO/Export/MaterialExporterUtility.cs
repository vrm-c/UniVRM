namespace UniGLTF
{
    public static class MaterialExporterUtility
    {
        public static IMaterialExporter GetValidGltfMaterialExporter()
        {
            return RenderPipelineUtility.GetRenderPipelineType() switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => throw new System.NotImplementedException(),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInGltfMaterialExporter(),
                _ => new BuiltInGltfMaterialExporter(),
            };
        }
    }
}