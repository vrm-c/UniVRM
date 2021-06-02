#ifndef VRMC_MATERIALS_MTOON_UV_INCLUDED
#define VRMC_MATERIALS_MTOON_UV_INCLUDED

#include <UnityCG.cginc>
#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"

float2 GetMToonUv(const float2 geometryUv)
{
    // get raw uv with _MainTex_ST
    const float2 uvRaw = TRANSFORM_TEX(geometryUv, _MainTex);

    if (MToon_IsUvAnimationOn())
    {
        const float uvAnimationCoef = UNITY_SAMPLE_TEX2D(_UvAnimMaskTex, uvRaw).b * _Time.y;
        const float2 uvAnimationAdd = uvAnimationCoef * float2(_UvAnimScrollXSpeed, _UvAnimScrollYSpeed);
        const float rotateRad = uvAnimationCoef * _UvAnimRotationSpeed * PI_2;
        const float2 rotatePivot = float2(0.5, 0.5);
        return mul(float2x2(cos(rotateRad), -sin(rotateRad), sin(rotateRad), cos(rotateRad)), uvRaw + uvAnimationAdd - rotatePivot) + rotatePivot;
    }
    else
    {
        return uvRaw;
    }
}

#endif
