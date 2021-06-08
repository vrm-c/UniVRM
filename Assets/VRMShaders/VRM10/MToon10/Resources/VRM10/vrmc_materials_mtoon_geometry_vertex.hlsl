#ifndef VRMC_MATERIALS_MTOON_GEOMETRY_VERTEX_INCLUDED
#define VRMC_MATERIALS_MTOON_GEOMETRY_VERTEX_INCLUDED

#include <UnityCG.cginc>
#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"

struct VertexPositionInfo
{
    float4 positionWS;
    float4 positionCS;
};

inline half MToon_GetOutlineVertex_OutlineWidth(const float2 uv)
{
    if (MToon_IsParameterMapOn())
    {
        return _OutlineWidth * UNITY_SAMPLE_TEX2D_LOD(_OutlineWidthTex, uv, 0);
    }
    else
    {
        return _OutlineWidth;
    }
}

inline VertexPositionInfo MToon_GetOutlineVertex(const float3 positionOS, const half3 normalOS, const float2 uv)
{
    if (MToon_IsOutlineModeWorldCoordinates())
    {
        const float3 positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1)).xyz;
        const half outlineWidth = MToon_GetOutlineVertex_OutlineWidth(uv);
        const half3 normalWS = UnityObjectToWorldNormal(normalOS);

        VertexPositionInfo output;
        output.positionWS = float4(positionWS + normalWS * outlineWidth, 1);
        output.positionCS = UnityWorldToClipPos(output.positionWS);
        return output;
    }
    else if (MToon_IsOutlineModeScreenCoordinates())
    {
        const float3 positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1)).xyz;
        const half outlineWidth = MToon_GetOutlineVertex_OutlineWidth(uv);

        const float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
        const half aspect = abs(nearUpperRight.y / nearUpperRight.x);

        float4 positionCS = UnityObjectToClipPos(positionOS);
        const half3 normalVS = MToon_GetObjectToViewNormal(normalOS);
        const half3 normalCS = TransformViewToProjection(normalVS.xyz);
        half2 normalProjectedCS = normalize(normalCS.xy);
        normalProjectedCS *= positionCS.w;
        normalProjectedCS.x *= aspect;
        positionCS.xy += outlineWidth * normalProjectedCS.xy * saturate(1 - abs(normalVS.z)); // ignore offset when normal toward camera

        VertexPositionInfo output;
        output.positionWS = float4(positionWS, 1);
        output.positionCS = positionCS;
        return output;
    }
    else
    {
        VertexPositionInfo output;
        output.positionWS = mul(unity_ObjectToWorld, float4(positionOS * 0.001, 1));
        output.positionCS = UnityWorldToClipPos(output.positionWS);
        return output;
    }
}

inline VertexPositionInfo MToon_GetVertex(const float3 positionOS)
{
    VertexPositionInfo output;
    output.positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1));
    output.positionCS = UnityWorldToClipPos(output.positionWS);
    return output;
}

#endif
