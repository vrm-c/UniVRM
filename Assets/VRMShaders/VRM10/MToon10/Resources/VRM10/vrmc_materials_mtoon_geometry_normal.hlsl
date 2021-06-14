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

inline float3 GetMToonGeometry_Normal(const Varyings input, const MTOON_FRONT_FACE_TYPE facing, const float2 mtoonUv)
{
    const half3 normalWS = MTOON_IS_FRONT_VFACE(facing, input.normalWS, -input.normalWS);

#if defined(_NORMALMAP)
    // Get Normal in WorldSpace from Normalmap if available
    const half3 normalTS = normalize(UnpackNormalWithScale(UNITY_SAMPLE_TEX2D(_BumpMap, mtoonUv), _BumpScale));
    return normalize(mul(normalTS, MToon_GetTangentToWorld(normalWS, input.tangentWS)));
#else
    return GetMToonGeometry_NormalWithoutNormalMap(normalWS);
#endif
}

#endif
