Shader "Custom/vdx"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha
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
//				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _Source;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f o) : SV_Target
			{
			//blur
//			    fixed3 color = fixed3(0,0,0);
//			    float Offsets[5] ={ -2.0, -1.0, 0, 1.0, 2.0 };
//				// sample the texture
//				for(int i = 0; i < 5; ++i)
//				{
//				    for(int j = 0; j < 5; ++j)
//					{
//					    float2 tc = o.uv;
//						tc.x = o.uv.x + Offsets[j] / float(_width);
//                        tc.y = o.uv.y + Offsets[i] / float(_height);
//						color += tex2D(_MainTex,tc);
//					}
//				}
//				color /= 25;
                
				fixed2 uv = o.uv;
				uv.y = 1-uv.y;
				fixed4 c = tex2D(_Source,uv);
                if(c.r == 1 && c.g ==1 &&c.b == 1)
				    return fixed4(tex2D(_MainTex,o.uv));
				else
				    return c;
			}
			ENDCG
		}
	}
}
