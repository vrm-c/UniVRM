#ifndef VRMC_MATERIALS_MTOON_UTILITY_INCLUDED
#define VRMC_MATERIALS_MTOON_UTILITY_INCLUDED

#ifdef MTOON_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
#include <UnityCG.cginc>
#endif

// define
static const float PI_2 = 6.28318530718;
// 2^-10 (min positive value of 16-bit floating point)
static const half EPSILON_FP16 = 0.0009765625;

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

    // NOTE: tangent Sign は本来、+1 か -1 の値が望まれる。そしてこれは通常、三角形内ですべておなじ値である。
    //       しかし Unity 純正の Mesh.RecalculateTangents() を呼び出すと、頂点ごとに異なる tangent Sign を持つ三角形が生まれることがある。
    //       その場合 interpolate されて 0 となり、ゼロ除算と NaN が発生し、Android 環境等でビジュアルアーティファクトが出る場合があるため、二値化する。
    const half tangentSign = tangentWS.w > 0 ? 1.0 : -1.0;
    const half3 normalizedBitangentWS = normalize(tangentSign * cross(normalizedNormalWS, normalizedTangentWS));

    return half3x3(normalizedTangentWS, normalizedBitangentWS, normalizedNormalWS);
}

inline half3 MToon_GetObjectToViewNormal(const half3 normalOS)
{
    return normalize(mul((half3x3)UNITY_MATRIX_IT_MV, normalOS));
}

#endif
