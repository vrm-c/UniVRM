using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreShaderPropExporter
    {
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> VRM_UnlitCutout 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "VRM/UnlitCutout",
                    new ShaderProps
                    {
                        Properties = new ShaderProperty[]{
new ShaderProperty("_MainTex", ShaderPropertyType.TexEnv)
,new ShaderProperty("_Cutoff", ShaderPropertyType.Range)

                        }
                    }
                );
            }
        }
    }
}
