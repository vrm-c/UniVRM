#ifndef VRMC_MATERIALS_MTOON_LIGHTING_INCLUDED
#define VRMC_MATERIALS_MTOON_LIGHTING_INCLUDED

#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_unity_lighting.hlsl"

struct MToonLightingInput
{
    float2 uv;
    half3 normalWS;
    half3 litColor;
    half alpha;
};

half4 GetMToonLighting(UnityLighting lighting, MToonLightingInput input)
{
    const half dotNL = dot(input.normalWS, lighting.directLightDirection);
    const half shadingInput = lerp(-1, 1, mtoon_linearstep(-1, 1, dotNL) * lighting.directLightAttenuation);
    const half shadingShift = UNITY_SAMPLE_TEX2D(_ShadingShiftTex, input.uv).r * _ShadingShiftTexScale + _ShadingShiftFactor;
    const half shadingToony = _ShadingToonyFactor;
    const half shading = mtoon_linearstep(-1.0 + shadingToony, +1.0 - shadingToony, shadingInput + shadingShift);

    // lighting
#if defined(UNITY_PASS_FORWARDBASE)
    const half3 shadeColor = UNITY_SAMPLE_TEX2D(_ShadeTex, input.uv).rgb * _ShadeColor.rgb;

    const half3 direct = lerp(shadeColor, input.litColor, shading) * lighting.directLightColor;
    const half3 indirect = input.litColor * lerp(lighting.indirectLight, lighting.indirectLightEqualized, _GiEqualization);

#elif defined(UNITY_PASS_FORWARDADD)
    const half3 direct = input.litColor * shading * lighting.directLightColor * lerp(1, 0.5, shadingToony);
    const half3 indirect = 0;

#else
    const half3 direct = 0; // unexpected
    const half3 indirect = 0;

#endif

    // emission
#if defined(UNITY_PASS_FORWARDBASE)
    const half3 emission = UNITY_SAMPLE_TEX2D(_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
#elif defined(UNITY_PASS_FORWARDADD)
    const half3 emission = 0;
#else
    const half3 emission = 0;
#endif


    const half3 col = direct + indirect + emission;

    return half4(col, input.alpha);
}

#endif
