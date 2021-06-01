#ifndef VRMC_MATERIALS_MTOON_UNITY_LIGHTING_INCLUDED
#define VRMC_MATERIALS_MTOON_UNITY_LIGHTING_INCLUDED

#include <UnityCG.cginc>
#include <AutoLight.cginc>
#include <Lighting.cginc>
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

UnityLighting GetUnityLighting(Varyings input, half3 normalWS)
{
    UNITY_LIGHT_ATTENUATION(atten, input, input.positionWS);

    const half3 lightDir = normalize(UnityWorldSpaceLightDir(input.positionWS));
    const half3 lightColor = _LightColor0.rgb;

    UnityLighting output = (UnityLighting) 0;

#if defined(UNITY_PASS_FORWARDBASE)
    output.indirectLight = ShadeSH9(half4(normalWS, 1));
    output.indirectLightEqualized = (ShadeSH9(half4(0, 1, 0, 1)) + ShadeSH9(half4(0, -1, 0, 1))) * 0.5;
    output.directLightColor = lightColor;
    output.directLightDirection = lightDir;
    output.directLightAttenuation = atten;
#elif defined(UNITY_PASS_FORWARDADD)
    output.indirectLight = 0;
    output.indirectLightEqualized = 0;
    output.directLightColor = lightColor;
    output.directLightDirection = lightDir;
    output.directLightAttenuation = atten;
#endif

    return output;
}

#endif
