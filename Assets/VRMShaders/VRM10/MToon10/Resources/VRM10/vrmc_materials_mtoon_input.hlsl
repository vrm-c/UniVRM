#ifndef VRMC_MATERIALS_MTOON_INPUT_INCLUDED
#define VRMC_MATERIALS_MTOON_INPUT_INCLUDED

#include <UnityShaderVariables.cginc>

// Textures
UNITY_DECLARE_TEX2D(_MainTex);
UNITY_DECLARE_TEX2D(_ShadeTex);
UNITY_DECLARE_TEX2D(_BumpMap);
UNITY_DECLARE_TEX2D(_ShadingShiftTex);
UNITY_DECLARE_TEX2D(_EmissionMap);
UNITY_DECLARE_TEX2D(_MatcapTex);
UNITY_DECLARE_TEX2D(_RimTex);
UNITY_DECLARE_TEX2D(_OutlineWidthTex);
// NOTE: "tex2d() * _Time.y" returns mediump value if sampler is half precision in Android VR platform
UNITY_DECLARE_TEX2D_FLOAT(_UvAnimMaskTex);

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
