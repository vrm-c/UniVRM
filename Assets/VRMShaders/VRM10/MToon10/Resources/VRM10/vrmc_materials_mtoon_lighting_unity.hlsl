#ifndef VRMC_MATERIALS_MTOON_LIGHTING_UNITY_INCLUDED
#define VRMC_MATERIALS_MTOON_LIGHTING_UNITY_INCLUDED

#include <UnityCG.cginc>
#include <AutoLight.cginc>
#include <Lighting.cginc>
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
    UNITY_LIGHT_ATTENUATION(atten, input, input.positionWS);

    const half3 lightDir = normalize(UnityWorldSpaceLightDir(input.positionWS));
    const half3 lightColor = _LightColor0.rgb;

    if (MToon_IsForwardBasePass())
    {
        UnityLighting output;
        output.indirectLight = ShadeSH9(half4(normalWS, 1));
        output.indirectLightEqualized = (ShadeSH9(half4(0, 1, 0, 1)) + ShadeSH9(half4(0, -1, 0, 1))) * 0.5;
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

#endif
