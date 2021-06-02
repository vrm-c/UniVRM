#ifndef VRMC_MATERIALS_MTOON_DEFINE_INCLUDED
#define VRMC_MATERIALS_MTOON_DEFINE_INCLUDED

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
inline bool MToon_IsUvAnimationOn()
{
#if defined(_UVANIMATION)
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

#endif
