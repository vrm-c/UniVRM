#ifndef VRMC_MATERIALS_MTOON_FORWARD_FRAGMENT_INCLUDED
#define VRMC_MATERIALS_MTOON_FORWARD_FRAGMENT_INCLUDED

#include <UnityCG.cginc>
#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"
#include "./vrmc_materials_mtoon_geometry_uv.hlsl"
#include "./vrmc_materials_mtoon_geometry_alpha.hlsl"
#include "./vrmc_materials_mtoon_geometry_normal.hlsl"
#include "./vrmc_materials_mtoon_lighting_unity.hlsl"
#include "./vrmc_materials_mtoon_lighting_mtoon.hlsl"

half4 MToonFragment(const FragmentInput fragmentInput) : SV_Target
{
    if (MToon_IsOutlinePass() && MToon_IsOutlineModeDisabled())
    {
        clip(-1);
    }

    const Varyings input = fragmentInput.varyings;
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    // Get MToon UV (with UVAnimation)
    const float2 uv = GetMToonGeometry_Uv(input.uv);

    // Get LitColor with Alpha
    const half4 litColor = UNITY_SAMPLE_TEX2D(_MainTex, uv) * _Color;

    // Alpha Test
    const half alpha = GetMToonGeometry_Alpha(litColor);

    // Get Normal
    const float3 normalWS = GetMToonGeometry_Normal(input, fragmentInput.facing, uv);

    // Get Unity Lighting
    const UnityLighting unityLighting = GetUnityLighting(input, normalWS);

    // Get MToon Lighting
    MToonInput mtoonInput;
    mtoonInput.uv = uv;
    mtoonInput.normalWS = normalWS;
    mtoonInput.viewDirWS = normalize(input.viewDirWS);
    mtoonInput.litColor = litColor.rgb;
    mtoonInput.alpha = alpha;
    half4 col = GetMToonLighting(unityLighting, mtoonInput);

    // Apply Fog
    UNITY_APPLY_FOG(fragmentInput.varyings.fogCoord, col);

    return col;
}

#endif