Shader "Custom/FlareArmShader" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_WorldRenderTex ("World Render Texture", 2D) = "white" {}
		_PropagandaRenderTex ("Propaganda Render Texture", 2D) = "white" {}
		_WorldBlend  ("World Blend Value", Range(0,1)) = 1.0
		_PropagandaBlend  ("Propaganda Blend Value", Range(0,1)) = 1.0
		_DecalTexture ("Dynamic Decal Texture", 2D) = "white" {}
	}
	SubShader {
		Tags {
			//"Queue"="Transparent"
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
		float4 _MainTex_TexelSize;
		sampler2D _WorldRenderTex;
		sampler2D _PropagandaRenderTex;
		sampler2D _DecalTexture;

		half _WorldBlend;
		half _PropagandaBlend;

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
			float2 texcoord2 : TEXCOORD1;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color    : COLOR;
			float2 texcoord  : TEXCOORD0;
			float2 texcoord2  : TEXCOORD1;
			float3 worldPos  : TEXCOORD2;
			float4 screenPos : SCREEN_POSITION;

		};

		v2f vert(appdata_t IN ) {
			v2f OUT;
			OUT.vertex = UnityObjectToClipPos(IN.vertex);
			OUT.worldPos = mul (unity_ObjectToWorld, IN.vertex);
			#ifdef PIXELSNAP_ON
			OUT.vertex = UnityPixelSnap (OUT.vertex);
			#endif
			OUT.texcoord = IN.texcoord;
			OUT.screenPos = ComputeScreenPos(OUT.vertex);

			return OUT;
		}

		fixed4 frag(v2f IN) : SV_Target {

			fixed4 decalTex = tex2D (_DecalTexture, IN.texcoord);


			float2 uvCoords = IN.screenPos.xy/IN.screenPos.w;

			fixed4 worldRenderTex = tex2D (_WorldRenderTex, uvCoords);
			//fixed4 propagandaRenderTex = tex2D (_PropagandaRenderTex, uvCoords);

			fixed4 mainTex = tex2D (_MainTex, IN.texcoord);
			fixed4 mixedMainAndWorld = lerp(mainTex,worldRenderTex,_WorldBlend);
			//fixed4 mixedMainAndWorld = lerp(mainTex,propagandaRenderTex,_PropagandaBlend);

			if (decalTex.a>0.1f) {
				return decalTex * decalTex.a + (1- decalTex.a)*mixedMainAndWorld;
			} else {
				return mixedMainAndWorld;
			}

		}

		ENDCG
		}
	}
	FallBack "Diffuse"
}
