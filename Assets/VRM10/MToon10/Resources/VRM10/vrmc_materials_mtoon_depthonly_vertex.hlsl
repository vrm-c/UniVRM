#ifndef VRMC_MATERIALS_MTOON_DEPTHONLY_VERTEX_INCLUDED
#define VRMC_MATERIALS_MTOON_DEPTHONLY_VERTEX_INCLUDED

#ifdef MTOON_URP

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./vrmc_materials_mtoon_define.hlsl"
#include "./vrmc_materials_mtoon_utility.hlsl"
#include "./vrmc_materials_mtoon_input.hlsl"
#include "./vrmc_materials_mtoon_attribute.hlsl"
#include "./vrmc_materials_mtoon_geometry_vertex.hlsl"

Varyings MToonDepthOnlyVertex(const Attributes v)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.pos = TransformObjectToHClip(v.vertex.xyz);
    output.uv = v.texcoord0;

    return output;
}

#endif

#endif