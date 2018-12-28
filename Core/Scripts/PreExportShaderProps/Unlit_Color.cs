using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreShaderPropExporter
    {
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> Unlit_Color 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "Unlit/Color",
                    new ShaderProps
                    {
                        Properties = new ShaderProperty[]{
new ShaderProperty("_Color", ShaderPropertyType.Color)

                        }
                    }
                );
            }
        }
    }
}
