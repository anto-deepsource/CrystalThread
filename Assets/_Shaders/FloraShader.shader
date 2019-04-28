Shader "Custom/FloraShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_StemTex ("Stem Texture", 2D) = "white" {}
		_FlowerTex ("Flower Texture", 2D) = "white" {}
		_Emission ("Emission", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" }
		LOD 200
		Cull Off
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _StemTex;
		sampler2D _FlowerTex;

		struct Input {
			float4 color    : COLOR;
			float2 uv_StemTex;
			float2 uv2_FlowerTex;
		};

		half _Emission;
		//half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c;
			if ( IN.uv2_FlowerTex.x < 0.5 ) {
				c = tex2D (_StemTex, IN.uv_StemTex);
			} else {
				c = tex2D (_FlowerTex, IN.uv_StemTex);
			}
			if (c.a < 0.5){
				clip(-1);
			}
			c *= IN.color * _Color;
			o.Albedo = c.rgb;
			//o.Emission = fixed4(_Emission,_Emission,_Emission,_Emission);
			// Metallic and smoothness come from slider variables
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Alpha = c.a;


		}
		ENDCG
	}
	FallBack "Diffuse"
}
