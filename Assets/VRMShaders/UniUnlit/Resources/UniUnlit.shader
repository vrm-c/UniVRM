Shader "UniGLTF/UniUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", COLOR) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

        [HideInInspector] _BlendMode ("_BlendMode", Float) = 0.0
        [HideInInspector] _CullMode ("_CullMode", Float) = 2.0
        [HideInInspector] _VColBlendMode ("_VColBlendMode", Float) = 0.0
        [HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1.0
        [HideInInspector] _DstBlend ("_DstBlend", Float) = 0.0
        [HideInInspector] _ZWrite ("_ZWrite", Float) = 1.0

        // VertexColor
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull [_CullMode]
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest LEqual
            BlendOp Add, Max
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma multi_compile _ _VERTEXCOL_MUL
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            #if defined(_VERTEXCOL_MUL)
                fixed4 color : COLOR;
            #endif
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            #if defined(_VERTEXCOL_MUL)
                fixed4 color : COLOR;
            #endif
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;
            half _Cutoff;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                
            #if defined(_VERTEXCOL_MUL)
                o.color = v.color;
            #endif
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                #if defined(_VERTEXCOL_MUL)
                    col *= i.color;
                #endif
                
                #if defined(_ALPHATEST_ON)
                    clip(col.a - _Cutoff);
                #endif
                
                #if !defined(_ALPHATEST_ON) && !defined(_ALPHABLEND_ON)
                    col.a = 1.0;
                #endif
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    CustomEditor "UniGLTF.UniUnlit.UniUnlitEditor"
    Fallback "Unlit/Texture"
}
