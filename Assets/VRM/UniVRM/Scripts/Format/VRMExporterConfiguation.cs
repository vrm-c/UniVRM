namespace VRM
{
    public struct VRMExporterConfiguration
    {
        public bool UseSparseAccessorForBlendShape;
        public bool ExportOnlyBlendShapePosition;

        public static VRMExporterConfiguration Default => new VRMExporterConfiguration
        {
            UseSparseAccessorForBlendShape = true,
            ExportOnlyBlendShapePosition = false,
        };
    }
}
