namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreExportShaders
    {
        const string GLTF_FOLDER = "GLTF";

#pragma warning disable 414
        [PreExportShaders]
        static SupportedShader[] SupportedShaders = new SupportedShader[]
        {
            new SupportedShader(GLTF_FOLDER, "Standard"),
            new SupportedShader(GLTF_FOLDER, "Unlit/Color"),
            new SupportedShader(GLTF_FOLDER, "Unlit/Texture"),
            new SupportedShader(GLTF_FOLDER, "Unlit/Transparent"),
            new SupportedShader(GLTF_FOLDER, "Unlit/Transparent Cutout"),
            new SupportedShader(GLTF_FOLDER, "UniGLTF/UniUnlit"),
        };
#pragma warning restore 414
    }
}
