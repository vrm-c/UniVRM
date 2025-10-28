Shader "CloverUI/OverlayGizmo"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _RimFactor ("Rim", Float) = 0
        [PowerSlider(4.0)] _RimFresnelPower ("Rim Fresnel Power", Range(0, 100)) = 1
        _RimLift ("Rim Lift", Range(0, 1)) = 0
        _RimReverse ("Rim Reverse", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Overlay" "Queue"="Overlay" }
        LOD 100

        Pass
        {
            Lighting Off
            ZTest Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            half4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _RimFactor;
            half _RimFresnelPower;
            half _RimLift;
            half _RimReverse;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 worldNormal = normalize(i.normal);
                float3 worldView = normalize(lerp(_WorldSpaceCameraPos.xyz - i.posWorld.xyz, UNITY_MATRIX_V[2].xyz, unity_OrthoParams.w));
                
                half4 col = _Color * tex2D(_MainTex, i.uv);
                
                half rim = pow(saturate(dot(worldNormal, worldView)), _RimFresnelPower);
                rim = (1 - _RimReverse) * rim + _RimReverse * (1 - rim);
                rim += _RimLift;
                col = (1 - _RimFactor) * col + _RimFactor * (col * rim);
                
                return col;
            }
            ENDCG
        }
    }
}
