#ifndef VRMC_MATERIALS_MTOON_FORWARD_VERTEX_INCLUDED
#define VRMC_MATERIALS_MTOON_FORWARD_VERTEX_INCLUDED

#ifdef MTOON_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#else
#include <UnityCG.cginc>
#include <AutoLight.cginc>
#endif

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
        output.positionWS = position.positionWS;

        #ifdef MTOON_URP
        output.normalWS = TransformObjectToWorldNormal(-v.normalOS);
        #else
        output.normalWS = UnityObjectToWorldNormal(-v.normalOS);
        #endif
    }
    else
    {
        const VertexPositionInfo position = MToon_GetVertex(v.vertex.xyz);
        output.pos = position.positionCS;
        output.positionWS = position.positionWS;

        #ifdef MTOON_URP
        output.normalWS = TransformObjectToWorldNormal(v.normalOS);
        #else
        output.normalWS = UnityObjectToWorldNormal(v.normalOS);
        #endif
    }

    output.viewDirWS = MToon_GetWorldSpaceNormalizedViewDir(output.positionWS);

#if defined(_NORMALMAP)
    const half tangentSign = v.tangentOS.w * unity_WorldTransformParams.w;
    output.tangentWS = half4(UnityObjectToWorldDir(v.tangentOS), tangentSign);
#endif

#ifdef MTOON_URP
    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

    OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    half3 vertexLight = VertexLighting(output.positionWS, output.normalWS);
    half fogFactor = ComputeFogFactor(output.pos.z);
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

#else
    UNITY_TRANSFER_FOG(output, output.pos);
    UNITY_TRANSFER_LIGHTING(output, v.texcoord1.xy);
#endif

    return output;
}

#endif