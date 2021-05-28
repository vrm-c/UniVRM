Shader "VRM10/vrmc_materials_mtoon"
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
        _Color ("pbrMetallicRoughness.baseColorFactor", Color) = (1,1,1,1) // Unity specified name
        _MainTex ("pbrMetallicRoughness.baseColorTexture", 2D) = "white" {} // Unity specified name
        _ShadeColor ("mtoon.shadeColorFactor", Color) = (1, 1, 1, 1)
        _ShadeTex ("mtoon.shadeMultiplyTexture", 2D) = "white" {}
        [Normal] _BumpMap ("normalTexture", 2D) = "bump" {} // Unity specified name
        _BumpScale ("normalTexture.scale", Float) = 1.0 // Unity specified name
        _ShadingShiftColor ("mtoon.shadingShiftFactor", Range(0, 1)) = 0
        _ShadingShiftTex ("mtoon.shadingShiftTexture", 2D) = "black" {} // channel R
        _ShadingShiftTexScale ("mtoon.shadingShiftTexture.scale", Float) = 1
        _ShadingToonyColor ("mtoon.shadingToonyFactor", Range(0, 1)) = 0.9
//        _ShadingToonyTex ("mtoon.shadingToonyTexture", 2D) = "black" {} // parameter texture // need?
//        _ShadingToonyTexScale ("mtoon.shadingToonyTexture.scale", Float) = 1 // need?

        // GI
        _GiEqualization ("mtoon.giEqualizationFactor", Range(0, 1)) = 0.9

        // Emission
        _EmissionColor ("emissiveFactor", Color) = (0, 0, 0) // Unity specified name
        _EmissionMap ("emissiveTexture", 2D) = "white" {} // Unity specified name

        // Rim Lighting
        _RimMatcapTex ("mtoon.matcapTexture", 2D) = "black" {}
        _RimColor ("mtoon.parametricRimColorFactor", Color) = (0, 0, 0)
        _RimFresnelPower ("mtoon.parametricRimFresnelPowerFactor", Float) = 5.0
        _RimLift ("mtoon.parametricRimLiftFactor", Float) = 0
        _RimTex ("mtoon.rimMultiplyTexture", 2D) = "white" {}
        _RimLightingMix ("mtoon.rimLightingMixFactor", Float) = 1

        // Outline
        _OutlineWidthMode ("mtoon.outlineWidthMode", Int) = 0
        _OutlineWidth ("mtoon.outlineWidthFactor", Float) = 0
        _OutlineWidthTex ("mtoon.outlineWidthMultiplyTexture", 2D) = "white" {} // channel G
        _OutlineColor ("mtoon.outlineColorFactor", Color) = (0, 0, 0)
        _OutlineLightingMix ("mtoon.outlineLightingMixFactor", Float) = 1 // default 0

        // UV Animation
        _UvAnimMaskTex ("mtoon.uvAnimationMaskTexture", 2D) = "white" {} // channel B
        _UvAnimScrollXSpeed ("mtoon.uvAnimationScrollXSpeedFactor", Float) = 0
        _UvAnimScrollYSpeed ("mtoon.uvAnimationScrollYSpeedFactor", Float) = 0
        _UvAnimRotationSpeed ("mtoon.uvAnimationRotationSpeedFactor", Float) = 0

        // Unity ShaderPass Mode
        [HideInInspector] _M_CullMode ("_CullMode", Float) = 2.0
        [HideInInspector] _M_SrcBlend ("_SrcBlend", Float) = 1.0
        [HideInInspector] _M_DstBlend ("_DstBlend", Float) = 0.0
        [HideInInspector] _M_ZWrite ("_ZWrite", Float) = 1.0
        [HideInInspector] _M_AlphaToMask ("_AlphaToMask", Float) = 0.0

        // etc
        [HideInInspector] _M_DebugMode ("_DebugMode", Float) = 0.0
    }

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

            CGPROGRAM
            #pragma target 3.0
            #pragma shader_feature _ MTOON_DEBUG_NORMAL MTOON_DEBUG_LITSHADERATE
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON
            #include "./vrmc_materials_mtoon_sm3.hlsl"
            #pragma vertex vert_forward_base
            #pragma fragment frag_forward
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
//            #pragma multi_compile_instancing
            ENDCG
        }


        // Built-in Forward Base Outline Pass
        Pass
        {
            Name "FORWARD_BASE_ONLY_OUTLINE"
            Tags { "LightMode" = "ForwardBase" }

            Cull Front
            Blend [_M_SrcBlend] [_M_DstBlend]
            ZWrite [_M_ZWrite]
            ZTest LEqual
            Offset 1, 1
            BlendOp Add, Max
            AlphaToMask [_M_AlphaToMask]

            CGPROGRAM
            #pragma target 3.0
            #pragma shader_feature _ MTOON_DEBUG_NORMAL MTOON_DEBUG_LITSHADERATE
            #pragma multi_compile _ MTOON_OUTLINE_WIDTH_WORLD MTOON_OUTLINE_WIDTH_SCREEN
            #pragma multi_compile _ MTOON_OUTLINE_COLOR_FIXED MTOON_OUTLINE_COLOR_MIXED
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON
            #define MTOON_CLIP_IF_OUTLINE_IS_NONE
            #include "./vrmc_materials_mtoon_sm3.hlsl"
            #pragma vertex vert_forward_base_outline
            #pragma fragment frag_forward
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
//            #pragma multi_compile_instancing
            ENDCG
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

            CGPROGRAM
            #pragma target 3.0
            #pragma shader_feature _ MTOON_DEBUG_NORMAL MTOON_DEBUG_LITSHADERATE
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON
            #define MTOON_FORWARD_ADD
            #include "./vrmc_materials_mtoon_sm3.hlsl"
            #pragma vertex vert_forward_add
            #pragma fragment frag_forward
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            ENDCG
        }

        //  Built-in Shadow Rendering Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull [_M_CullMode]
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0
            #pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma multi_compile_shadowcaster
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
            #include "UnityStandardShadow.cginc"
            ENDCG
        }
    }

    Fallback "Unlit/Texture"
    CustomEditor "MToon.MToonInspector"
}
