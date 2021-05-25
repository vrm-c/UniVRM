Shader "UniGLTF/NormalMapEncoder"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);

				half4 packedNormal;
				packedNormal.x = col.x;
				packedNormal.y = col.y;
				packedNormal.z = col.z;
				packedNormal.w = 1.0;

				// normalize as normal vector
				float3 normal;
				normal.xy = packedNormal.xy * 2 - 1;
				if (dot(normal.xy, normal.xy) > 1)
				{
					normal.xy = normalize(normal.xy);
				}
				normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));

				packedNormal.xyz = normal.xyz * 0.5 + 0.5;
				return packedNormal;
			}
			ENDCG
		}
	}
}

