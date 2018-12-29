using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreShaderPropExporter
    {
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> VRM_UnlitTransparent 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "VRM/UnlitTransparent",
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
