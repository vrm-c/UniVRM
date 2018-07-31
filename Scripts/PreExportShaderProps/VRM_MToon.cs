using System.Collections.Generic;


namespace VRM 
{
    public static partial class VRMPreShaderPropExporter
    {
        [VRMPreExportShader]
        static KeyValuePair<string, ShaderProps> VRM_MToon 
        {
            get 
            {
                return new KeyValuePair<string, ShaderProps>(
                    "VRM/MToon",
                    new ShaderProps
                    {
                        Properties = new KeyValuePair<string, ShaderPropertyType>[]{
new KeyValuePair<string, ShaderPropertyType>("_Cutoff", ShaderPropertyType.Range)
,new KeyValuePair<string, ShaderPropertyType>("_Color", ShaderPropertyType.Color)
,new KeyValuePair<string, ShaderPropertyType>("_ShadeColor", ShaderPropertyType.Color)
,new KeyValuePair<string, ShaderPropertyType>("_MainTex", ShaderPropertyType.TexEnv)
,new KeyValuePair<string, ShaderPropertyType>("_ShadeTexture", ShaderPropertyType.TexEnv)
,new KeyValuePair<string, ShaderPropertyType>("_BumpScale", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_BumpMap", ShaderPropertyType.TexEnv)
,new KeyValuePair<string, ShaderPropertyType>("_ReceiveShadowRate", ShaderPropertyType.Range)
,new KeyValuePair<string, ShaderPropertyType>("_ReceiveShadowTexture", ShaderPropertyType.TexEnv)
,new KeyValuePair<string, ShaderPropertyType>("_ShadeShift", ShaderPropertyType.Range)
,new KeyValuePair<string, ShaderPropertyType>("_ShadeToony", ShaderPropertyType.Range)
,new KeyValuePair<string, ShaderPropertyType>("_LightColorAttenuation", ShaderPropertyType.Range)
,new KeyValuePair<string, ShaderPropertyType>("_SphereAdd", ShaderPropertyType.TexEnv)
,new KeyValuePair<string, ShaderPropertyType>("_EmissionColor", ShaderPropertyType.Color)
,new KeyValuePair<string, ShaderPropertyType>("_EmissionMap", ShaderPropertyType.TexEnv)
,new KeyValuePair<string, ShaderPropertyType>("_OutlineWidthTexture", ShaderPropertyType.TexEnv)
,new KeyValuePair<string, ShaderPropertyType>("_OutlineWidth", ShaderPropertyType.Range)
,new KeyValuePair<string, ShaderPropertyType>("_OutlineScaledMaxDistance", ShaderPropertyType.Range)
,new KeyValuePair<string, ShaderPropertyType>("_OutlineColor", ShaderPropertyType.Color)
,new KeyValuePair<string, ShaderPropertyType>("_OutlineLightingMix", ShaderPropertyType.Range)
,new KeyValuePair<string, ShaderPropertyType>("_DebugMode", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_BlendMode", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_OutlineWidthMode", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_OutlineColorMode", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_CullMode", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_OutlineCullMode", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_SrcBlend", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_DstBlend", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_ZWrite", ShaderPropertyType.Float)
,new KeyValuePair<string, ShaderPropertyType>("_IsFirstSetup", ShaderPropertyType.Float)

                        }
                    }
                );
            }
        }
    }
}
