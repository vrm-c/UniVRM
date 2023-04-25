#ifndef VRMC_MATERIALS_MTOON_INPUT_INCLUDED
#define VRMC_MATERIALS_MTOON_INPUT_INCLUDED

#ifdef MTOON_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
#include <UnityShaderVariables.cginc>
#endif

// Textures
TEXTURE2D(_MainTex);                SAMPLER(sampler_MainTex);
TEXTURE2D(_ShadeTex);               SAMPLER(sampler_ShadeTex);
TEXTURE2D(_BumpMap);                SAMPLER(sampler_BumpMap);
TEXTURE2D(_ShadingShiftTex);        SAMPLER(sampler_ShadingShiftTex);
TEXTURE2D(_EmissionMap);            SAMPLER(sampler_EmissionMap);
TEXTURE2D(_MatcapTex);              SAMPLER(sampler_MatcapTex);
TEXTURE2D(_RimTex);                 SAMPLER(sampler_RimTex);
TEXTURE2D(_OutlineWidthTex);        SAMPLER(sampler_OutlineWidthTex);
// NOTE: "tex2d() * _Time.y" returns mediump value if sampler is half precision in Android VR platform
TEXTURE2D_FLOAT(_UvAnimMaskTex);    SAMPLER(sampler_UvAnimMaskTex);

CBUFFER_START(UnityPerMaterial)
// Vector
float4 _MainTex_ST;
// Colors
half4 _Color;
half4 _ShadeColor;
half4 _EmissionColor;
half4 _MatcapColor;
half4 _RimColor;
half4 _OutlineColor;
// Floats
half _Cutoff;
half _BumpScale;
half _ShadingShiftFactor;
half _ShadingShiftTexScale;
half _ShadingToonyFactor;
half _GiEqualization;
half _RimFresnelPower;
half _RimLift;
half _RimLightingMix;
half _OutlineWidth;
half _OutlineLightingMix;
float _UvAnimScrollXSpeed;
float _UvAnimScrollYSpeed;
float _UvAnimRotationSpeed;
CBUFFER_END

// No Using on shader
// half _AlphaMode;
// half _TransparentWithZWrite;
// half _RenderQueueOffset;
// half _DoubleSided;
// half _OutlineWidthMode;

#endif
