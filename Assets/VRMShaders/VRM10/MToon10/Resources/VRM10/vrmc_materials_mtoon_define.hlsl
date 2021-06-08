#ifndef VRMC_MATERIALS_MTOON_DEFINE_INCLUDED
#define VRMC_MATERIALS_MTOON_DEFINE_INCLUDED

#ifndef UNITY_SAMPLE_TEX2D_LOD
    #define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord,lod)
#endif

// Compile-time constant
inline bool MToon_IsForwardBasePass()
{
#if defined(UNITY_PASS_FORWARDBASE)
    return true;
#elif defined(UNITY_PASS_FORWARDADD)
    return false;
#else
    // unexpected
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsOutlinePass()
{
#if defined(MTOON_PASS_OUTLINE)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsNormalMapOn()
{
#if defined(_NORMALMAP)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsEmissiveMapOn()
{
#if defined(_MTOON_EMISSIVEMAP)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsRimMapOn()
{
#if defined(_MTOON_RIMMAP)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsParameterMapOn()
{
#if defined(_MTOON_PARAMETERMAP)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsOutlineModeWorldCoordinates()
{
 #if defined(MTOON_OUTLINE_WIDTH_WORLD)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsOutlineModeScreenCoordinates()
{
 #if defined(MTOON_OUTLINE_WIDTH_SCREEN)
    return true;
#else
    return false;
#endif
}

#endif
