#ifndef VRMC_MATERIALS_MTOON_FORWARD_VERTEX_INCLUDED
#define VRMC_MATERIALS_MTOON_FORWARD_VERTEX_INCLUDED

#include <UnityCG.cginc>
#include <AutoLight.cginc>
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"

Varyings MToonVertex(const Attributes v) // v is UnityCG macro specified name.
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.pos = UnityObjectToClipPos(v.vertex);
    output.positionWS = mul(unity_ObjectToWorld, v.vertex);
    output.uv = TRANSFORM_TEX(v.texcoord0, _MainTex);

    output.normalWS = UnityObjectToWorldNormal(v.normalOS);
#if defined(_NORMALMAP)
    const half tangentSign = v.tangentOS.w * unity_WorldTransformParams.w;
    output.tangentWS = half4(UnityObjectToWorldDir(v.tangentOS), tangentSign);
#endif

    output.viewDirWS = MToon_GetWorldSpaceNormalizedViewDir(output.positionWS);

    UNITY_TRANSFER_FOG(output, output.positionWS);
    UNITY_TRANSFER_LIGHTING(output, v.texcoord1.xy);

    return output;
}

#endif
