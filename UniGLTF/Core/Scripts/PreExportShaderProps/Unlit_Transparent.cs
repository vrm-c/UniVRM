using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreShaderPropExporter
    {
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> Unlit_Transparent 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "Unlit/Transparent",
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
