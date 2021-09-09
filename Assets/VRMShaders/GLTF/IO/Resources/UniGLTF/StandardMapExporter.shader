Shader "Hidden/UniGLTF/StandardMapExporter"
{
     Properties
     {
          _MainTex ("Texture", 2D) = "white" {}
          _UnitySmoothness ("Unity Smoothness Factor", Float) = 1.0
          _UnityMetallicSmoothTexture ("Unity Metallic Smoothness Texture", 2D) = "black" {}
          _UnityOcclusionTexture ("Unity Occlusion Texture", 2D) = "black" {}
     }
     SubShader
     {
          // No culling or depth
          Cull Off ZWrite Off ZTest Always

          Pass
          {
               CGPROGRAM
               #pragma vertex vert
               #pragma fragment frag

               #include "UnityCG.cginc"

               struct appdata
               {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
               };

               struct v2f
               {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
               };

               v2f vert (appdata v)
               {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
               }

               sampler2D _MainTex;
               half _UnitySmoothness;
               sampler2D _UnityMetallicSmoothTexture;
               sampler2D _UnityOcclusionTexture;

               fixed4 frag (v2f i) : SV_Target
               {
                    half4 occlusionTex = tex2D(_UnityOcclusionTexture, i.uv);
                    half occlusion = occlusionTex.g; // G: Unity Occlusion

                    half4 metallicRoughnessTex = tex2D(_UnityMetallicSmoothTexture, i.uv);
                    half smoothness = metallicRoughnessTex.a * _UnitySmoothness; // A: Unity Roughness
                    half metallic = metallicRoughnessTex.r; // R: Unity Metallic

                    fixed4 result;
                    result.r = occlusion;  // R: glTF Occlusion 
                    result.g = 1.0 - smoothness;  // G: glTF Roughness
                    result.b = metallic; // B: glTF Metallic
                    result.a = 1.0; // A: glTF not used

                    return result;
               }
               ENDCG
          }
     }
}
