#ifndef VRMC_MATERIALS_MTOON_UNITY_LIGHTING_INCLUDED
#define VRMC_MATERIALS_MTOON_UNITY_LIGHTING_INCLUDED

#include <UnityCG.cginc>
#include <AutoLight.cginc>
#include <Lighting.cginc>
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"

struct UnityLighting
{
    half3 directLight;
    half3 indirectLight;
};

UnityLighting GetUnityLighting(Varyings input, half3 normalWS)
{
    UNITY_LIGHT_ATTENUATION(atten, input, input.positionWS);

    const half3 lightDir = normalize(UnityWorldSpaceLightDir(input.positionWS));
    const half3 lightColor = _LightColor0.rgb;
    const half dotNL = dot(lightDir, normalWS);

#if defined(UNITY_PASS_FORWARDBASE)
    const half3 indirect = ShadeSH9(half4(normalWS, 1));
    const half3 direct = lightColor * max(0, dotNL) * atten;
#elif defined(UNITY_PASS_FORWARDADD)
    const half3 indirect = 0;
    const half3 direct = lightColor * max(0, dotNL) * atten;
#else
    const half3 indirect = half3(1, 1, 0); // error
    const half3 direct = 0;
#endif

    UnityLighting output = (UnityLighting) 0;
    output.directLight = direct;
    output.indirectLight = indirect;
    return output;
}

#endif
