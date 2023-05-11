#ifndef VRMC_MATERIALS_MTOON_RENDER_PIPELINE_INCLUDED
#define VRMC_MATERIALS_MTOON_RENDER_PIPELINE_INCLUDED

// Include
#ifdef MTOON_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#else
#include <UnityCG.cginc>
#include <AutoLight.cginc>
#include <Lighting.cginc>
#include <UnityShaderVariables.cginc>
#endif

// Texture
#ifdef MTOON_URP
#define MTOON_DECLARE_TEX2D(tex) TEXTURE2D_FLOAT(tex); SAMPLER(sampler##tex);
#define MTOON_DECLARE_TEX2D_FLOAT(tex) TEXTURE2D(tex); SAMPLER(sampler##tex);
#define MTOON_SAMPLE_TEXTURE2D(tex, uv) SAMPLE_TEXTURE2D(tex, sampler##tex, uv)
#else
#define MTOON_DECLARE_TEX2D(tex) UNITY_DECLARE_TEX2D(tex)
#define MTOON_DECLARE_TEX2D_FLOAT(tex) UNITY_DECLARE_TEX2D_FLOAT(tex);
#define MTOON_SAMPLE_TEXTURE2D(tex, uv) UNITY_SAMPLE_TEX2D(tex, uv)
#endif

// Fog and lighting
#ifdef MTOON_URP

#ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR

#define MTOON_FOG_AND_LIGHTING_COORDS(idx1, idx2, idx3) \
    half4 fogFactorAndVertexLight : TEXCOORD##idx1; \
    float4 shadowCoord              : TEXCOORD##idx2; \
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, idx3);

#else

#define MTOON_FOG_AND_LIGHTING_COORDS(idx1, idx2, idx3) \
    half4 fogFactorAndVertexLight : TEXCOORD##idx1; \
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, idx);

#endif

#else
#define MTOON_FOG_AND_LIGHTING_COORDS(idx1, idx2, idx3) \
    UNITY_FOG_COORDS(5) \
    UNITY_LIGHTING_COORDS(6,7)
#endif

// Transform
inline float3 MToon_TransformObjectToWorldNormal(float3 normalOS)
{
    #ifdef MTOON_URP
    return TransformObjectToWorldNormal(normalOS);
    #else
    return UnityObjectToWorldNormal(normalOS);
    #endif
}

inline float4 MToon_TransformWorldToHClip(float3 positionWS)
{
    #ifdef MTOON_URP
    return TransformWorldToHClip(positionWS);
    #else
    return UnityWorldToClipPos(positionWS);
    #endif
}

inline float4 MToon_TransformObjectToHClip(float3 positionOS)
{
    #ifdef MTOON_URP
    return TransformObjectToHClip(positionOS);
    #else
    return UnityObjectToClipPos(positionOS);
    #endif
}

inline float3 MToon_TransformViewToHClip(float3 positionVS)
{
    #ifdef MTOON_URP
    return mul((float3x3)GetViewToHClipMatrix(), positionVS);
    #else
    return TransformViewToProjection(positionVS);
    #endif
}

inline float3 MToon_UnpackNormalScale(float4 packedNormal, float bumpScale)
{
    #ifdef MTOON_URP
    return UnpackNormalScale(packedNormal, bumpScale);
    #else
    return UnpackNormalWithScale(packedNormal, bumpScale);
    #endif
}

inline float3 MToon_TransformObjectToWorldDir(float3 dirOS)
{
    #ifdef MTOON_URP
    return TransformObjectToWorldDir(dirOS);
    #else
    return UnityObjectToWorldDir(dirOS);
    #endif
}

#endif
