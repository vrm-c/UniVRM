#ifndef VRMC_MATERIALS_MTOON_LIGHTING_MTOON_INCLUDED
#define VRMC_MATERIALS_MTOON_LIGHTING_MTOON_INCLUDED

#include <UnityShaderVariables.cginc>
#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_lighting_unity.hlsl"

struct MToonInput
{
    float2 uv;
    half3 normalWS;
    half3 viewDirWS;
    half3 litColor;
    half alpha;
};

inline half GetMToonLighting_Reflectance(const UnityLighting lighting, const MToonInput input)
{
    const half dotNL = dot(input.normalWS, lighting.directLightDirection);
    const half shadingInput = lerp(-1, 1, mtoon_linearstep(-1, 1, dotNL) * lighting.directLightAttenuation);
    const half shadingShift = UNITY_SAMPLE_TEX2D(_ShadingShiftTex, input.uv).r * _ShadingShiftTexScale + _ShadingShiftFactor;
    const half shadingToony = _ShadingToonyFactor;
    return mtoon_linearstep(-1.0 + shadingToony, +1.0 - shadingToony, shadingInput + shadingShift);
}

inline half3 GetMToonLighting_BasicLighting(const UnityLighting unityLight, const MToonInput input, const half reflectance)
{
    if (MToon_IsForwardBasePass())
    {
        const half3 shadeColor = UNITY_SAMPLE_TEX2D(_ShadeTex, input.uv).rgb * _ShadeColor.rgb;

        const half3 direct = lerp(shadeColor, input.litColor, reflectance) * unityLight.directLightColor;
        const half3 indirect = input.litColor * lerp(unityLight.indirectLight, unityLight.indirectLightEqualized, _GiEqualization);

        return direct + indirect;
    }
    else
    {
        const half3 direct = input.litColor * reflectance * unityLight.directLightColor * 0.5;
        const half3 indirect = 0;

        return direct + indirect;
    }
}

inline half3 GetMToonLighting_Emissive(const MToonInput input)
{
    if (MToon_IsForwardBasePass())
    {
        return UNITY_SAMPLE_TEX2D(_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
    }
    else
    {
        return 0;
    }
}

inline half3 GetMToonLighting_Rim(const MToonInput input, const half3 lighting)
{
    if (MToon_IsForwardBasePass())
    {
        const half3 worldUpWS = half3(0, 1, 0);
        const half3 matcapRightWS = cross(input.viewDirWS, worldUpWS);
        const half2 matcapUv = float2(dot(matcapRightWS, input.normalWS), dot(worldUpWS, input.normalWS)) * 0.5 + 0.5;
        const half3 matcapFactor = UNITY_SAMPLE_TEX2D(_MatcapTex, matcapUv).rgb;
        const half3 parametricRimFactor = pow(saturate(1.0 - dot(input.normalWS, input.viewDirWS) + _RimLift), _RimFresnelPower) * _RimColor.rgb;
        const half3 rimLightingFactor = lerp(half3(1, 1, 1), lighting, _RimLightingMix);
        return (matcapFactor + parametricRimFactor) * UNITY_SAMPLE_TEX2D(_RimTex, input.uv).rgb * rimLightingFactor;
    }
    else
    {
        return 0;
    }
}

half4 GetMToonLighting(const UnityLighting unityLight, const MToonInput input)
{
    const half reflectance = GetMToonLighting_Reflectance(unityLight, input);

    const half3 lighting = GetMToonLighting_BasicLighting(unityLight, input, reflectance);
    const half3 emissive = GetMToonLighting_Emissive(input);
    const half3 rim = GetMToonLighting_Rim(input, lighting);

    const half3 col = lighting + emissive + rim;

    return half4(col, input.alpha);
}

#endif
