#ifndef VRMC_MATERIALS_MTOON_FORWARD_VERTEX_INCLUDED
#define VRMC_MATERIALS_MTOON_FORWARD_VERTEX_INCLUDED

#include <UnityCG.cginc>
#include <AutoLight.cginc>
#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"
#include "./vrmc_materials_mtoon_geometry_vertex.hlsl"

Varyings MToonVertex(const Attributes v) // v is UnityCG macro specified name.
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv = TRANSFORM_TEX(v.texcoord0, _MainTex);

    if (MToon_IsOutlinePass())
    {
        const VertexPositionInfo position = MToon_GetOutlineVertex(v.vertex.xyz, normalize(v.normalOS), output.uv);
        output.pos = position.positionCS;
        output.positionWS = position.positionWS;
        output.normalWS = UnityObjectToWorldNormal(-v.normalOS);
    }
    else
    {
        const VertexPositionInfo position = MToon_GetVertex(v.vertex.xyz);
        output.pos = position.positionCS;
        output.positionWS = position.positionWS;
        output.normalWS = UnityObjectToWorldNormal(v.normalOS);
    }

    output.viewDirWS = MToon_GetWorldSpaceNormalizedViewDir(output.positionWS);

#if defined(_NORMALMAP)
    const half tangentSign = v.tangentOS.w * unity_WorldTransformParams.w;
    output.tangentWS = half4(UnityObjectToWorldDir(v.tangentOS), tangentSign);
#endif

    UNITY_TRANSFER_FOG(output, output.positionWS);
    UNITY_TRANSFER_LIGHTING(output, v.texcoord1.xy);

    return output;
}

#endif
