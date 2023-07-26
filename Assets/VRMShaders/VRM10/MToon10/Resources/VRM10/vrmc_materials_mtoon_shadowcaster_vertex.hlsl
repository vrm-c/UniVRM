#ifndef VRMC_MATERIALS_MTOON_SHADOWCASTER_VERTEX_INCLUDED
#define VRMC_MATERIALS_MTOON_SHADOWCASTER_VERTEX_INCLUDED

#ifdef MTOON_URP

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"
#include "./vrmc_materials_mtoon_geometry_vertex.hlsl"

float3 _LightDirection;

VertexPositionInfo MToon_GetShadowCasterVertex(const float3 positionOS, const float3 normalWS)
{
    VertexPositionInfo output;
    output.positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1));
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(output.positionWS.xyz, normalWS, _LightDirection));

    #if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif
    
    output.positionCS = positionCS;

    return output;
}

Varyings MToonShadowCasterVertex(const Attributes v)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
    const VertexPositionInfo position = MToon_GetShadowCasterVertex(v.vertex.xyz, normalWS);

    output.pos = position.positionCS;
    output.uv = v.texcoord0;

    return output;
}

#endif

#endif