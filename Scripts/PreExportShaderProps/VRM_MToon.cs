using System.Collections.Generic;


namespace UniGLTF.ShaderPropExporter
{
    public static partial class PreShaderPropExporter
    {
        [PreExportShader]
        static KeyValuePair<string, ShaderProps> VRM_MToon 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "VRM/MToon",
                    new ShaderProps
                    {
                        Properties = new ShaderProperty[]{
new ShaderProperty("_Cutoff", ShaderPropertyType.Range, false)
,new ShaderProperty("_Color", ShaderPropertyType.Color, false)
,new ShaderProperty("_ShadeColor", ShaderPropertyType.Color, false)
,new ShaderProperty("_MainTex", ShaderPropertyType.TexEnv, false)
,new ShaderProperty("_ShadeTexture", ShaderPropertyType.TexEnv, false)
,new ShaderProperty("_BumpScale", ShaderPropertyType.Float, false)
,new ShaderProperty("_BumpMap", ShaderPropertyType.TexEnv, true)
,new ShaderProperty("_ReceiveShadowRate", ShaderPropertyType.Range, false)
,new ShaderProperty("_ReceiveShadowTexture", ShaderPropertyType.TexEnv, false)
,new ShaderProperty("_ShadeShift", ShaderPropertyType.Range, false)
,new ShaderProperty("_ShadeToony", ShaderPropertyType.Range, false)
,new ShaderProperty("_LightColorAttenuation", ShaderPropertyType.Range, false)
,new ShaderProperty("_SphereAdd", ShaderPropertyType.TexEnv, false)
,new ShaderProperty("_EmissionColor", ShaderPropertyType.Color, false)
,new ShaderProperty("_EmissionMap", ShaderPropertyType.TexEnv, false)
,new ShaderProperty("_OutlineWidthTexture", ShaderPropertyType.TexEnv, false)
,new ShaderProperty("_OutlineWidth", ShaderPropertyType.Range, false)
,new ShaderProperty("_OutlineScaledMaxDistance", ShaderPropertyType.Range, false)
,new ShaderProperty("_OutlineColor", ShaderPropertyType.Color, false)
,new ShaderProperty("_OutlineLightingMix", ShaderPropertyType.Range, false)
,new ShaderProperty("_DebugMode", ShaderPropertyType.Float, false)
,new ShaderProperty("_BlendMode", ShaderPropertyType.Float, false)
,new ShaderProperty("_OutlineWidthMode", ShaderPropertyType.Float, false)
,new ShaderProperty("_OutlineColorMode", ShaderPropertyType.Float, false)
,new ShaderProperty("_CullMode", ShaderPropertyType.Float, false)
,new ShaderProperty("_OutlineCullMode", ShaderPropertyType.Float, false)
,new ShaderProperty("_SrcBlend", ShaderPropertyType.Float, false)
,new ShaderProperty("_DstBlend", ShaderPropertyType.Float, false)
,new ShaderProperty("_ZWrite", ShaderPropertyType.Float, false)
,new ShaderProperty("_IsFirstSetup", ShaderPropertyType.Float, false)

                        }
                    }
                );
            }
        }
    }
}
