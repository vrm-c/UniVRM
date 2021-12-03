#ifndef VRMC_MATERIALS_MTOON_UTILITY_INCLUDED
#define VRMC_MATERIALS_MTOON_UTILITY_INCLUDED

#include <UnityCG.cginc>

// define
static const float PI_2 = 6.28318530718;
static const float EPS_COL = 0.00001;

inline half mtoon_linearstep(const half start, const half end, const half t)
{
    return min(max((t - start) / (end - start), 0.0), 1.0);
}

inline half3 mtoon_linearstep(const half3 start, const half3 end, const half t)
{
    return min(max((t - start) / (end - start), 0.0), 1.0);
}

inline bool MToon_IsPerspective()
{
    return unity_OrthoParams.w != 1.0;
}

inline float3 MToon_GetWorldSpaceViewDir(const float3 positionWS)
{
    if (MToon_IsPerspective())
    {
        return _WorldSpaceCameraPos.xyz - positionWS;
    }
    else
    {
        const float3 cameraForwardWS = normalize(UNITY_MATRIX_V[2].xyz);
        const float3 viewDirWS = _WorldSpaceCameraPos.xyz - positionWS;
        return cameraForwardWS * dot(cameraForwardWS, viewDirWS);
    }
}

inline float3 MToon_GetWorldSpaceNormalizedViewDir(const float3 positionWS)
{
    if (MToon_IsPerspective())
    {
        return normalize(_WorldSpaceCameraPos.xyz - positionWS);
    }
    else
    {
        return normalize(UNITY_MATRIX_V[2].xyz);
    }
}

inline half3x3 MToon_GetTangentToWorld(const half3 normalWS, const half4 tangentWS)
{
    const half3 normalizedNormalWS = normalize(normalWS);
    const half3 normalizedTangentWS = normalize(tangentWS.xyz);

    const half3 normalizedBitangentWS = normalize(tangentWS.w * cross(normalizedNormalWS, normalizedTangentWS));

    return half3x3(normalizedTangentWS, normalizedBitangentWS, normalizedNormalWS);
}

inline half3 MToon_GetObjectToViewNormal(const half3 normalOS)
{
    return normalize(mul((half3x3)UNITY_MATRIX_IT_MV, normalOS));
}

#endif
