// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Dissolution" {
	Properties {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_DissolveScale("Dissolve Progression", Range(0.0, 1.0)) = 0.0
		_DissolveTex("Dissolve Texture", 2D) = "white" {}
		_GlowIntensity("Glow Intensity", Range(0.0, 5.0)) = 0.05
		_GlowScale("Glow Size", Range(0.0, 5.0)) = 1.0
		_Glow("Glow Color", Color) = (1, 1, 1, 1)
		_GlowEnd("Glow End Color", Color) = (1, 1, 1, 1)
		_GlowColFac("Glow Colorshift", Range(0.01, 2.0)) = 0.75
		_DissolveStart("Dissolve Start Point", Vector) = (1, 1, 1, 1)
		_DissolveEnd("Dissolve End Point", Vector) = (0, 0, 0, 1)
		_DissolveBand("Dissolve Band Size", Float) = 0.25

	}
	SubShader {
		Tags { 
		"Queue" = "Transparent"
		"RenderType" = "Fade" 
		}
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard alpha:fade vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _DissolveTex;

		struct Input {
			float2 uv_MainTex;
			half dGeometry;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _Glow;
		fixed4 _GlowEnd;

		float _GlowScale;
		float _GlowIntensity;
		float _GlowColFac;

		half _DissolveScale;

		fixed4 _DissolveEnd;
		fixed4 _DissolveStart;
		half _DissolveBand;

		//Precompute dissolve direction.
		static float3 dDir = normalize(_DissolveEnd - _DissolveStart);

		//Precompute gradient start position.
		static float3 dissolveStartConverted = _DissolveStart - _DissolveBand * dDir;

		//Precompute reciprocal of band size.
		static float dBandFactor = 1.0f / _DissolveBand;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)
			//Don't forget to specify your vertex routine.

		//...//
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			//Calculate geometry-based dissolve coefficient.
			//Compute top of dissolution gradient according to dissolve progression.
			float3 dPoint = lerp(dissolveStartConverted, _DissolveEnd, _DissolveScale);

			//Project vector between current vertex and top of gradient onto dissolve direction.
			//Scale coefficient by band (gradient) size.
			o.dGeometry = dot(v.vertex - dPoint, dDir) * dBandFactor;
		}
		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			//Convert dissolve progression to -1 to 1 scale.
			half dBase = -2.0f * _DissolveScale + 1.0f;

			//Read from noise texture.
			fixed4 dTex = tex2D(_DissolveTex, IN.uv_MainTex);
			//Convert dissolve texture sample based on dissolve progression.
			half dTexRead = dTex.r + dBase;
			half dFinal = dTexRead + IN.dGeometry;

			//Clamp and set alpha.
			half alpha = clamp(dFinal, 0.0f, 1.0f);
			o.Alpha = alpha;

			//Shift the computed raw alpha value based on the scale factor of the glow.
			//Scale the shifted value based on effect intensity.
			half dPredict = (_GlowScale - dFinal) * _GlowIntensity;
			//Change colour interpolation by adding in another factor controlling the gradient.
			half dPredictCol = (_GlowScale * _GlowColFac - dFinal) * _GlowIntensity;

			//Calculate and clamp glow colour.
			fixed4 glowCol = dPredict * lerp(_Glow, _GlowEnd, clamp(dPredictCol, 0.0f, 1.0f));
			glowCol = clamp(glowCol, 0.0f, 1.0f);
			o.Emission = glowCol;
		}
		ENDCG

		
	}
	FallBack "Diffuse"
}
