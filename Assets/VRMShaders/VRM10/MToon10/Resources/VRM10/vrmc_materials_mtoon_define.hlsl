#ifndef VRMC_MATERIALS_MTOON_DEFINE_INCLUDED
#define VRMC_MATERIALS_MTOON_DEFINE_INCLUDED

// define
static const float PI_2 = 6.28318530718;
static const float EPS_COL = 0.00001;

inline half3 mtoon_linearstep(half3 start, half3 end, half t)
{
    return min(max((t - start) / (end - start), 0.0), 1.0);
}

inline bool MToon_IsPerspective()
{
    return unity_OrthoParams.w != 1.0;
}

inline float3 MToon_GetWorldSpaceNormalizedViewDir(float3 positionWS)
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

inline half3x3 MToon_GetTangentToWorld(half3 normalWS, half4 tangentWS)
{
    const half3 normalizedNormalWS = normalize(normalWS);
    const half3 normalizedTangentWS = normalize(tangentWS.xyz);

    const half3 normalizedBitangentWS = normalize(tangentWS.w * cross(normalizedNormalWS, normalizedTangentWS));

    return half3x3(normalizedTangentWS, normalizedBitangentWS, normalizedNormalWS);
}

inline bool MToon_IsForwardBasePass()
{
#if defined(UNITY_PASS_FORWARDBASE)
    return true;
#elif defined(UNITY_PASS_FORWARDADD)
    return false;
#else
    // ????
    return false;
#endif
}

#endif
