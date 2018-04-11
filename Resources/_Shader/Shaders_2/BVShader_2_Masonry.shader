
Shader "BVShader/2_Masonry" {
    Properties {
        _DiffuseMap ("DiffuseMap", 2D) = "white" {}
        _SpecularGloss ("SpecularGloss", Float ) = 0
        _f0 ("f0", Range(0, 1)) = 0.5
        _f1 ("f1", Range(0, 1)) = 1
        _fresnelPower ("fresnelPower", Range(0, 10)) = 5
        _SpecularLevel ("SpecularLevel", Float ) = 0
        _SpecularColor ("SpecularColor", Color) = (1,1,1,1)
        _DiffuseColor ("DiffuseColor", Color) = (1,1,1,1)
        _DiffuseFade ("DiffuseFade", Range(0, 1)) = 0
			[KeywordEnum(None, Left, Up, Forward)]_Mode("Mode", Float) = 0
			_Clip("Clip", float) = 0
			_ChangeColor("ChangeColor", Color) = (1,0,0,1)
			_Cutoff("_Cutoff ",Range(0,1)) = 1
			_Trace("Trace Range",Range(0,0.5)) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 100
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            float4 myshadergeom( float3 H , float3 N , float3 L , float3 V ){
             float NL = saturate(dot(N, L)); 
            
                float NV = saturate(dot(N, V)); 
            
                float NH = saturate(dot(N, H)); 
            
                float VH = saturate(dot(V, H)); 
            
                return float4( NL, NV, NH, VH ); 
            }
            
            float3 LightL( float3 L , float3 N ){
            if(dot(N,L)>0)
               return L;
            else
               return -L;
            }
            
            float3 lambertianDiffuse( float4 NL_NV_NH_VH , float3 diffuseI ){
            const float oneOverPi = 1 / 3.141592654; 
            
                return NL_NV_NH_VH.x * diffuseI; // * oneOverPi; 
            }
            
            uniform sampler2D _DiffuseMap; uniform float4 _DiffuseMap_ST;
            float3 wardSpecularIso( float3 N , float3 H , float4 NL_NV_NH_VH , float glossiness ){
            const float EPSILON = 1e-6f;
            
                const float SOFTEN_MAX = 80.0f;
            
                const float FOUR_PI = 4.0f * 3.141592654f;
            
                float NLNV = NL_NV_NH_VH.x * NL_NV_NH_VH.y;
            
                if (NLNV < EPSILON) return 0.0f;
            
                float denom = FOUR_PI * sqrt(NLNV);
            
                float NH2 = NL_NV_NH_VH.z * NL_NV_NH_VH.z;
            
                float3 proj = H - NL_NV_NH_VH.z * N;
            
                float proj2 = dot(proj, proj);
            
                float gloss = pow(2.0f, 8.0f * glossiness);
            
                float diff = gloss - SOFTEN_MAX;
            
                if (diff > 0.0f) gloss = SOFTEN_MAX + sqrt(diff);
            
                float gloss2 = gloss * gloss;
            
                float result = 0.5f * exp(-(gloss2 * proj2) / NH2) * gloss2 / denom;
            
                gloss *= 0.5f;
            
                gloss2 = gloss * gloss;
            
                result += exp(-(gloss2 * proj2) / NH2) * gloss2 / denom;
            
                gloss *= 0.5f;
            
                gloss2 = gloss * gloss;
            
                result += 1.5f * exp(-(gloss2 * proj2) / NH2) * gloss2 / denom;
            
                return result * NL_NV_NH_VH.x;
            }
            
            uniform float _SpecularGloss;
            float3 ambient( float3 diffuse , float3 ambientShader ){
            return diffuse * ambientShader;
            }
            
            uniform float _f0;
            uniform float _f1;
            uniform float _fresnelPower;
            float customF( float4 NL_NV_NH_VH , float f0 , float f1 , float fresnelPower ){
            float f = lerp(f0, f1, pow(1.0f - NL_NV_NH_VH.y, fresnelPower)); 
            
                return f; 
            }
            
            float3 Result( float3 diffuse , float3 ambient , float3 specular ){
            return float4(diffuse + ambient + specular,1.0);
            }
            
            float3 specularcompute( float3 specular , float3 light , float Fs , float SpecularLevel ){
            return specular*light*Fs*SpecularLevel;
            }
            
            uniform float _SpecularLevel;
            uniform float4 _SpecularColor;
            uniform float4 _DiffuseColor;
            uniform float _DiffuseFade;
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
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float4 node_8845 = myshadergeom( halfDirection , i.normalDir , LightL( lightDirection , i.normalDir ) , viewDirection );
                float4 _DiffuseMap_var = tex2D(_DiffuseMap,TRANSFORM_TEX(i.uv0, _DiffuseMap));
                float3 node_3196 = lerp(_DiffuseColor.rgb,_DiffuseMap_var.rgb,_DiffuseFade);
                float3 finalColor = Result( lambertianDiffuse( node_8845 , node_3196 ) , ambient( node_3196 , UNITY_LIGHTMODEL_AMBIENT.rgb ) , specularcompute( (wardSpecularIso( i.normalDir , halfDirection , node_8845 , _SpecularGloss )*_SpecularColor.rgb) , _LightColor0.rgb , customF( node_8845 , _f0 , _f1 , _fresnelPower ) , _SpecularLevel ) );
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
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
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            float4 myshadergeom( float3 H , float3 N , float3 L , float3 V ){
             float NL = saturate(dot(N, L)); 
            
                float NV = saturate(dot(N, V)); 
            
                float NH = saturate(dot(N, H)); 
            
                float VH = saturate(dot(V, H)); 
            
                return float4( NL, NV, NH, VH ); 
            }
            
            float3 LightL( float3 L , float3 N ){
            if(dot(N,L)>0)
               return L;
            else
               return -L;
            }
            
            float3 lambertianDiffuse( float4 NL_NV_NH_VH , float3 diffuseI ){
            const float oneOverPi = 1 / 3.141592654; 
            
                return NL_NV_NH_VH.x * diffuseI; // * oneOverPi; 
            }
            
            uniform sampler2D _DiffuseMap; uniform float4 _DiffuseMap_ST;
            float3 wardSpecularIso( float3 N , float3 H , float4 NL_NV_NH_VH , float glossiness ){
            const float EPSILON = 1e-6f;
            
                const float SOFTEN_MAX = 80.0f;
            
                const float FOUR_PI = 4.0f * 3.141592654f;
            
                float NLNV = NL_NV_NH_VH.x * NL_NV_NH_VH.y;
            
                if (NLNV < EPSILON) return 0.0f;
            
                float denom = FOUR_PI * sqrt(NLNV);
            
                float NH2 = NL_NV_NH_VH.z * NL_NV_NH_VH.z;
            
                float3 proj = H - NL_NV_NH_VH.z * N;
            
                float proj2 = dot(proj, proj);
            
                float gloss = pow(2.0f, 8.0f * glossiness);
            
                float diff = gloss - SOFTEN_MAX;
            
                if (diff > 0.0f) gloss = SOFTEN_MAX + sqrt(diff);
            
                float gloss2 = gloss * gloss;
            
                float result = 0.5f * exp(-(gloss2 * proj2) / NH2) * gloss2 / denom;
            
                gloss *= 0.5f;
            
                gloss2 = gloss * gloss;
            
                result += exp(-(gloss2 * proj2) / NH2) * gloss2 / denom;
            
                gloss *= 0.5f;
            
                gloss2 = gloss * gloss;
            
                result += 1.5f * exp(-(gloss2 * proj2) / NH2) * gloss2 / denom;
            
                return result * NL_NV_NH_VH.x;
            }
            
            uniform float _SpecularGloss;
            float3 ambient( float3 diffuse , float3 ambientShader ){
            return diffuse * ambientShader;
            }
            
            uniform float _f0;
            uniform float _f1;
            uniform float _fresnelPower;
            float customF( float4 NL_NV_NH_VH , float f0 , float f1 , float fresnelPower ){
            float f = lerp(f0, f1, pow(1.0f - NL_NV_NH_VH.y, fresnelPower)); 
            
                return f; 
            }
            
            float3 Result( float3 diffuse , float3 ambient , float3 specular ){
            return float4(diffuse + ambient + specular,1.0);
            }
            
            float3 specularcompute( float3 specular , float3 light , float Fs , float SpecularLevel ){
            return specular*light*Fs*SpecularLevel;
            }
            
            uniform float _SpecularLevel;
            uniform float4 _SpecularColor;
            uniform float4 _DiffuseColor;
            uniform float _DiffuseFade;
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
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float4 node_8845 = myshadergeom( halfDirection , i.normalDir , LightL( lightDirection , i.normalDir ) , viewDirection );
                float4 _DiffuseMap_var = tex2D(_DiffuseMap,TRANSFORM_TEX(i.uv0, _DiffuseMap));
                float3 node_3196 = lerp(_DiffuseColor.rgb,_DiffuseMap_var.rgb,_DiffuseFade);
                float3 finalColor = Result( lambertianDiffuse( node_8845 , node_3196 ) , ambient( node_3196 , UNITY_LIGHTMODEL_AMBIENT.rgb ) , specularcompute( (wardSpecularIso( i.normalDir , halfDirection , node_8845 , _SpecularGloss )*_SpecularColor.rgb) , _LightColor0.rgb , customF( node_8845 , _f0 , _f1 , _fresnelPower ) , _SpecularLevel ) );
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
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
