#ifndef VRMC_MATERIALS_MTOON_GEOMETRY_VERTEX_INCLUDED
#define VRMC_MATERIALS_MTOON_GEOMETRY_VERTEX_INCLUDED

#include "./vrmc_materials_mtoon_render_pipeline.hlsl"
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
        return _OutlineWidth * UNITY_SAMPLE_TEX2D_LOD(_OutlineWidthTex, uv, 0).x;
    }
    else
    {
        return _OutlineWidth;
    }
}

inline float MToon_GetOutlineVertex_ScreenCoordinatesWidthMultiplier(const float4 positionCS)
{
    // NOTE: VRM 1.0 の仕様にはない実装なので、ユーザが任意で機能を外せるようにする.
    if (MToon_UseStrictMode())
    {
        return 1.0f;
    }
    else
    {
        const float maxViewFrustumPlaneHeight = 2.0f;
        const float invTangentHalfVerticalFov = unity_CameraProjection[1][1];
        // NOTE: viewFrustumPlaneHeight = tan(halfFov) * viewDirDistance * 2.0
        //           -> viewDirDistance = viewFrustumPlaneHeight / (tan(halfFov) * 2.0)
        const float widthScaledMaxDistance = maxViewFrustumPlaneHeight * invTangentHalfVerticalFov * 0.5f;
        // NOTE: VR などの高視野角カメラでは、純粋な実装では太くなりすぎる.
        //       よってある距離での視錐台平面が高さ 2m 以上になったらスケールをやめる.
        return min(positionCS.w, widthScaledMaxDistance);
    }
}

inline VertexPositionInfo MToon_GetOutlineVertex(const float3 positionOS, const half3 normalOS, const float2 uv)
{
    if (MToon_IsOutlineModeWorldCoordinates())
    {
        const float3 positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1)).xyz;
        const half3 normalWS =  MToon_TransformObjectToWorldNormal(normalOS);
        const half outlineWidth = MToon_GetOutlineVertex_OutlineWidth(uv);

        VertexPositionInfo output;
        output.positionWS = float4(positionWS + normalWS * outlineWidth, 1);
        output.positionCS = MToon_TransformWorldToHClip(output.positionWS.xyz);

        return output;
    }
    else if (MToon_IsOutlineModeScreenCoordinates())
    {
        const float3 positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1)).xyz;
        const half outlineWidth = MToon_GetOutlineVertex_OutlineWidth(uv);

        const float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
        const half aspect = abs(nearUpperRight.y / nearUpperRight.x);
        const float4 positionCS = MToon_TransformObjectToHClip(positionOS);
        const half3 normalVS = MToon_GetObjectToViewNormal(normalOS);
        const half3 normalCS = MToon_TransformViewToHClip(normalVS.xyz);

        half2 normalProjectedCS = normalize(normalCS.xy);
        const float clipSpaceHeight = 2.0f;
        normalProjectedCS *= clipSpaceHeight * outlineWidth * MToon_GetOutlineVertex_ScreenCoordinatesWidthMultiplier(positionCS);
        normalProjectedCS.x *= aspect;
        // NOTE: カメラ方向軸を向く法線を持つ頂点が XY 方向にだけずれると困るので、それを抑制する.
        normalProjectedCS.xy *= saturate(1 - normalVS.z * normalVS.z);

        VertexPositionInfo output;
        output.positionWS = float4(positionWS, 1);
        output.positionCS = float4(positionCS.xy + normalProjectedCS.xy, positionCS.zw);
        return output;
    }
    else
    {
        VertexPositionInfo output;
        output.positionWS = mul(unity_ObjectToWorld, float4(positionOS * 0.001, 1));
        output.positionCS = MToon_TransformWorldToHClip(output.positionWS.xyz);

        return output;
    }
}

inline VertexPositionInfo MToon_GetVertex(const float3 positionOS)
{
    VertexPositionInfo output;
    output.positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1));
    output.positionCS = MToon_TransformWorldToHClip(output.positionWS.xyz);

    return output;
}

#endif
