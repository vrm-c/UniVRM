#ifndef VRMC_MATERIALS_MTOON_FORWARD_VERTEX_INCLUDED
#define VRMC_MATERIALS_MTOON_FORWARD_VERTEX_INCLUDED

#include "./vrmc_materials_mtoon_render_pipeline.hlsl"
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

    output.uv = v.texcoord0;

    if (MToon_IsOutlinePass())
    {
        const VertexPositionInfo position = MToon_GetOutlineVertex(v.vertex.xyz, normalize(v.normalOS), output.uv);
        output.pos = position.positionCS;
        output.positionWS = position.positionWS.xyz;

        output.normalWS = MToon_TransformObjectToWorldNormal(-v.normalOS);
    }
    else
    {
        const VertexPositionInfo position = MToon_GetVertex(v.vertex.xyz);
        output.pos = position.positionCS;
        output.positionWS = position.positionWS.xyz;

        output.normalWS = MToon_TransformObjectToWorldNormal(v.normalOS);
    }

    output.viewDirWS = MToon_GetWorldSpaceNormalizedViewDir(output.positionWS);

#if defined(_NORMALMAP)
    const half tangentSign = v.tangentOS.w * unity_WorldTransformParams.w;
    output.tangentWS = half4(MToon_TransformObjectToWorldDir(v.tangentOS), tangentSign);
#endif

    MTOON_TRANSFER_FOG_AND_LIGHTING(output, output.pos, v.texcoord1.xy, v.vertex.xyz);

    return output;
}

#endif