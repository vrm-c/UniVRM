using System.Collections.Generic;


namespace VRM 
{
    public static partial class VRMPreShaderPropExporter
    {
        [VRMPreExportShader]
        static KeyValuePair<string, ShaderProps> VRM_UnlitTransparentZWrite 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "VRM/UnlitTransparentZWrite",
                    new ShaderProps
                    {
                        Properties = new KeyValuePair<string, ShaderPropertyType>[]{
new KeyValuePair<string, ShaderPropertyType>("_MainTex", ShaderPropertyType.TexEnv)

                        }
                    }
                );
            }
        }
    }
}
