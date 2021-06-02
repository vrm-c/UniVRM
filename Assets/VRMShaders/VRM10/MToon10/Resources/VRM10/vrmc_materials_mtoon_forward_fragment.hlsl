#ifndef VRMC_MATERIALS_MTOON_FORWARD_FRAGMENT_INCLUDED
#define VRMC_MATERIALS_MTOON_FORWARD_FRAGMENT_INCLUDED

#include <UnityCG.cginc>
#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"
#include "./vrmc_materials_mtoon_lighting.hlsl"
#include "./vrmc_materials_mtoon_uv.hlsl"
#include "./vrmc_materials_mtoon_unity_lighting.hlsl"

half4 MToonFragment(const Varyings input) : SV_Target
{
    // Get MToon UV (with UVAnimation)
    const float2 uv = GetMToonUv(input.uv);

    // Get LitColor with Alpha
    const half4 litColor = UNITY_SAMPLE_TEX2D(_MainTex, uv) * _Color;

    // Alpha Test
#if defined(_ALPHATEST_ON)
    const half rawAlpha = litColor.a;
    const half tmpAlpha = (rawAlpha - _Cutoff) / max(fwidth(rawAlpha), 0.00001) + 0.5; // Alpha to Coverage
    clip(tmpAlpha - _Cutoff);
    const half alpha = 1.0;
#elif defined(_ALPHABLEND_ON)
    const half alpha = litColor.a;
    clip(alpha - EPS_COL);
#else
    const half alpha = 1.0;
#endif

    // Get Normal in WorldSpace from Normalmap if available
#if defined(_NORMALMAP)
    const half3 normalTS = normalize(UnpackNormalWithScale(UNITY_SAMPLE_TEX2D(_BumpMap, uv), _BumpScale));
    const half3 normalWS = normalize(mul(normalTS, MToon_GetTangentToWorld(input.normalWS, input.tangentWS)));
#else
    const half3 normalWS = normalize(input.normalWS);
#endif

    // Get Unity Lighting
    const UnityLighting unityLighting = GetUnityLighting(input, normalWS);

    // Get MToon Lighting
    MToonInput mtoonInput;
    mtoonInput.uv = uv;
    mtoonInput.normalWS = normalWS;
    mtoonInput.viewDirWS = input.viewDirWS;
    mtoonInput.litColor = litColor.rgb;
    mtoonInput.alpha = alpha;
    const half4 col = GetMToonLighting(unityLighting, mtoonInput);

    UNITY_APPLY_FOG(i.fogCoord, col);

    return col;
}

#endif
