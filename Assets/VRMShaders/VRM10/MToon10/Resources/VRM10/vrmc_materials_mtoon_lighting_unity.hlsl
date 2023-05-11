#ifndef VRMC_MATERIALS_MTOON_LIGHTING_UNITY_INCLUDED
#define VRMC_MATERIALS_MTOON_LIGHTING_UNITY_INCLUDED

#include "./vrmc_materials_mtoon_render_pipeline.hlsl"
#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"

struct UnityLighting
{
    half3 indirectLight;
    half3 indirectLightEqualized;
    half3 directLightColor;
    half3 directLightDirection;
    half3 directLightAttenuation;
};

UnityLighting GetUnityLighting(const Varyings input, const half3 normalWS)
{
    MTOON_LIGHT_DESCRIPTION(input, atten, lightDir, lightColor);

    if (MToon_IsForwardBasePass())
    {
        UnityLighting output;
        output.indirectLight = MToon_SampleSH(half3(normalWS));
        output.indirectLightEqualized = (MToon_SampleSH(half3(0, 1, 0)) + MToon_SampleSH(half3(0, -1, 0))) * 0.5;
        output.directLightColor = lightColor;
        output.directLightDirection = lightDir;
        output.directLightAttenuation = atten;
        return output;
    }
    UnityLighting output;
    output.indirectLight = 0;
    output.indirectLightEqualized = 0;
    output.directLightColor = lightColor;
    output.directLightDirection = lightDir;
    output.directLightAttenuation = atten;
    return output;
}

#ifdef MTOON_URP
UnityLighting GetAdditionalUnityLighting(const Varyings input, const half3 normalWS, int lightIndex)
{
    // TODO: Duplicate in GetUnityLighting
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    #else
    float4 shadowCoord = float4(0, 0, 0, 0);
    #endif
        
    half4 shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
    Light light = GetAdditionalLight(lightIndex, input.positionWS, shadowMask);

    UnityLighting output;
    output.indirectLight = 0;
    output.indirectLightEqualized = 0;
    output.directLightColor = light.color;
    output.directLightDirection = light.direction;
    output.directLightAttenuation = light.shadowAttenuation * light.distanceAttenuation;
    return output;
}
#endif

#endif
