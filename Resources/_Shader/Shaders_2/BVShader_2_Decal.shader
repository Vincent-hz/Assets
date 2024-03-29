
Shader "BVShader/2_Decal" {
    Properties {
        _DiffuseMap ("DiffuseMap", 2D) = "white" {}
	    _Transpose ("Transpose", Int) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
			[KeywordEnum(None, Left, Up, Forward)]_Mode("Mode", Float) = 0
			_Clip("Clip", float) = 0
			_ChangeColor("ChangeColor", Color) = (1,0,0,1)
			_Cutoff("_Cutoff ",Range(0,1)) = 1
			_Trace("Trace Range",Range(0,0.5)) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
			uniform sampler2D _DiffuseMap; uniform float4 _DiffuseMap_ST;
			uniform float4x4 _DiffuseMatrix;
			uniform int _Transpose;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
				float2 uv = float2(0, 0);
		//		uv.x = i.uv0.x * _DiffuseMatrix[0].x + i.uv0.y * _DiffuseMatrix[0].y + _DiffuseMatrix[0].w;
		//		uv.y = i.uv0.x * _DiffuseMatrix[1].x + i.uv0.y * _DiffuseMatrix[1].y + _DiffuseMatrix[1].w;
				if(_Transpose == 1)
				    uv.xy = i.uv0.yx * _DiffuseMap_ST.xy + _DiffuseMap_ST.zw;
				else
					uv.xy = i.uv0.xy * _DiffuseMap_ST.xy + _DiffuseMap_ST.zw;
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 _DiffuseMap_var = tex2D(_DiffuseMap, uv);
                float3 diffuseColor = _DiffuseMap_var.rgb;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);

				if (uv.x < 0 || uv.x > 1)
					finalRGBA = float4(finalRGBA.xyz, 0);

				if (uv.y < 0 || uv.y > 1)
					finalRGBA = float4(finalRGBA.xyz, 0);

                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
			uniform sampler2D _DiffuseMap; uniform float4 _DiffuseMap_ST;
			uniform float4x4 _DiffuseMatrix;
			uniform int _Transpose;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
				float2 uv = float2(0, 0);
		//		uv.x = i.uv0.x * _DiffuseMatrix[0].x + i.uv0.y * _DiffuseMatrix[0].y + _DiffuseMatrix[0].w;
		//		uv.y = i.uv0.x * _DiffuseMatrix[1].x + i.uv0.y * _DiffuseMatrix[1].y + _DiffuseMatrix[1].w;
				if (_Transpose == 1)
					uv.xy = i.uv0.yx * _DiffuseMap_ST.xy + _DiffuseMap_ST.zw;
				else
					uv.xy = i.uv0.xy * _DiffuseMap_ST.xy + _DiffuseMap_ST.zw;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
				float4 _DiffuseMap_var = tex2D(_DiffuseMap, uv);
                float3 diffuseColor = _DiffuseMap_var.rgb;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor * 0.5,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);

				if (uv.x < 0 || uv.x > 1)
					finalRGBA = float4(finalRGBA.xyz, 0);

				if (uv.y < 0 || uv.y > 1)
					finalRGBA = float4(finalRGBA.xyz, 0);

                return finalRGBA;
            }
            ENDCG
        }
			CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff vertex:vert  
				fixed _Clip;
			float4 _ChangeColor;
			float _Mode;
			fixed _Trace;
			struct Input {
				float4 vertColor;
			};
			void vert(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.vertColor = v.color;
				if (_Mode == 0)
				{
					o.vertColor.a = 0;
				}
				if (_Mode == 1)
				{
					if (v.vertex.x<_Clip)
					{
						o.vertColor.a = 0;
					}
					else
					{
						if (v.vertex.x < _Clip + _Trace)
						{
							o.vertColor.a = 0.5;
						}
					}
				}
				if (_Mode == 2)
				{
					if (v.vertex.y<_Clip)
					{
						o.vertColor.a = 0;
					}
					else
					{
						if (v.vertex.y < _Clip + _Trace)
						{
							o.vertColor.a = 0.5;
						}
					}
				}
				if (_Mode == 3)
				{
					if (v.vertex.z<_Clip)
					{
						o.vertColor.a = 0;
					}
					else
					{
						if (v.vertex.z < _Clip + _Trace)
						{
							o.vertColor.a = 0.5;
						}
					}
				}

			}
			void surf(Input IN, inout SurfaceOutput o)
			{
				o.Albedo = _ChangeColor;
				o.Alpha = IN.vertColor.a;
			}
			ENDCG
    }
    FallBack "Diffuse"
}
