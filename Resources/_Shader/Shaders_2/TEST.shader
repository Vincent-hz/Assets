Shader "MyShader/TEST" {
	Properties{
		_DissFactor("Diss Factor ",Range(-2,2)) = 0
		_Trace("Trace Range",Range(0,0.5)) = 0
		_Cutoff("_Cutoff ",Range(0,1)) = 0
		_Color("SpecularColor", Color) = (1,1,1,1)
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" }
		LOD 200
		//Cull off
		//ZWrite Off  
		CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff vertex:vert  


	struct Input {
		fixed4 vertColor;
	};


	fixed _DissFactor;
	fixed _Trace;
	fixed4 _Color;
	void vert(inout appdata_full v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.vertColor = v.color;

		if (v.vertex.y<_DissFactor)
		{
			o.vertColor.a = 0;
		}
		else
		{
			if (v.vertex.y < _DissFactor + _Trace)
			{
				o.vertColor.a = 0.5;
			}
		}
	}

	void surf(Input IN, inout SurfaceOutput o) 
	{
		//o.Albedo = IN.vertColor.rgb;
		//if (IN.vertColor.a == 0.5)
		//{
		//	/*o.Albedo = _Color.rgb;
		//	o.Alpha = 1;*/
		//}
		//else
		//{
			o.Albedo = _Color.rgb;
			o.Alpha = IN.vertColor.a;
	//	}
	}

	ENDCG
	}
		FallBack "Diffuse"
}