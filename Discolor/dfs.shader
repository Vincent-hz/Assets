// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/dfs" {
		Properties{
			_Color("Color", Color) = (1,1,1,1)
			_Color1("Color1", Color) = (1,0,0,0)
			_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.44
			_Metallic("Metallic", Range(0,1)) = 0.21
			_Clip("Clip", float) = 0
			[KeywordEnum(None, Left, Up, Forward)]_Mode("Mode", Float) = 0
		}
			SubShader{
			Tags{ "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows vertex:vert

			// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

			sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 localPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Clip;
		float _Mode;
		float4 _Color1;
		void vert(inout appdata_full vertexNumbers, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = vertexNumbers.vertex;
		}

		void surf(Input vertexNumbers, inout SurfaceOutputStandard o) {

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, vertexNumbers.uv_MainTex) * _Color;
			if (vertexNumbers.localPos.x >= _Clip && _Mode == 1 || vertexNumbers.localPos.y >= _Clip && _Mode == 2 || vertexNumbers.localPos.z >= _Clip && _Mode == 3)
			{
				//clip(-1);
				o.Albedo = _Color1;
			}
			else
			{
				o.Albedo = c.rgb;
			}
		
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
		}
			FallBack "Diffuse"
	}