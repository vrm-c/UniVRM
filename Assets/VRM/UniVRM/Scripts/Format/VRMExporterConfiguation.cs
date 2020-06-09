namespace VRM
{
    public struct VRMExporterConfiguration
    {
        public bool UseSparseAccessorForBlendShape;
        public bool ExportOnlyBlendShapePosition;
        public bool RemoveVertexColor;

        public static VRMExporterConfiguration Default => new VRMExporterConfiguration
        {
            UseSparseAccessorForBlendShape = true,
            ExportOnlyBlendShapePosition = false,
            RemoveVertexColor = false,
        };
    }
}
