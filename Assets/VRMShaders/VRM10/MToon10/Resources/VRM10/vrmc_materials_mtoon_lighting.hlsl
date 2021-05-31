#ifndef VRMC_MATERIALS_MTOON_LIGHTING_INCLUDED
#define VRMC_MATERIALS_MTOON_LIGHTING_INCLUDED

#include "./vrmc_materials_mtoon_input.hlsl"

half4 GetMToonLighting(half3 directLight, half3 indirectLight, float2 uv, half4 litColor, half alpha)
{
    const half4 albedo = litColor;

    return half4(albedo.rgb * (directLight + indirectLight), alpha);
}

#endif
