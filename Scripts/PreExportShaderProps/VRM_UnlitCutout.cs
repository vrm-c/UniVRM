using System.Collections.Generic;


namespace VRM 
{
    public static partial class VRMPreShaderPropExporter
    {
        [VRMPreExportShader]
        static KeyValuePair<string, ShaderProps> VRM_UnlitCutout 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "VRM/UnlitCutout",
                    new ShaderProps
                    {
                        Properties = new KeyValuePair<string, ShaderPropertyType>[]{
new KeyValuePair<string, ShaderPropertyType>("_MainTex", ShaderPropertyType.TexEnv)
,new KeyValuePair<string, ShaderPropertyType>("_Cutoff", ShaderPropertyType.Range)

                        }
                    }
                );
            }
        }
    }
}
