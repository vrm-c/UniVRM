#ifndef VRMC_MATERIALS_MTOON_GEOMETRY_NORMAL_INCLUDED
#define VRMC_MATERIALS_MTOON_GEOMETRY_NORMAL_INCLUDED

#include <UnityCG.cginc>
#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"

inline float3 GetMToonGeometry_NormalWithoutNormalMap(const half3 normalWS)
{
    return normalize(normalWS);
}

inline float3 GetMToonGeometry_Normal(const Varyings input, const float2 mtoonUv)
{
#if defined(_NORMALMAP)
    // Get Normal in WorldSpace from Normalmap if available
    const half3 normalTS = normalize(UnpackNormalWithScale(UNITY_SAMPLE_TEX2D(_BumpMap, mtoonUv), _BumpScale));
    return normalize(mul(normalTS, MToon_GetTangentToWorld(input.normalWS, input.tangentWS)));
#else
    return GetMToonGeometry_NormalWithoutNormalMap(input.normalWS);
#endif
}

#endif
