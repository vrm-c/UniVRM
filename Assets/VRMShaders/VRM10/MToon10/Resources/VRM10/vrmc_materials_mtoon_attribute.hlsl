#ifndef VRMC_MATERIALS_MTOON_ATTRIBUTE_INCLUDED
#define VRMC_MATERIALS_MTOON_ATTRIBUTE_INCLUDED

#include "./vrmc_materials_mtoon_render_pipeline.hlsl"

struct Attributes
{
    float4 vertex : POSITION; // UnityCG macro specified name. Accurately "positionOS"
    float3 normalOS : NORMAL;
#if defined(_NORMALMAP)
    float4 tangentOS : TANGENT;
#endif
    float2 texcoord0 : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
#if defined(_NORMALMAP)
    half4 tangentWS : TEXCOORD3;
#endif
    float3 viewDirWS : TEXCOORD4;

#ifdef MTOON_URP
    half4 fogFactorAndVertexLight   : TEXCOORD5; // x: fogFactor, yzw: vertex light
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
    #endif

    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 7);

#else
    UNITY_FOG_COORDS(5)
    UNITY_LIGHTING_COORDS(6,7)
#endif
    
    float4 pos : SV_POSITION; // UnityCG macro specified name. Accurately "positionCS"
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

struct FragmentInput
{
    Varyings varyings;
    MTOON_FRONT_FACE_TYPE facing : MTOON_FRONT_FACE_SEMANTIC;
};

#endif