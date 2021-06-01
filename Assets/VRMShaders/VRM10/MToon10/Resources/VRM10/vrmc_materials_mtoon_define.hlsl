#ifndef VRMC_MATERIALS_MTOON_DEFINE_INCLUDED
#define VRMC_MATERIALS_MTOON_DEFINE_INCLUDED

// define
static const float PI_2 = 6.28318530718;
static const float EPS_COL = 0.00001;

inline half3 mtoon_linearstep(half3 start, half3 end, half t)
{
    return min(max((t - start) / (end - start), 0.0), 1.0);
}

#endif
