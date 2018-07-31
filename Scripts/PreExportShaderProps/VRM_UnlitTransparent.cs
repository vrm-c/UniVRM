using System.Collections.Generic;


namespace VRM 
{
    public static partial class VRMPreShaderPropExporter
    {
        [VRMPreExportShader]
        static KeyValuePair<string, ShaderProps> VRM_UnlitTransparent 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "VRM/UnlitTransparent",
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
