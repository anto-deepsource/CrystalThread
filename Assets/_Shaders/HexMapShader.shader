Shader "Custom/HexMapShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture (A)", 2D) = "white" {}
		_TextureB ("Texture B", 2D) = "white" {}
		_GradientAB ("Gradient from A to B", Range(0,1)) = 1.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PerformanceChecks"="False"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		//LOD 200

		Cull Back
    ZWrite On ZTest LEqual
    Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

		Pass
		{
		CGPROGRAM

		#pragma vertex vert alpha
		#pragma fragment frag alpha
		#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
		#pragma multi_compile _ PIXELSNAP_ON
		#include "UnityCG.cginc"


		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _TextureB;

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
			float2 texcoord2 : TEXCOORD1;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color    : COLOR;
			float2 texcoord  : TEXCOORD0;
			float2 texcoord2  : TEXCOORD1;
			float3 worldPos  : TEXCOORD2;
		};

		half _Glossiness;
		half _Metallic;
		half _GradientAB;
		fixed4 _Color;

		v2f vert(appdata_t IN) {
			v2f OUT;
			OUT.vertex = UnityObjectToClipPos(IN.vertex);
			OUT.worldPos = mul (unity_ObjectToWorld, IN.vertex);
			OUT.texcoord = IN.texcoord;
			OUT.texcoord2 = IN.texcoord2;
			OUT.color = IN.color * _Color;

			#ifdef PIXELSNAP_ON
			OUT.vertex = UnityPixelSnap (OUT.vertex);
			#endif

			return OUT;
		}

		fixed4 frag(v2f IN) : SV_Target {
			float g = IN.texcoord2.x * _GradientAB * 1.3f;
			//float g = _GradientAB;
			// Albedo comes from a texture tinted by color
			fixed4 a = tex2D (_MainTex, IN.texcoord) * _Color * g;
			fixed4 b = tex2D (_TextureB, IN.texcoord) * _Color * ( 1.0 - g);

			fixed4 c = a + b;

			float camDistance = distance(_WorldSpaceCameraPos,IN.worldPos);
			if ( camDistance < 20 ) {
				c.a = 0.2f;
			} else
			if ( camDistance > 1500 ) {
				c.a = 0.2f;
			} else {
				c.a = c.a;
			}

			return c * IN.color;
		}

		ENDCG
		}
	}
}
