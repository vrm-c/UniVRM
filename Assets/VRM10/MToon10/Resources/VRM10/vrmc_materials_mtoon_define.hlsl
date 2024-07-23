#ifndef VRMC_MATERIALS_MTOON_DEFINE_INCLUDED
#define VRMC_MATERIALS_MTOON_DEFINE_INCLUDED

// TEX2D with LOD
#ifndef UNITY_SAMPLE_TEX2D_LOD
    #define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord,lod)
#endif

// VFACE
#ifndef MTOON_FRONT_FACE_SEMANTIC
    #define MTOON_FRONT_FACE_SEMANTIC VFACE
#endif
#ifndef MTOON_FRONT_FACE_TYPE
    // from https://docs.unity3d.com/Manual/SL-ShaderSemantics.html
    #ifdef MTOON_URP
        #define MTOON_FRONT_FACE_TYPE float
    #else
        #define MTOON_FRONT_FACE_TYPE fixed
    #endif
#endif
#ifndef MTOON_IS_FRONT_VFACE
    #define MTOON_IS_FRONT_VFACE(VAL, FRONT, BACK) ((VAL > 0.0) ? (FRONT) : (BACK))
#endif

// Compile-time constant
// EXPERIMENTAL
inline bool MToon_IsPbrCorrectOn()
{
    return false;
}

// Compile-time constant
inline bool MToon_IsForwardBasePass()
{
#if defined(MTOON_URP)
    return true;
#elif defined(UNITY_PASS_FORWARDBASE)
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
inline bool MToon_IsAlphaTestOn()
{
#if defined(_ALPHATEST_ON)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsAlphaBlendOn()
{
#if defined(_ALPHABLEND_ON)
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
 #if defined(_MTOON_OUTLINE_WORLD)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsOutlineModeScreenCoordinates()
{
 #if defined(_MTOON_OUTLINE_SCREEN)
    return true;
#else
    return false;
#endif
}

// Compile-time constant
inline bool MToon_IsOutlineModeDisabled()
{
 #if defined(_MTOON_OUTLINE_WORLD) || defined(_MTOON_OUTLINE_SCREEN)
    return false;
#else
    return true;
#endif
}

// Compile-time constant
inline bool MToon_UseStrictMode()
{
 #if defined(_MTOON_USE_STRICT_MODE)
    return true;
#else
    return false;
#endif
}

#endif
