using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreShaderPropExporter
    {
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> Standard 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "Standard",
                    new ShaderProps
                    {
                        Properties = new ShaderProperty[]{
new ShaderProperty("_Color", ShaderPropertyType.Color)
,new ShaderProperty("_MainTex", ShaderPropertyType.TexEnv)
,new ShaderProperty("_Cutoff", ShaderPropertyType.Range)
,new ShaderProperty("_Glossiness", ShaderPropertyType.Range)
,new ShaderProperty("_GlossMapScale", ShaderPropertyType.Range)
,new ShaderProperty("_SmoothnessTextureChannel", ShaderPropertyType.Float)
,new ShaderProperty("_Metallic", ShaderPropertyType.Range)
,new ShaderProperty("_MetallicGlossMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_SpecularHighlights", ShaderPropertyType.Float)
,new ShaderProperty("_GlossyReflections", ShaderPropertyType.Float)
,new ShaderProperty("_BumpScale", ShaderPropertyType.Float)
,new ShaderProperty("_BumpMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_Parallax", ShaderPropertyType.Range)
,new ShaderProperty("_ParallaxMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_OcclusionStrength", ShaderPropertyType.Range)
,new ShaderProperty("_OcclusionMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_EmissionColor", ShaderPropertyType.Color)
,new ShaderProperty("_EmissionMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_DetailMask", ShaderPropertyType.TexEnv)
,new ShaderProperty("_DetailAlbedoMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_DetailNormalMapScale", ShaderPropertyType.Float)
,new ShaderProperty("_DetailNormalMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_UVSec", ShaderPropertyType.Float)
,new ShaderProperty("_Mode", ShaderPropertyType.Float)
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
