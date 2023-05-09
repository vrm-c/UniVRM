#ifndef VRMC_MATERIALS_MTOON_SHADOWCASTER_FRAGMENT_INCLUDED
#define VRMC_MATERIALS_MTOON_SHADOWCASTER_FRAGMENT_INCLUDED

#ifdef MTOON_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"
#include "./vrmc_materials_mtoon_geometry_uv.hlsl"
#include "./vrmc_materials_mtoon_geometry_alpha.hlsl"
#include "./vrmc_materials_mtoon_geometry_normal.hlsl"
#include "./vrmc_materials_mtoon_lighting_unity.hlsl"
#include "./vrmc_materials_mtoon_lighting_mtoon.hlsl"

half4 MToonShadowCasterFragment(const FragmentInput fragmentInput) : SV_Target
{
    const Varyings input = fragmentInput.varyings;
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    // Get MToon UV (with UVAnimation)
    const float2 uv = GetMToonGeometry_Uv(input.uv);

    // Get LitColor with Alpha
    const half4 litColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
    GetMToonGeometry_Alpha(litColor);

    return 0;
}

#endif

#endif