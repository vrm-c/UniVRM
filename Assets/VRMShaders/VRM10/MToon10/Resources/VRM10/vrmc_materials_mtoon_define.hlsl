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
inline bool MToon_IsUvAnimationOn()
{
#if defined(_MTOON_UVANIMATION)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsRimOn()
{
#if defined(_MTOON_RIM)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsEmissiveOn()
{
#if defined(_MTOON_EMISSIVE)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsNormalMapOn()
{
#if defined(_MTOON_NORMALMAP)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsShadingMapOn()
{
#if defined(_MTOON_SHADINGMAP)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsOutlineMapOn()
{
#if defined(_MTOON_OUTLINEMAP)
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
