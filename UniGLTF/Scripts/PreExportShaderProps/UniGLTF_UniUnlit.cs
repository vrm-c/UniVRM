using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreShaderPropExporter
    {
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> UniGLTF_UniUnlit 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "UniGLTF/UniUnlit",
                    new ShaderProps
                    {
                        Properties = new ShaderProperty[]{
new ShaderProperty("_MainTex", ShaderPropertyType.TexEnv)
,new ShaderProperty("_Color", ShaderPropertyType.Color)
,new ShaderProperty("_Cutoff", ShaderPropertyType.Range)
,new ShaderProperty("_BlendMode", ShaderPropertyType.Float)
,new ShaderProperty("_CullMode", ShaderPropertyType.Float)
,new ShaderProperty("_VColBlendMode", ShaderPropertyType.Float)
,new ShaderProperty("_SrcBlend", ShaderPropertyType.Float)
,new ShaderProperty("_DstBlend", ShaderPropertyType.Float)
,new ShaderProperty("_ZWrite", ShaderPropertyType.Float)

                        }
                    }
                );
            }
        }
    }
}
