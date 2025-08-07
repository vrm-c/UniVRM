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

// Light
#ifdef MTOON_URP

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
#define MTOON_SHADOW_COORD(input) input.shadowCoord
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
#define MTOON_SHADOW_COORD(input) TransformWorldToShadowCoord(input.positionWS)
#else
#define MTOON_SHADOW_COORD(input) float4(0, 0, 0, 0)
#endif

#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
#define MTOON_SAMPLE_SHADOWMASK(uv) SAMPLE_TEXTURE2D_LIGHTMAP(SHADOWMASK_NAME, SHADOWMASK_SAMPLER_NAME, uv SHADOWMASK_SAMPLE_EXTRA_ARGS)
#elif !defined (LIGHTMAP_ON)
#define MTOON_SAMPLE_SHADOWMASK(uv) unity_ProbesOcclusion
#else
#define MTOON_SAMPLE_SHADOWMASK(uv) half4(1, 1, 1, 1)
#endif

#define MTOON_LIGHT_DESCRIPTION(input, atten, lightDir, lightColor) \
    const half3 lightDir = _MainLightPosition.xyz; \
    const half3 lightColor = _MainLightColor.rgb; \
    const float atten = MainLightShadow(MTOON_SHADOW_COORD(input), input.positionWS, MTOON_SAMPLE_SHADOWMASK(input.lightmapUV), _MainLightOcclusionProbes);

#else

#define MTOON_LIGHT_DESCRIPTION(input, atten, lightDir, lightColor) \
    UNITY_LIGHT_ATTENUATION(atten, input, input.positionWS); \
    const half3 lightDir = normalize(UnityWorldSpaceLightDir(input.positionWS)); \
    const half3 lightColor = _LightColor0.rgb;

#endif

// Transfer fog and lighting
#ifdef MTOON_URP

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
#define MTOON_TRANSFER_FOG_AND_LIGHTING(output, outpos, coord, vertex) \
    OUTPUT_LIGHTMAP_UV(coord.xy, unity_LightmapST, output.lightmapUV); \
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH); \
    output.fogFactorAndVertexLight = half4(ComputeFogFactor(outpos.z), VertexLighting(output.positionWS, output.normalWS)); \
    output.shadowCoord = GetShadowCoord(GetVertexPositionInputs(vertex.xyz)); 
#else
#define MTOON_TRANSFER_FOG_AND_LIGHTING(output, outpos, coord, vertex) \
    OUTPUT_LIGHTMAP_UV(coord.xy, unity_LightmapST, output.lightmapUV); \
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH); \
    output.fogFactorAndVertexLight = half4(ComputeFogFactor(outpos.z), VertexLighting(output.positionWS, output.normalWS));
#endif

#else
#define MTOON_TRANSFER_FOG_AND_LIGHTING(output, outpos, coord, vertex) \
    UNITY_TRANSFER_FOG(output, outpos); \
    UNITY_TRANSFER_LIGHTING(output, coord.xy);
#endif

// SampleSH
inline half3 MToon_SampleSH(half3 normalWS)
{
    #ifdef MTOON_URP
    return SampleSH(normalWS);
    #else
    return ShadeSH9(half4(normalWS, 1));
    #endif
}

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
