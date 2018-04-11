
Shader "BVShader/2_Ceramic" {
    Properties {
        _DiffuseRoughness ("DiffuseRoughness", Float ) = 0
        _DiffuseColor ("DiffuseColor", Color) = (1,1,1,1)
        _DiffuseMap ("DiffuseMap", 2D) = "white" {}
        _BumpMap ("BumpMap", 2D) = "black" {}
        _SpecularColor ("SpecularColor", Color) = (1,1,1,1)
        _SpecularGloss ("SpecularGloss", Range(0, 1)) = 0
        _BumpAmount ("BumpAmount", Range(0, 1)) = 0.2289992
        _f0 ("f0", Range(0, 1)) = 0
        _f1 ("f1", Range(0, 1)) = 0
        _fresnelPower ("fresnelPower", Range(0, 1)) = 0
        _DiffuseFade ("DiffuseFade", Float ) = 1
        _SpecularLevel ("SpecularLevel", Range(0, 1)) = 0
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
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            float2 orenNayarAB( float diffRoughness ){
             float r2 = diffRoughness * diffRoughness; 
            
                float2 ab; 
            
                ab.x = 1.0 - 0.5 * r2 / (r2 + 0.33); 
            
                ab.y = 0.45 * r2 / (r2 + 0.09); 
            
                return ab; 
            }
            
            float lambertianDiffuse( float4 NL_NV_NH_VH ){
            return NL_NV_NH_VH.x;
            }
            
            uniform float _DiffuseRoughness;
            uniform float4 _DiffuseColor;
            uniform sampler2D _DiffuseMap; uniform float4 _DiffuseMap_ST;
            float wardSpecularIso( float3 N , float3 H , float4 NL_NV_NH_VH , float glossiness ){
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
            
            float4 shaderGeom2( float3 N , float3 L , float3 V , float3 H ){
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
            uniform float4 _SpecularColor;
            uniform float _SpecularGloss;
            uniform float _BumpAmount;
            float3 Combine( float3 diffuse , float3 specular , float3 ambient ){
            return diffuse+specular+ambient;
            }
            
            float orenNayarDiffuse( float4 NL_NV_NH_VH , float3 N , float3 L , float3 V , float2 ab ){
            float NL = NL_NV_NH_VH.x; 
            
            				float NV = NL_NV_NH_VH.y; 
            
            				float sinTan = sqrt( (1.0-NL*NL) * (1.0-NV*NV) ); 
            
            				sinTan /= 0.0000001 + max( NL, NV); 
            
            				float3 Vp = normalize( V - NV * N ); 
            
            				float3 Lp = normalize( L - NL * N ); 
            
            				float c = saturate( dot( Vp, Lp ) ); 
            
            				return NL * (ab.x + ab.y * c * sinTan );
            }
            
            float3 ambientcompute( float3 diffusemat , float3 ambientshader , float Fs ){
            return diffusemat*ambientshader;
            }
            
            float3 diffusecompute( float3 diffuse , float3 light , float Fs , float lightintensity ){
            return diffuse*light*lightintensity;
            }
            
            uniform float _f0;
            uniform float _f1;
            uniform float _fresnelPower;
            float customF( float4 NL_NV_NH_VH , float f0 , float f1 , float fresnelPower ){
            float f = lerp(f0, f1, pow(1.0f - NL_NV_NH_VH.y, fresnelPower)); 
            
                return f; 
            }
            
            float3 specularcompute( float3 specular , float3 light , float Fs , float SpecularLevel ){
            return specular*light*Fs*SpecularLevel;
            }
            
            uniform float _DiffuseFade;
            float4x4 bumpmatrix(){
            float4x4 bmt = { _BumpMap_ST.x,0,0,_BumpMap_ST.z,0,_BumpMap_ST.y,0,_BumpMap_ST.w,0,0,1,0,0,1,0,1 };
            return bmt;
            }
            
            uniform float _SpecularLevel;
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
                float attenuation = LIGHT_ATTENUATION(i);
                float node_1039_if_leA = step(_DiffuseRoughness,0.0);
                float node_1039_if_leB = step(0.0,_DiffuseRoughness);
                float3 node_1529 = LightL( lightDirection , i.normalDir );
                float4 node_172 = myshadergeom( halfDirection , i.normalDir , node_1529 , viewDirection );
                float node_6293 = orenNayarDiffuse( node_172 , i.normalDir , node_1529 , viewDirection , orenNayarAB( _DiffuseRoughness ) );
                float node_9188_if_leA = step(_DiffuseFade,0.0);
                float node_9188_if_leB = step(0.0,_DiffuseFade);
                float4 _DiffuseMap_var = tex2D(_DiffuseMap,TRANSFORM_TEX(i.uv0, _DiffuseMap));
                float3 node_8276 = (lerp((node_1039_if_leA*node_6293)+(node_1039_if_leB*lambertianDiffuse( node_172 )),node_6293,node_1039_if_leA*node_1039_if_leB)*lerp((node_9188_if_leA*_DiffuseColor.rgb)+(node_9188_if_leB*_DiffuseMap_var.rgb),_DiffuseColor.rgb,node_9188_if_leA*node_9188_if_leB));
                float3 node_1054 = heightMapTransform( _BumpMap , i.uv0 , bumpmatrix() , _BumpAmount , i.tangentDir , i.bitangentDir , i.normalDir );
                float4 node_1632 = shaderGeom2( node_1054 , node_1529 , viewDirection , halfDirection );
                float node_8523 = customF( node_1632 , _f0 , _f1 , _fresnelPower );
                float3 finalColor = Combine( diffusecompute( node_8276 , _LightColor0.rgb , node_8523 , attenuation ) , specularcompute( (wardSpecularIso( node_1054 , halfDirection , node_1632 , _SpecularGloss )*_SpecularColor.rgb) , _LightColor0.rgb , node_8523 , _SpecularLevel ) , ambientcompute( node_8276 , float3(1,1,1) , node_8523 ) );
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
            #include "Lighting.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            float2 orenNayarAB( float diffRoughness ){
             float r2 = diffRoughness * diffRoughness; 
            
                float2 ab; 
            
                ab.x = 1.0 - 0.5 * r2 / (r2 + 0.33); 
            
                ab.y = 0.45 * r2 / (r2 + 0.09); 
            
                return ab; 
            }
            
            float lambertianDiffuse( float4 NL_NV_NH_VH ){
            return NL_NV_NH_VH.x;
            }
            
            uniform float _DiffuseRoughness;
            uniform float4 _DiffuseColor;
            uniform sampler2D _DiffuseMap; uniform float4 _DiffuseMap_ST;
            float wardSpecularIso( float3 N , float3 H , float4 NL_NV_NH_VH , float glossiness ){
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
            
            float4 shaderGeom2( float3 N , float3 L , float3 V , float3 H ){
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
            uniform float4 _SpecularColor;
            uniform float _SpecularGloss;
            uniform float _BumpAmount;
            float3 Combine( float3 diffuse , float3 specular , float3 ambient ){
            return diffuse+specular+ambient;
            }
            
            float orenNayarDiffuse( float4 NL_NV_NH_VH , float3 N , float3 L , float3 V , float2 ab ){
            float NL = NL_NV_NH_VH.x; 
            
            				float NV = NL_NV_NH_VH.y; 
            
            				float sinTan = sqrt( (1.0-NL*NL) * (1.0-NV*NV) ); 
            
            				sinTan /= 0.0000001 + max( NL, NV); 
            
            				float3 Vp = normalize( V - NV * N ); 
            
            				float3 Lp = normalize( L - NL * N ); 
            
            				float c = saturate( dot( Vp, Lp ) ); 
            
            				return NL * (ab.x + ab.y * c * sinTan );
            }
            
            float3 ambientcompute( float3 diffusemat , float3 ambientshader , float Fs ){
            return diffusemat*ambientshader;
            }
            
            float3 diffusecompute( float3 diffuse , float3 light , float Fs , float lightintensity ){
            return diffuse*light*lightintensity;
            }
            
            uniform float _f0;
            uniform float _f1;
            uniform float _fresnelPower;
            float customF( float4 NL_NV_NH_VH , float f0 , float f1 , float fresnelPower ){
            float f = lerp(f0, f1, pow(1.0f - NL_NV_NH_VH.y, fresnelPower)); 
            
                return f; 
            }
            
            float3 specularcompute( float3 specular , float3 light , float Fs , float SpecularLevel ){
            return specular*light*Fs*SpecularLevel;
            }
            
            uniform float _DiffuseFade;
            float4x4 bumpmatrix(){
            float4x4 bmt = { _BumpMap_ST.x,0,0,_BumpMap_ST.z,0,_BumpMap_ST.y,0,_BumpMap_ST.w,0,0,1,0,0,1,0,1 };
            return bmt;
            }
            
            uniform float _SpecularLevel;
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
                float attenuation = LIGHT_ATTENUATION(i);
                float node_1039_if_leA = step(_DiffuseRoughness,0.0);
                float node_1039_if_leB = step(0.0,_DiffuseRoughness);
                float3 node_1529 = LightL( lightDirection , i.normalDir );
                float4 node_172 = myshadergeom( halfDirection , i.normalDir , node_1529 , viewDirection );
                float node_6293 = orenNayarDiffuse( node_172 , i.normalDir , node_1529 , viewDirection , orenNayarAB( _DiffuseRoughness ) );
                float node_9188_if_leA = step(_DiffuseFade,0.0);
                float node_9188_if_leB = step(0.0,_DiffuseFade);
                float4 _DiffuseMap_var = tex2D(_DiffuseMap,TRANSFORM_TEX(i.uv0, _DiffuseMap));
                float3 node_8276 = (lerp((node_1039_if_leA*node_6293)+(node_1039_if_leB*lambertianDiffuse( node_172 )),node_6293,node_1039_if_leA*node_1039_if_leB)*lerp((node_9188_if_leA*_DiffuseColor.rgb)+(node_9188_if_leB*_DiffuseMap_var.rgb),_DiffuseColor.rgb,node_9188_if_leA*node_9188_if_leB));
                float3 node_1054 = heightMapTransform( _BumpMap , i.uv0 , bumpmatrix() , _BumpAmount , i.tangentDir , i.bitangentDir , i.normalDir );
                float4 node_1632 = shaderGeom2( node_1054 , node_1529 , viewDirection , halfDirection );
                float node_8523 = customF( node_1632 , _f0 , _f1 , _fresnelPower );
                float3 finalColor = Combine( diffusecompute( node_8276 , _LightColor0.rgb , node_8523 , attenuation ) , specularcompute( (wardSpecularIso( node_1054 , halfDirection , node_1632 , _SpecularGloss )*_SpecularColor.rgb) , _LightColor0.rgb , node_8523 , _SpecularLevel ) , ambientcompute( node_8276 , float3(1,1,1) , node_8523 ) );
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
