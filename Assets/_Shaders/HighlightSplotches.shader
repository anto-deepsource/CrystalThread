Shader "Custom/HighlightSplotches" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_GlowColor ("Glow Color", Color) = (1,0,0,1)
		_MainTex ("Main Texture (A)", 2D) = "white" {}
		_R("R", Range(0.00000001,1)) = 0.05
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
		float _R;
		fixed4 _Color;
		fixed4 _GlowColor;

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			float4 color    : COLOR;
			float2 texcoord  : TEXCOORD0;
		};



		bool CompareColors( fixed4 a, fixed4 otherColor ) {
			float r = abs(a.r-otherColor.r);
			float g = abs(a.g-otherColor.g);
			float b = abs(a.b-otherColor.b);
			float epsilon = 0.000001f;
			return (r<epsilon && g<epsilon && b<epsilon);
		}

		fixed4 Lerp( fixed4 colorA, fixed4 colorB, float percentageOfA ) {
			fixed4 a = colorA * percentageOfA;
			fixed4 b = colorB * ( 1.0 - percentageOfA);
			return a + b;
		}

		v2f vert(appdata_t IN) {
			v2f OUT;
			OUT.vertex = UnityObjectToClipPos(IN.vertex);
			OUT.texcoord = IN.texcoord;
			OUT.color = IN.color * _Color;

			return OUT;
		}

		fixed4 frag(v2f IN) : SV_Target {
			// Albedo comes from a texture tinted by color
			fixed4 a = tex2D (_MainTex, IN.texcoord);

			int notColor = 0;
			int isColor = 0;

			int count =0;

			float PART_PI = 3.14f/5;

			for( float r = 0; r<= _R; r += _R*0.25f) {
				for( float theta = 0; theta <= 3.14f * 2; theta +=PART_PI ) {
					float x = cos(theta) * r;
					float y = sin(theta) * r;
					fixed pointColor = tex2D (_MainTex, IN.texcoord + float2(x,y));
					if ( CompareColors(pointColor, IN.color ) ) {
						isColor ++;
					} else {
						notColor ++;
					}
					count ++;
					if ( count > 1000) {
						break;
					}
				}
				if ( count > 1000) {
					break;
				}
			}
			float dif = abs(notColor - isColor);
			if (dif <= count* 0.9f ) {
				float value = 1-dif / (count* 0.9f);
				a = Lerp(_GlowColor, a, value );
			}

			return a;
		}


		ENDCG
		}
	}
}
