Shader "Custom/select" {
    Properties {
			_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
	    Tags { "Queue" = "Transparent" "RenderType" = "Opaque"}      
        //描边
        pass
        {
            Cull off
			ZWrite off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

             struct appdata {
		        float4 vertex : POSITION;
		    };
            struct v2f {
                float4  pos : SV_POSITION;
            } ;
            v2f vert (appdata v)
            {
                v2f o;
                float4 pos = v.vertex;
                o.pos = mul(UNITY_MATRIX_MVP,pos);

                return o;
            }
            float4 frag (v2f i) : COLOR
            {
                return float4(1,0.4,0,0);
            }
            ENDCG
        }
        
    }
}