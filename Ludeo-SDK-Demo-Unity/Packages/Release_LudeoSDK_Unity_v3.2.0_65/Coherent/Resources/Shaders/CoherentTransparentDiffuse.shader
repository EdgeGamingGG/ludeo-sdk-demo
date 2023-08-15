Shader "Coherent/TransparentDiffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
		float2 flipuv = float2(IN.uv_MainTex.x, 1 -  IN.uv_MainTex.y);
		fixed4 color = tex2D(_MainTex, flipuv) * _Color;
		o.Albedo = color.rgb;
		o.Alpha = color.a;
}
ENDCG
}

Fallback "Transparent/VertexLit"
}
