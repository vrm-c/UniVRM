Shader "VRM10/MToon10"
{
    Properties
    {
        // Rendering
        _AlphaMode ("alphaMode", Int) = 0
        _TransparentWithZWrite ("mtoon.transparentWithZWrite", Int) = 0
        _Cutoff ("alphaCutoff", Range(0, 1)) = 0.5 // Unity specified name
        _RenderQueueOffset ("mtoon.renderQueueOffsetNumber", Int) = 0
        _DoubleSided ("doubleSided", Int) = 0

        // Lighting
        _Color ("pbrMetallicRoughness.baseColorFactor", Color) = (1, 1, 1, 1) // Unity specified name
        _MainTex ("pbrMetallicRoughness.baseColorTexture", 2D) = "white" {} // Unity specified name
        _ShadeColor ("mtoon.shadeColorFactor", Color) = (1, 1, 1, 1)
        _ShadeTex ("mtoon.shadeMultiplyTexture", 2D) = "white" {}
        [Normal] _BumpMap ("normalTexture", 2D) = "bump" {} // Unity specified name
        _BumpScale ("normalTexture.scale", Float) = 1.0 // Unity specified name
        _ShadingShiftFactor ("mtoon.shadingShiftFactor", Range(-1, 1)) = -0.05
        _ShadingShiftTex ("mtoon.shadingShiftTexture", 2D) = "black" {} // channel R
        _ShadingShiftTexScale ("mtoon.shadingShiftTexture.scale", Float) = 1
        _ShadingToonyFactor ("mtoon.shadingToonyFactor", Range(0, 1)) = 0.95

        // GI
        _GiEqualization ("mtoon.giEqualizationFactor", Range(0, 1)) = 0.9

        // Emission
        [HDR] _EmissionColor ("emissiveFactor", Color) = (0, 0, 0, 1) // Unity specified name
        _EmissionMap ("emissiveTexture", 2D) = "white" {} // Unity specified name

        // Rim Lighting
        _MatcapColor ("mtoon.matcapFactor", Color) = (1, 1, 1, 1)
        _MatcapTex ("mtoon.matcapTexture", 2D) = "black" {}
        _RimColor ("mtoon.parametricRimColorFactor", Color) = (0, 0, 0, 1)
        _RimFresnelPower ("mtoon.parametricRimFresnelPowerFactor", Range(0, 100)) = 5.0
        _RimLift ("mtoon.parametricRimLiftFactor", Range(0, 1)) = 0
        _RimTex ("mtoon.rimMultiplyTexture", 2D) = "white" {}
        _RimLightingMix ("mtoon.rimLightingMixFactor", Range(0, 1)) = 1

        // Outline
        _OutlineWidthMode ("mtoon.outlineWidthMode", Int) = 0
        [PowerSlider(2.2)] _OutlineWidth ("mtoon.outlineWidthFactor", Range(0, 0.05)) = 0
        _OutlineWidthTex ("mtoon.outlineWidthMultiplyTexture", 2D) = "white" {} // channel G
        _OutlineColor ("mtoon.outlineColorFactor", Color) = (0, 0, 0, 1)
        _OutlineLightingMix ("mtoon.outlineLightingMixFactor", Range(0, 1)) = 1

        // UV Animation
        _UvAnimMaskTex ("mtoon.uvAnimationMaskTexture", 2D) = "white" {} // channel B
        _UvAnimScrollXSpeed ("mtoon.uvAnimationScrollXSpeedFactor", Float) = 0
        _UvAnimScrollYSpeed ("mtoon.uvAnimationScrollYSpeedFactor", Float) = 0
        _UvAnimRotationSpeed ("mtoon.uvAnimationRotationSpeedFactor", Float) = 0

        // Unity ShaderPass Mode
        _M_CullMode ("_CullMode", Float) = 2.0
        _M_SrcBlend ("_SrcBlend", Float) = 1.0
        _M_DstBlend ("_DstBlend", Float) = 0.0
        _M_ZWrite ("_ZWrite", Float) = 1.0
        _M_AlphaToMask ("_AlphaToMask", Float) = 0.0

        // etc
        _M_DebugMode ("_DebugMode", Float) = 0.0

        // for Editor
        _M_EditMode ("_EditMode", Float) = 0.0
    }

    // Shader Model 3.0
    SubShader
    {
        Tags { "RenderType" = "Opaque"  "Queue" = "Geometry" }

        // Built-in Forward Base Pass
        Pass
        {
            Name "FORWARD_BASE"
            Tags { "LightMode" = "ForwardBase" }

            Cull [_M_CullMode]
            Blend [_M_SrcBlend] [_M_DstBlend]
            ZWrite [_M_ZWrite]
            ZTest LEqual
            BlendOp Add, Max
            AlphaToMask [_M_AlphaToMask]

            HLSLPROGRAM
            #pragma target 3.0

            // Unity defined keywords
            #pragma multi_compile_fwdbase nolightmap nodynlightmap nodirlightmap novertexlight
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma multi_compile __ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma multi_compile __ _NORMALMAP
            #pragma multi_compile __ _MTOON_EMISSIVEMAP
            #pragma multi_compile __ _MTOON_RIMMAP
            #pragma multi_compile __ _MTOON_PARAMETERMAP

            #pragma vertex MToonVertex
            #pragma fragment MToonFragment

            #include "./vrmc_materials_mtoon_forward_vertex.hlsl"
            #include "./vrmc_materials_mtoon_forward_fragment.hlsl"
            ENDHLSL
        }

        // Built-in Forward Base Pass: OUTLINE
        Pass
        {
            Name "FORWARD_BASE_OUTLINE"
            Tags { "LightMode" = "ForwardBase" }

            Cull Front
            Blend [_M_SrcBlend] [_M_DstBlend]
            ZWrite [_M_ZWrite]
            ZTest LEqual
            Offset 1, 1
            BlendOp Add, Max
            AlphaToMask [_M_AlphaToMask]

            HLSLPROGRAM
            #pragma target 3.0

            // Unity defined keywords
            #pragma multi_compile_fwdbase nolightmap nodynlightmap nodirlightmap novertexlight
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma multi_compile __ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma multi_compile __ _NORMALMAP
            #pragma multi_compile __ _MTOON_EMISSIVEMAP
            #pragma multi_compile __ _MTOON_RIMMAP
            #pragma multi_compile __ _MTOON_PARAMETERMAP
            #pragma multi_compile __ _MTOON_OUTLINE_WORLD _MTOON_OUTLINE_SCREEN

            #pragma vertex MToonVertex
            #pragma fragment MToonFragment

            #define MTOON_PASS_OUTLINE

            #include "./vrmc_materials_mtoon_forward_vertex.hlsl"
            #include "./vrmc_materials_mtoon_forward_fragment.hlsl"
            ENDHLSL
        }

        // Built-in Forward Add Pass
        Pass
        {
            Name "FORWARD_ADD"
            Tags { "LightMode" = "ForwardAdd" }

            Cull [_M_CullMode]
            Blend [_M_SrcBlend] One
            ZWrite Off
            ZTest LEqual
            BlendOp Add, Max
            AlphaToMask [_M_AlphaToMask]

            HLSLPROGRAM
            #pragma target 3.0

            // Unity defined keywords
            #pragma multi_compile_fwdadd_fullshadows nolightmap nodynlightmap nodirlightmap novertexlight
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma multi_compile __ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma multi_compile __ _NORMALMAP
            #pragma multi_compile __ _MTOON_EMISSIVEMAP
            #pragma multi_compile __ _MTOON_RIMMAP
            #pragma multi_compile __ _MTOON_PARAMETERMAP

            #pragma vertex MToonVertex
            #pragma fragment MToonFragment

            #include "./vrmc_materials_mtoon_forward_vertex.hlsl"
            #include "./vrmc_materials_mtoon_forward_fragment.hlsl"
            ENDHLSL
        }

        //  Shadow rendering pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull [_M_CullMode]
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // Unity defined keywords
            #pragma multi_compile_shadowcaster nolightmap nodynlightmap nodirlightmap novertexlight
            #pragma multi_compile_instancing

            #pragma multi_compile __ _ALPHATEST_ON _ALPHABLEND_ON

            // Use unity standard shadow implementation.
            // internal usage:
            //     keywords: _ALPHATEST_ON _ALPHABLEND_ON
            //     variables: _MainTex.a _Color.a _Cutoff
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
    CustomEditor "VRM10.MToon10.Editor.MToonInspector"
}
