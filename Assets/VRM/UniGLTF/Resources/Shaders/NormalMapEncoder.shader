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

#if defined(UNITY_NO_DXT5nm)
				// This is a trick from UnpackNormal in UnityCG.cginc !!!!
				return col;
#endif

				half4 normal;
				normal.x = 1.0;
				normal.y = col.y;
				normal.z = 1.0;
				normal.w = col.x;

				return normal;
			}
			ENDCG
		}
	}
}

