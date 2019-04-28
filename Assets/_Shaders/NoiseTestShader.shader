Shader "Custom/NoiseTestShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float fade(float t ) {
				return t*t*t*(t*(t*6.0 - 15.0) + 10.0);
			}

			float grad(fixed2 p ) {
				const float texture_width = 256.0;
				fixed4 v = tex2D(_MainTex,
					fixed2(p.x / texture_width, p.y / texture_width));
				return normalize(v.xy*2.0 - fixed2(1.0,1.0));
			}

			float noise(fixed2 p ) {
				/* Calculate lattice points. */
				fixed2 p0 = floor(p);
				fixed2 p1 = p0 + fixed2(1.0, 0.0);
				fixed2 p2 = p0 + fixed2(0.0, 1.0);
				fixed2 p3 = p0 + fixed2(1.0, 1.0);

				/* Look up gradients at lattice points. */
				fixed2 g0 = grad(p0);
				fixed2 g1 = grad(p1);
				fixed2 g2 = grad(p2);
				fixed2 g3 = grad(p3);

				float t0 = p.x - p0.x;
				float fade_t0 = fade(t0); /* Used for interpolation in horizontal direction */

				float t1 = p.y - p0.y;
				float fade_t1 = fade(t1); /* Used for interpolation in vertical direction. */

				/* Calculate dot products and interpolate.*/
				float p0p1 = (1.0 - fade_t0) * dot(g0, (p - p0)) + fade_t0 * dot(g1, (p - p1)); /* between upper two lattice points */
				float p2p3 = (1.0 - fade_t0) * dot(g2, (p - p2)) + fade_t0 * dot(g3, (p - p3)); /* between lower two lattice points */

				/* Calculate final result */
				return (1.0 - fade_t1) * p0p1 + fade_t1 * p2p3;
			}

			float noise01(fixed2 p) {
				return (noise(p) + 1) / 2;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				//fixed4 col = tex2D(_MainTex, i.uv);

				fixed2 p = i.uv * 856;

				float r =
						noise01(p * (1.0/150))  * noise01(p * (1.0/140))
//						+ noise01(p * (1.0/96))  * 0.3
//						+ noise01(p * (1.0/80))  * 0.3
//						- noise01(p * (1.0/148))  * 0.2
//						- noise01(p * (1.0/128))  * noise01(p * (1.0/140))
						;

				//float g = noise(p * (-1.0/4))  * .5 +
				//	noise(p * (-1.0/6))  * .75 -
				//	noise(p * (-1.0/12))  * 1.75
				//	- r * 0.6f;
				//if (g<0) {
				//	g = 0;
				//}
				float g = 0;

				//float b = noise(p * (1.0/20))  * 1 +
				//	 noise(p * (1.0/24))  * 1 +
				//	 noise(p * (1.0/26))  * 1

 				//	- r * 0.6f;

				//if (b<0) {
				//	b = 0;
				//}
				float b = 0;

				fixed4 color = fixed4(r,g,b,1);

				return color;
			}

			ENDCG
		}
	}
}
