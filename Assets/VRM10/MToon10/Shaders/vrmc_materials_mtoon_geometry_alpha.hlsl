#ifndef VRMC_MATERIALS_MTOON_GEOMETRY_ALPHA_INCLUDED
#define VRMC_MATERIALS_MTOON_GEOMETRY_ALPHA_INCLUDED

#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"

inline half GetMToonGeometry_Alpha(half4 litColor)
{
    if (MToon_IsAlphaTestOn())
    {
        const half rawAlpha = litColor.a;
        const half tmpAlpha = (rawAlpha - _Cutoff) / max(fwidth(rawAlpha), 0.00001) + 0.5; // Alpha to Coverage
        clip(tmpAlpha - _Cutoff);
        return 1.0;
    }
    else if (MToon_IsAlphaBlendOn())
    {
        const half alpha = litColor.a;
        clip(alpha - EPS_COL);
        return alpha;
    }
    else
    {
        return 1.0;
    }
}

#endif
