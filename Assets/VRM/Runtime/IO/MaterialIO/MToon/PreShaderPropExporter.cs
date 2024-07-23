namespace VRM
{
    /// <summary>
    /// TODO: MToon にひきとってもらう
    /// </summary>
    public static class PreShaderPropExporter
    {
        const string ShaderName = "VRM/MToon";
        public static ShaderProps GetPropsForMToon()
        {
            return VRM_MToon;
        }

        static ShaderProps VRM_MToon =
                    new ShaderProps
                    {
                        Properties = new ShaderProperty[]{
new ShaderProperty("_Cutoff", ShaderPropertyType.Range)
,new ShaderProperty("_Color", ShaderPropertyType.Color)
,new ShaderProperty("_ShadeColor", ShaderPropertyType.Color)
,new ShaderProperty("_MainTex", ShaderPropertyType.TexEnv)
,new ShaderProperty("_ShadeTexture", ShaderPropertyType.TexEnv)
,new ShaderProperty("_BumpScale", ShaderPropertyType.Float)
,new ShaderProperty("_BumpMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_ReceiveShadowRate", ShaderPropertyType.Range)
,new ShaderProperty("_ReceiveShadowTexture", ShaderPropertyType.TexEnv)
,new ShaderProperty("_ShadingGradeRate", ShaderPropertyType.Range)
,new ShaderProperty("_ShadingGradeTexture", ShaderPropertyType.TexEnv)
,new ShaderProperty("_ShadeShift", ShaderPropertyType.Range)
,new ShaderProperty("_ShadeToony", ShaderPropertyType.Range)
,new ShaderProperty("_LightColorAttenuation", ShaderPropertyType.Range)
,new ShaderProperty("_IndirectLightIntensity", ShaderPropertyType.Range)
,new ShaderProperty("_RimColor", ShaderPropertyType.Color)
,new ShaderProperty("_RimTexture", ShaderPropertyType.TexEnv)
,new ShaderProperty("_RimLightingMix", ShaderPropertyType.Range)
,new ShaderProperty("_RimFresnelPower", ShaderPropertyType.Range)
,new ShaderProperty("_RimLift", ShaderPropertyType.Range)
,new ShaderProperty("_SphereAdd", ShaderPropertyType.TexEnv)
,new ShaderProperty("_EmissionColor", ShaderPropertyType.Color)
,new ShaderProperty("_EmissionMap", ShaderPropertyType.TexEnv)
,new ShaderProperty("_OutlineWidthTexture", ShaderPropertyType.TexEnv)
,new ShaderProperty("_OutlineWidth", ShaderPropertyType.Range)
,new ShaderProperty("_OutlineScaledMaxDistance", ShaderPropertyType.Range)
,new ShaderProperty("_OutlineColor", ShaderPropertyType.Color)
,new ShaderProperty("_OutlineLightingMix", ShaderPropertyType.Range)
,new ShaderProperty("_UvAnimMaskTexture", ShaderPropertyType.TexEnv)
,new ShaderProperty("_UvAnimScrollX", ShaderPropertyType.Float)
,new ShaderProperty("_UvAnimScrollY", ShaderPropertyType.Float)
,new ShaderProperty("_UvAnimRotation", ShaderPropertyType.Float)
,new ShaderProperty("_MToonVersion", ShaderPropertyType.Float)
,new ShaderProperty("_DebugMode", ShaderPropertyType.Float)
,new ShaderProperty("_BlendMode", ShaderPropertyType.Float)
,new ShaderProperty("_OutlineWidthMode", ShaderPropertyType.Float)
,new ShaderProperty("_OutlineColorMode", ShaderPropertyType.Float)
,new ShaderProperty("_CullMode", ShaderPropertyType.Float)
,new ShaderProperty("_OutlineCullMode", ShaderPropertyType.Float)
,new ShaderProperty("_SrcBlend", ShaderPropertyType.Float)
,new ShaderProperty("_DstBlend", ShaderPropertyType.Float)
,new ShaderProperty("_ZWrite", ShaderPropertyType.Float)
                        }
                    };
    }
}
