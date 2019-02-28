namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreExportShaders
    {
        const string VRM_TARGET_FOLDER = "VRM";
        [PreExportShaders]
        public static SupportedShader[] VRMSupportedShaders = new SupportedShader[]
        {
            new SupportedShader(VRM_TARGET_FOLDER, "VRM/MToon"),
            new SupportedShader(VRM_TARGET_FOLDER, "VRM/UnlitTexture"),
            new SupportedShader(VRM_TARGET_FOLDER, "VRM/UnlitCutout"),
            new SupportedShader(VRM_TARGET_FOLDER, "VRM/UnlitTransparent"),
            new SupportedShader(VRM_TARGET_FOLDER, "VRM/UnlitTransparentZWrite"),
        };
    }
}
