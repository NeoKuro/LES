// Derived from:
// IPhoneGems
//     http://wiki.unity3d.com/index.php?title=IPhoneGems
// Spherical environment mapping (TexGen SphereMap)
//     http://docs.unity3d.com/Manual/SL-ImplementingTexGen.html
// Silhouette-Outlined Diffuse
//     http://wiki.unity3d.com/index.php?title=Silhouette-Outlined_Diffuse

Shader "FX/Diamond" {
Properties {
	_Color("Color", Color) = (1,1,1,1)
	_Emission("Emissive", Color) = (1,1,1,1)
	
	_OutlineColor ("Outline Color", Color) = (0,0,0,1)
	_Outline ("Outline width", Range (0.0, 0.03)) = .005
	
	_RefractTex("Refraction", 2D) = ""
	_ReflectTex("Reflect", 2D) = ""
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

	// note that a vertex shader is specified here but its using the one above
	Pass {
		Name "OUTLINE"
		Tags { "LightMode" = "Always" }
		Cull Off
		ZWrite Off
		
		// you can choose what kind of blending mode you want for the outline
		Blend SrcAlpha OneMinusSrcAlpha // Normal
		//Blend One One // Additive
		//Blend One OneMinusDstColor // Soft Additive
		//Blend DstColor Zero // Multiplicative
		//Blend DstColor SrcColor // 2x Multiplicative
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		
		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};
		
		struct v2f {
			float4 pos : POSITION;
			float4 color : COLOR;
		};
		
		uniform float _Outline;
		uniform float4 _OutlineColor;
		
		v2f vert(appdata v) {
			// just make a copy of incoming vertex data but scaled according to normal direction
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			
			float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
			float2 offset = TransformViewToProjection(norm.xy);
			
			o.pos.xy += offset * o.pos.z * _Outline;
			o.color = _OutlineColor;
			return o;
		}

		half4 frag(v2f i) :COLOR {
			return i.color;
		}
		ENDCG
	}

	Pass {
		Lighting Off
		
		Cull Front
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		
		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};
		
		v2f vert (float4 v : POSITION, float3 n : NORMAL) {
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v);
			
			// TexGen SphereMap
			float3 viewDir = normalize(ObjSpaceViewDir(v));
			float3 r = reflect(-viewDir, n);
			r = mul((float3x3)UNITY_MATRIX_MV, r);
			r.z += 1;
			float m = 2 * length(r);
			o.uv = r.xy / m + 0.5;
			
			return o;
		}
		
		sampler2D _ReflectTex;
		fixed4 _Color;
		
		half4 frag (v2f i) : SV_Target {
			return tex2D(_ReflectTex, i.uv) * _Color;
		}
		ENDCG
	}
	
	// Second pass - here we render the front faces of the diamonds.
	Pass {
		ZWrite On
		Blend One One
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		
		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};
		
		sampler2D _RefractTex;
		fixed4 _Emission;
		
		v2f vert (float4 v : POSITION, float3 n : NORMAL) {
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v);
			
			// TexGen SphereMap
			float3 viewDir = normalize(ObjSpaceViewDir(v));
			float3 r = reflect(-viewDir, n);
			r = mul((float3x3)UNITY_MATRIX_MV, r);
			r.z += 1;
			float m = 2 * length(r);
			o.uv = r.xy / m + 0.5;
			
			return o;
		}
		
		fixed4 frag (v2f i) : SV_Target {
			return tex2D(_RefractTex, i.uv) * _Emission;
		}
		ENDCG
	}
}
}
