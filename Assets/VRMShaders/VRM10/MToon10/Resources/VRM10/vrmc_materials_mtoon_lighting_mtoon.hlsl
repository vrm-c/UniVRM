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

inline half GetMToonLighting_Reflectance_ShadingShift(const MToonInput input)
{
    if (MToon_IsParameterMapOn())
    {
        return UNITY_SAMPLE_TEX2D(_ShadingShiftTex, input.uv).r * _ShadingShiftTexScale + _ShadingShiftFactor;
    }
    else
    {
        return _ShadingShiftFactor;
    }
}

inline half GetMToonLighting_DotNL(const UnityLighting lighting, const MToonInput input)
{
    return dot(input.normalWS, lighting.directLightDirection);
}

inline half GetMToonLighting_Shade(const UnityLighting lighting, const MToonInput input, const half dotNL)
{
    const half shadeShift = GetMToonLighting_Reflectance_ShadingShift(input);
    const half shadeToony = _ShadingToonyFactor;

    if (MToon_IsPbrCorrectOn())
    {
        const half shadeInput = dotNL;
        return mtoon_linearstep(-1.0 + shadeToony, +1.0 - shadeToony, shadeInput + shadeShift);
    }

    if (MToon_IsForwardBasePass())
    {
        const half shadeInput = lerp(-1, 1, mtoon_linearstep(-1, 1, dotNL));
        return mtoon_linearstep(-1.0 + shadeToony, +1.0 - shadeToony, shadeInput + shadeShift) * lighting.directLightAttenuation;
    }
    else
    {
        const half shadeInput = dotNL;
        return mtoon_linearstep(-1.0 + shadeToony, +1.0 - shadeToony, shadeInput + shadeShift);
    }
}

inline half GetMToonLighting_Shadow(const UnityLighting lighting, const half dotNL)
{
    if (MToon_IsPbrCorrectOn())
    {
        return lighting.directLightAttenuation * step(0, dotNL);
    }

    if (MToon_IsForwardBasePass())
    {
        return 1;
    }
    else
    {
        // heuristic term for weak lights.
        //     0.5: heuristic.
        //     min(0, dotNL) + 1: darken if (dotNL < 0) by using half lambert.
        return lighting.directLightAttenuation * 0.5 * (min(0, dotNL) + 1);
    }
}

inline half3 GetMToonLighting_DirectLighting(const UnityLighting unityLight, const MToonInput input, const half shade, const half shadow)
{
    const half3 shadeColor = UNITY_SAMPLE_TEX2D(_ShadeTex, input.uv).rgb * _ShadeColor.rgb;
    const half3 albedo = lerp(shadeColor, input.litColor, shade);

    return albedo * unityLight.directLightColor * shadow;
}

inline half3 GetMToonLighting_GlobalIllumination(const UnityLighting unityLight, const MToonInput input)
{
    if (MToon_IsForwardBasePass())
    {
        return input.litColor * lerp(unityLight.indirectLight, unityLight.indirectLightEqualized, _GiEqualization);
    }
    else
    {
        return 0;
    }
}

inline half3 GetMToonLighting_Emissive(const MToonInput input)
{
    if (MToon_IsForwardBasePass())
    {
        if (MToon_IsEmissiveMapOn())
        {
            return UNITY_SAMPLE_TEX2D(_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
        }
        else
        {
            return _EmissionColor.rgb;
        }
    }
    else
    {
        return 0;
    }
}

inline half3 GetMToonLighting_Rim_Matcap(const MToonInput input)
{
    if (MToon_IsRimMapOn())
    {
        const half3 worldUpWS = half3(0, 1, 0);
        // TODO: use view space axis if abs(dot(viewDir, worldUp)) == 1.0
        const half3 matcapRightAxisWS = normalize(cross(input.viewDirWS, worldUpWS));
        const half3 matcapUpAxisWS = normalize(cross(matcapRightAxisWS, input.viewDirWS));
        const half2 matcapUv = float2(dot(matcapRightAxisWS, input.normalWS), dot(matcapUpAxisWS, input.normalWS)) * 0.5 + 0.5;
        return _MatcapColor.rgb * UNITY_SAMPLE_TEX2D(_MatcapTex, matcapUv).rgb;
    }
    else
    {
        return _MatcapColor.rgb;
    }
}

inline half3 GetMToonLighting_Rim(const UnityLighting unityLight, const MToonInput input, const half shadow)
{
    const half3 parametricRimFactor = pow(saturate(1.0 - dot(input.normalWS, input.viewDirWS) + _RimLift), max(_RimFresnelPower, EPS_COL)) * _RimColor.rgb;
    const half3 matcapFactor = GetMToonLighting_Rim_Matcap(input);
    const half3 directLightingFactor = unityLight.directLightColor * shadow;

    half3 rimLightingFactor;
    if (MToon_IsForwardBasePass())
    {
        const half3 indirectLightingFactor = unityLight.indirectLight;

        rimLightingFactor = lerp(half3(1, 1, 1), directLightingFactor + indirectLightingFactor, _RimLightingMix);
    }
    else
    {
        rimLightingFactor = lerp(half3(0, 0, 0), directLightingFactor, _RimLightingMix);
    }

    if (MToon_IsRimMapOn())
    {
        return (matcapFactor + parametricRimFactor) * rimLightingFactor * UNITY_SAMPLE_TEX2D(_RimTex, input.uv).rgb;
    }
    else
    {
        return (matcapFactor + parametricRimFactor) * rimLightingFactor;
    }
}

half4 GetMToonLighting(const UnityLighting unityLight, const MToonInput input)
{
    const half dotNL = GetMToonLighting_DotNL(unityLight, input);
    const half shade = GetMToonLighting_Shade(unityLight, input, dotNL);
    const half shadow = GetMToonLighting_Shadow(unityLight, dotNL);

    const half3 direct = GetMToonLighting_DirectLighting(unityLight, input, shade, shadow);
    const half3 indirect = GetMToonLighting_GlobalIllumination(unityLight, input);
    const half3 lighting = direct + indirect;
    const half3 emissive = GetMToonLighting_Emissive(input);
    const half3 rim = GetMToonLighting_Rim(unityLight, input, shadow);

    const half3 baseCol = lighting + emissive + rim;

    if (MToon_IsOutlinePass())
    {
        const half3 outlineCol = _OutlineColor.rgb * lerp(half3(1, 1, 1), baseCol, _OutlineLightingMix);
        return half4(outlineCol, input.alpha);
    }
    else
    {
        return half4(baseCol, input.alpha);
    }
}

#endif
