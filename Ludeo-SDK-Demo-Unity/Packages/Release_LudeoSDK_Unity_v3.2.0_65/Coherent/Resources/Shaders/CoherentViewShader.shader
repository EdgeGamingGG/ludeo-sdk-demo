Shader "Coherent/ViewShader" {
	Properties {
		_MainTex ("Texture", any) = "" {}
		_ColorMultiplier ("_ColorMultiplier", float) = 1.0
	}

	SubShader {

		Tags { "ForceSupported" = "True" "RenderType"="Overlay" }

		Lighting Off
		Blend One OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		Fog { Mode Off }
		ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float _ColorMultiplier;
			uniform float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 flipuv = float2(i.texcoord.x, 1 - i.texcoord.y);
				fixed4 color = tex2D(_MainTex, flipuv) * i.color;
				return fixed4(pow(color.rgba, _ColorMultiplier));
			}
			ENDCG
		}
	}

	Fallback off
}