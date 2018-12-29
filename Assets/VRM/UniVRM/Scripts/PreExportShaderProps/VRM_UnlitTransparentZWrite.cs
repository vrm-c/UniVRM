using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreShaderPropExporter
    {
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> VRM_UnlitTransparentZWrite 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "VRM/UnlitTransparentZWrite",
                    new ShaderProps
                    {
                        Properties = new ShaderProperty[]{
new ShaderProperty("_MainTex", ShaderPropertyType.TexEnv)

                        }
                    }
                );
            }
        }
    }
}
