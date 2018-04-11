
Shader "BVShader/1_MetalPaint" {
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
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
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
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
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
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0+1.0);
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                d.boxMax[0] = unity_SpecCube0_BoxMax;
                d.boxMin[0] = unity_SpecCube0_BoxMin;
                d.probePosition[0] = unity_SpecCube0_ProbePosition;
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.boxMax[1] = unity_SpecCube1_BoxMax;
                d.boxMin[1] = unity_SpecCube1_BoxMin;
                d.probePosition[1] = unity_SpecCube1_ProbePosition;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
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
                float3 node_7659 = Combine( _DiffuseLevel , MPDiffuse( node_6551 , _LightColor0.rgb , _DiffuseBias , node_9173 , _BaseBias , MPEdgeColor( i.normalDir , viewDirection , _DiffuseBias , node_9173 , _PearlColor.rgb , _PearlLevel , _PearlFalloff ) ) , node_9173 , _DiffusionTint , _DiffusionLevel , MPDiffusion( node_6551 , _LightColor0.rgb , _DiffusionExponent ) , _SpecularLevel , MPSpecular( node_6551 , _SpecularColor.rgb , _SpecularExponent , _SpecularGlazing ) , CustomF( node_6551 , _f0 , _f1 , _fresnelPower ) , UNITY_LIGHTMODEL_AMBIENT.rgb );
                float3 specularColor = node_7659;
                float specularMonochrome;
                float3 diffuseColor = node_7659; // Need this for specular when using metallic
                diffuseColor = EnergyConservationBetweenDiffuseAndSpecular(diffuseColor, specularColor, specularMonochrome);
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, GGXTerm(NdotH, 1.0-gloss));
                float specularPBL = (NdotL*visTerm*normTerm) * (UNITY_PI / 4);
                if (IsGammaSpace())
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                specularPBL = max(0, specularPBL * NdotL);
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
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
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
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
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
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
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
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
                float3 node_7659 = Combine( _DiffuseLevel , MPDiffuse( node_6551 , _LightColor0.rgb , _DiffuseBias , node_9173 , _BaseBias , MPEdgeColor( i.normalDir , viewDirection , _DiffuseBias , node_9173 , _PearlColor.rgb , _PearlLevel , _PearlFalloff ) ) , node_9173 , _DiffusionTint , _DiffusionLevel , MPDiffusion( node_6551 , _LightColor0.rgb , _DiffusionExponent ) , _SpecularLevel , MPSpecular( node_6551 , _SpecularColor.rgb , _SpecularExponent , _SpecularGlazing ) , CustomF( node_6551 , _f0 , _f1 , _fresnelPower ) , UNITY_LIGHTMODEL_AMBIENT.rgb );
                float3 specularColor = node_7659;
                float specularMonochrome;
                float3 diffuseColor = node_7659; // Need this for specular when using metallic
                diffuseColor = EnergyConservationBetweenDiffuseAndSpecular(diffuseColor, specularColor, specularMonochrome);
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, GGXTerm(NdotH, 1.0-gloss));
                float specularPBL = (NdotL*visTerm*normTerm) * (UNITY_PI / 4);
                if (IsGammaSpace())
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                specularPBL = max(0, specularPBL * NdotL);
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
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
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
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
                float3 node_7659 = Combine( _DiffuseLevel , MPDiffuse( node_6551 , _LightColor0.rgb , _DiffuseBias , node_9173 , _BaseBias , MPEdgeColor( i.normalDir , viewDirection , _DiffuseBias , node_9173 , _PearlColor.rgb , _PearlLevel , _PearlFalloff ) ) , node_9173 , _DiffusionTint , _DiffusionLevel , MPDiffusion( node_6551 , _LightColor0.rgb , _DiffusionExponent ) , _SpecularLevel , MPSpecular( node_6551 , _SpecularColor.rgb , _SpecularExponent , _SpecularGlazing ) , CustomF( node_6551 , _f0 , _f1 , _fresnelPower ) , UNITY_LIGHTMODEL_AMBIENT.rgb );
                float3 diffColor = node_7659;
                float3 specColor = node_7659;
                float specularMonochrome = max(max(specColor.r, specColor.g),specColor.b);
                diffColor *= (1.0-specularMonochrome);
                o.Albedo = diffColor + specColor * 0.125; // No gloss connected. Assume it's 0.5
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    
}
