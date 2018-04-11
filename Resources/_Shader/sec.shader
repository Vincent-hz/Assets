Shader "Custom/sec"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float luma(float3 color)
			{
			    return 0.2126*color.r + 0.7152*color.g + 0.0722*color.b;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			    float dx = 1/_ScreenParams.x;
				float dy = 1/_ScreenParams.y;

				float s00 = luma(tex2D(_MainTex,i.uv + float2(-dx,-dy)).rgb);
				float s01 = luma(tex2D(_MainTex,i.uv + float2(0,-dy)).rgb);
				float s02 = luma(tex2D(_MainTex,i.uv + float2(dx,-dy)).rgb);
				float s10 = luma(tex2D(_MainTex,i.uv + float2(-dx,0)).rgb);
				float s12 = luma(tex2D(_MainTex,i.uv + float2(dx,0)).rgb);
				float s20 = luma(tex2D(_MainTex,i.uv + float2(-dx,dy)).rgb);
				float s21 = luma(tex2D(_MainTex,i.uv + float2(0,dy)).rgb);
				float s22 = luma(tex2D(_MainTex,i.uv + float2(dx,dy)).rgb);
				float sx = s00 + 2*s10 + s20 - (s02 + 2*s12 + s22);
				float sy = s00 + 2*s01 + s02 - (s20 + 2*s21 + s22);
				float dist = sx*sx + sy*sy;
				if(dist > 0.0001)
				    return float4(1,0.4,0,1);
				else
				    return float4(1,1,1,0);
			}
			ENDCG
		}
	}
}
