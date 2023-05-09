#ifndef VRMC_MATERIALS_MTOON_LIGHTING_UNITY_INCLUDED
#define VRMC_MATERIALS_MTOON_LIGHTING_UNITY_INCLUDED

#ifdef MTOON_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#else
#include <UnityCG.cginc>
#include <AutoLight.cginc>
#include <Lighting.cginc>
#endif

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
#ifdef MTOON_URP
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    #else
    float4 shadowCoord = float4(0, 0, 0, 0);
    #endif
    
    half4 shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
    Light mainLight = GetMainLight(shadowCoord, input.positionWS, shadowMask);
    
    const half3 lightDir = mainLight.direction;
    const half3 lightColor = mainLight.color.rgb;

    float atten = mainLight.shadowAttenuation;

#else
    UNITY_LIGHT_ATTENUATION(atten, input, input.positionWS);

    const half3 lightDir = normalize(UnityWorldSpaceLightDir(input.positionWS));
    const half3 lightColor = _LightColor0.rgb;

#endif

    if (MToon_IsForwardBasePass())
    {
        UnityLighting output;
#ifdef MTOON_URP
        output.indirectLight = SampleSH(normalWS);
        output.indirectLightEqualized = (SampleSH(half3(0, 1, 0)) + SampleSH(half3(0, -1, 0))) * 0.5;
#else
        output.indirectLight = ShadeSH9(half4(normalWS, 1));
        output.indirectLightEqualized = (ShadeSH9(half4(0, 1, 0, 1)) + ShadeSH9(half4(0, -1, 0, 1))) * 0.5;
#endif
        output.directLightColor = lightColor;
        output.directLightDirection = lightDir;
        output.directLightAttenuation = atten;
        return output;
    }
    else
    {
        UnityLighting output;
        output.indirectLight = 0;
        output.indirectLightEqualized = 0;
        output.directLightColor = lightColor;
        output.directLightDirection = lightDir;
        output.directLightAttenuation = atten;
        return output;
    }
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
