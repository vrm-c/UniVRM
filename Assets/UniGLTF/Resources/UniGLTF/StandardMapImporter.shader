Shader "Hidden/UniGLTF/StandardMapImporter"
{
     Properties
     {
          _MainTex ("Texture", 2D) = "white" {}
          _GltfMetallicFactor ("glTF Metallic Factor", Float) = 0.0
          _GltfRoughnessFactor ("glTF Roughness Factor", Float) = 0.0
          _GltfMetallicRoughnessTexture ("glTF Metallic Roughness Texture", 2D) = "black" {}
          _GltfOcclusionTexture ("glTF Occlusion Texture", 2D) = "black" {}
          _IsLegacySquaredRoughness ("Is UniGLTF Legacy Squared Roughness", Float) = 0.0
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
               half _GltfMetallicFactor;
               half _GltfRoughnessFactor;
               sampler2D _GltfMetallicRoughnessTexture;
               sampler2D _GltfOcclusionTexture;
               half _IsLegacySquaredRoughness;

               fixed4 frag (v2f i) : SV_Target
               {
                    half4 metallicRoughnessTex = tex2D(_GltfMetallicRoughnessTexture, i.uv);
                    half4 occlusionTex = tex2D(_GltfOcclusionTexture, i.uv);

                    half occlusion = occlusionTex.r; // R: glTF Occlusion
                    half roughness = metallicRoughnessTex.g * _GltfRoughnessFactor; // G: glTF Roughness
                    half metallic = metallicRoughnessTex.b * _GltfMetallicFactor; // B: glTF Metallic

                    // MIGRATION code: legacy behaviour
                    if (_IsLegacySquaredRoughness == 1.0)
                    {
                         roughness = sqrt(roughness);
                    }

                    fixed4 result;
                    result.r = metallic;  // R: Unity Metallic
                    result.g = occlusion;  // G: Unity Occlusion
                    result.b = 0; // B: Unity Heightmap (no use)
                    result.a = 1.0 - roughness; // A: Unity Smoothness

                    return result;
               }
               ENDCG
          }
     }
}
