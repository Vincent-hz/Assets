
Shader "BVShader/2_MetalPaint" {
    Properties {
        _BumpMap ("BumpMap", 2D) = "black" {}
        _BumpAcount ("BumpAcount", Range(0, 1)) = 0
        _PearlColor ("PearlColor", Color) = (0,0,0,1)
        _PearlLevel ("PearlLevel", Float ) = 0
        _PearlFalloff ("PearlFalloff", Float ) = 0
        _DiffuseBias ("DiffuseBias", Float ) = 0
        _BaseBias ("BaseBias", Float ) = 0
        _DiffusionExponent ("DiffusionExponent", Float ) = 0
        _SpecularExponent ("SpecularExponent", Float ) = 0
        _SpecularGlazing ("SpecularGlazing", Float ) = 0
        _f0 ("f0", Float ) = 0
        _f1 ("f1", Float ) = 0
        _fresnelPower ("fresnelPower", Float ) = 0
        _DiffuseLevel ("DiffuseLevel", Float ) = 0
        _DiffusionLevel ("DiffusionLevel", Float ) = 1
        _DiffusionTint ("DiffusionTint", Float ) = 0
        _SpecularLevel ("SpecularLevel", Float ) = 0
        _PearlMode ("PearlMode", Float ) = 0
        _BaseColor ("BaseColor", Color) = (0,0,0,1)
        _SpecularColor ("SpecularColor", Color) = (1,1,1,1)
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
            float4 shaderGeom( float3 N , float3 L , float3 V , float3 H ){
                float NL = saturate(dot(N, L)); 
            
                float NV = saturate(dot(N, V)); 
            
                float NH = saturate(dot(N, H)); 
            
                float VH = saturate(dot(V, H)); 
            
                return float4( NL, NV, NH, VH ); 
            }
            
            float3 heightMapTransform( sampler2D Map1 , float2 uv , float4x4 transform , float scale , float3 Tw , float3 Bw , float3 Nw ){
            float3x3 mtxTangent = {Tw, Bw, Nw};	
            				Tw = normalize(mul(mul((float3x3)transform, float3(1.0f, 0.0f, 0.0f)), mtxTangent)); 
            				Bw = normalize(mul(mul((float3x3)transform, float3(0.0f, 1.0f, 0.0f)), mtxTangent)); 
            				float3 avg = (1.0f / 3.0f).xxx;	float2 offset = max(fwidth(uv), float2(0.001f, 0.001f)); 	
            				float2 st = mul(transform, float4(uv, 0.0f, 1.0f)).xy;
            				float h0 = dot(tex2D(Map1, st).xyz, avg); 
            				float hx = dot(tex2D(Map1, st + float2(offset.x, 0.0f)).xyz, avg); 
            				float hy = dot(tex2D(Map1, st + float2(0.0f, offset.y)).xyz, avg); 
            				float2 diff = float2(h0 - hx, h0 - hy) / offset;
            				return normalize(Nw + (diff.x * Tw + diff.y * Bw) * scale);	
            }
            
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _BumpAcount;
            float3 MPEdgeColor( float3 N , float3 V , float diffuseBias , float3 baseColor , float3 pearlColor , float pearlLevel , float pearlFalloff ){
            float NV = saturate(dot(N, V));
            
               float3 darkColor     = (1.0f - diffuseBias) * baseColor;
            
               float3 adjPearlColor = lerp(darkColor, pearlColor, pearlLevel);
            
               return lerp(adjPearlColor, darkColor, pow(NV, pearlFalloff));
            }
            
            uniform float4 _PearlColor;
            uniform float _PearlLevel;
            uniform float _PearlFalloff;
            uniform float _DiffuseBias;
            float3 MPDiffuse( float4 NL_NV , float3 light , float diffuseBias , float3 baseColor , float baseBias , float3 edgeColor ){
            float3 color = lerp(edgeColor, baseColor, pow(NL_NV.x, baseBias) * NL_NV.y);
            
               return light * color * pow(NL_NV.x, diffuseBias + 1.0f);
            }
            
            float3 LightL( float3 L , float3 N ){
            if(dot(L,N)>0)
                return L;
            else
                return -L;
            }
            
            uniform float _BaseBias;
            float3 MPDiffusion( float4 NL_NV_NH , float3 light , float exponent ){
            return light * pow(NL_NV_NH.z, exponent);
            }
            
            float3 MPSpecular( float4 NL_NV_NH , float3 light , float exponent , float glazing ){
            float s = pow(NL_NV_NH.z, exponent);
            
               return light * (glazing ? smoothstep(0.5f, 0.8f, s) : s);
            
            }
            
            uniform float _DiffusionExponent;
            uniform float _SpecularExponent;
            uniform float _SpecularGlazing;
            float CustomF( float4 NL_NV_NH_VH , float f0 , float f1 , float fresnelPower ){
            float f = lerp(f0, f1, pow(1.0f - NL_NV_NH_VH.y, fresnelPower)); 
            
                return f; 
            }
            
            uniform float _f0;
            uniform float _f1;
            uniform float _fresnelPower;
            float3 Combine( float diffuseLevel , float3 diffuseContrib , float3 baseColor , float diffusionTint , float diffusionLevel , float3 diffusionContrib , float specularLevel , float3 specularContrib , float specularFalloff , float3 Ambient ){
            float3 diff      = diffuseLevel   * diffuseContrib;
            
               float3 diffusion = diffusionLevel * diffusionContrib * (diffusionTint ? baseColor : 1.0f);
            
               float3 spec      = specularLevel  * specularContrib * specularFalloff;
            return float4(diff + diffusion + spec + Ambient*baseColor,1.0);
            }
            
            uniform float _DiffuseLevel;
            uniform float _DiffusionLevel;
            uniform float _DiffusionTint;
            uniform float _SpecularLevel;
            uniform float _PearlMode;
            uniform float4 _BaseColor;
            uniform float4 _SpecularColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
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
                float4 node_6551 = shaderGeom( i.normalDir , LightL( lightDirection , i.normalDir ) , viewDirection , halfDirection );
                float node_9173_if_leA = step(_PearlMode,1.0);
                float node_9173_if_leB = step(1.0,_PearlMode);
                float4x4 node_7847 = {
                    {1,0,0,0},
                    {0,1,0,0},
                    {0,0,1,0},
                    {0,0,0,1}
                };
                float3 node_8710 = heightMapTransform( _BumpMap , i.uv0 , node_7847 , _BumpAcount , i.tangentDir , i.bitangentDir , i.normalDir );
                float3 node_9173 = lerp((node_9173_if_leA*_BaseColor.rgb)+(node_9173_if_leB*node_8710),node_8710,node_9173_if_leA*node_9173_if_leB);
                float3 finalColor = Combine( _DiffuseLevel , MPDiffuse( node_6551 , _LightColor0.rgb , _DiffuseBias , node_9173 , _BaseBias , MPEdgeColor( i.normalDir , viewDirection , _DiffuseBias , node_9173 , _PearlColor.rgb , _PearlLevel , _PearlFalloff ) ) , node_9173 , _DiffusionTint , _DiffusionLevel , MPDiffusion( node_6551 , _LightColor0.rgb , _DiffusionExponent ) , _SpecularLevel , MPSpecular( node_6551 , _SpecularColor.rgb , _SpecularExponent , _SpecularGlazing ) , CustomF( node_6551 , _f0 , _f1 , _fresnelPower ) , UNITY_LIGHTMODEL_AMBIENT.rgb );
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
            float4 shaderGeom( float3 N , float3 L , float3 V , float3 H ){
                float NL = saturate(dot(N, L)); 
            
                float NV = saturate(dot(N, V)); 
            
                float NH = saturate(dot(N, H)); 
            
                float VH = saturate(dot(V, H)); 
            
                return float4( NL, NV, NH, VH ); 
            }
            
            float3 heightMapTransform( sampler2D Map1 , float2 uv , float4x4 transform , float scale , float3 Tw , float3 Bw , float3 Nw ){
            float3x3 mtxTangent = {Tw, Bw, Nw};	
            				Tw = normalize(mul(mul((float3x3)transform, float3(1.0f, 0.0f, 0.0f)), mtxTangent)); 
            				Bw = normalize(mul(mul((float3x3)transform, float3(0.0f, 1.0f, 0.0f)), mtxTangent)); 
            				float3 avg = (1.0f / 3.0f).xxx;	float2 offset = max(fwidth(uv), float2(0.001f, 0.001f)); 	
            				float2 st = mul(transform, float4(uv, 0.0f, 1.0f)).xy;
            				float h0 = dot(tex2D(Map1, st).xyz, avg); 
            				float hx = dot(tex2D(Map1, st + float2(offset.x, 0.0f)).xyz, avg); 
            				float hy = dot(tex2D(Map1, st + float2(0.0f, offset.y)).xyz, avg); 
            				float2 diff = float2(h0 - hx, h0 - hy) / offset;
            				return normalize(Nw + (diff.x * Tw + diff.y * Bw) * scale);	
            }
            
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _BumpAcount;
            float3 MPEdgeColor( float3 N , float3 V , float diffuseBias , float3 baseColor , float3 pearlColor , float pearlLevel , float pearlFalloff ){
            float NV = saturate(dot(N, V));
            
               float3 darkColor     = (1.0f - diffuseBias) * baseColor;
            
               float3 adjPearlColor = lerp(darkColor, pearlColor, pearlLevel);
            
               return lerp(adjPearlColor, darkColor, pow(NV, pearlFalloff));
            }
            
            uniform float4 _PearlColor;
            uniform float _PearlLevel;
            uniform float _PearlFalloff;
            uniform float _DiffuseBias;
            float3 MPDiffuse( float4 NL_NV , float3 light , float diffuseBias , float3 baseColor , float baseBias , float3 edgeColor ){
            float3 color = lerp(edgeColor, baseColor, pow(NL_NV.x, baseBias) * NL_NV.y);
            
               return light * color * pow(NL_NV.x, diffuseBias + 1.0f);
            }
            
            float3 LightL( float3 L , float3 N ){
            if(dot(L,N)>0)
                return L;
            else
                return -L;
            }
            
            uniform float _BaseBias;
            float3 MPDiffusion( float4 NL_NV_NH , float3 light , float exponent ){
            return light * pow(NL_NV_NH.z, exponent);
            }
            
            float3 MPSpecular( float4 NL_NV_NH , float3 light , float exponent , float glazing ){
            float s = pow(NL_NV_NH.z, exponent);
            
               return light * (glazing ? smoothstep(0.5f, 0.8f, s) : s);
            
            }
            
            uniform float _DiffusionExponent;
            uniform float _SpecularExponent;
            uniform float _SpecularGlazing;
            float CustomF( float4 NL_NV_NH_VH , float f0 , float f1 , float fresnelPower ){
            float f = lerp(f0, f1, pow(1.0f - NL_NV_NH_VH.y, fresnelPower)); 
            
                return f; 
            }
            
            uniform float _f0;
            uniform float _f1;
            uniform float _fresnelPower;
            float3 Combine( float diffuseLevel , float3 diffuseContrib , float3 baseColor , float diffusionTint , float diffusionLevel , float3 diffusionContrib , float specularLevel , float3 specularContrib , float specularFalloff , float3 Ambient ){
            float3 diff      = diffuseLevel   * diffuseContrib;
            
               float3 diffusion = diffusionLevel * diffusionContrib * (diffusionTint ? baseColor : 1.0f);
            
               float3 spec      = specularLevel  * specularContrib * specularFalloff;
            return float4(diff + diffusion + spec + Ambient*baseColor,1.0);
            }
            
            uniform float _DiffuseLevel;
            uniform float _DiffusionLevel;
            uniform float _DiffusionTint;
            uniform float _SpecularLevel;
            uniform float _PearlMode;
            uniform float4 _BaseColor;
            uniform float4 _SpecularColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
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
                float4 node_6551 = shaderGeom( i.normalDir , LightL( lightDirection , i.normalDir ) , viewDirection , halfDirection );
                float node_9173_if_leA = step(_PearlMode,1.0);
                float node_9173_if_leB = step(1.0,_PearlMode);
                float4x4 node_7847 = {
                    {1,0,0,0},
                    {0,1,0,0},
                    {0,0,1,0},
                    {0,0,0,1}
                };
                float3 node_8710 = heightMapTransform( _BumpMap , i.uv0 , node_7847 , _BumpAcount , i.tangentDir , i.bitangentDir , i.normalDir );
                float3 node_9173 = lerp((node_9173_if_leA*_BaseColor.rgb)+(node_9173_if_leB*node_8710),node_8710,node_9173_if_leA*node_9173_if_leB);
                float3 finalColor = Combine( _DiffuseLevel , MPDiffuse( node_6551 , _LightColor0.rgb , _DiffuseBias , node_9173 , _BaseBias , MPEdgeColor( i.normalDir , viewDirection , _DiffuseBias , node_9173 , _PearlColor.rgb , _PearlLevel , _PearlFalloff ) ) , node_9173 , _DiffusionTint , _DiffusionLevel , MPDiffusion( node_6551 , _LightColor0.rgb , _DiffusionExponent ) , _SpecularLevel , MPSpecular( node_6551 , _SpecularColor.rgb , _SpecularExponent , _SpecularGlazing ) , CustomF( node_6551 , _f0 , _f1 , _fresnelPower ) , UNITY_LIGHTMODEL_AMBIENT.rgb );
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
