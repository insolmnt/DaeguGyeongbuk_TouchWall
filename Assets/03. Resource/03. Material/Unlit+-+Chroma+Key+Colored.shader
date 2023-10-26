// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Chroma Key Colored"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
		_Cut("Cut", Range(0.0,1.0)) = 0.5
		_Fade("Fade", Range(0,1)) = 0.1
		_Rg("Rg", Range(0,1)) = 0.2
		_Rb("Rb", Range(0,1)) = 0.2
	}
	
	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite On
			Fog { Mode Off }
			Offset -1, -1
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			half4 _Color; 
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Cut;
			float _Fade;
			float _Rg;
			float _Rb;
	
			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};
	
			v2f o;

			v2f vert (appdata_t v)
			{
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;

				return o;
			}
				
			fixed4 frag (v2f IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.texcoord) * IN.color; 

				float r = abs(_Color.r - c.r);
				float g = abs(_Color.g - c.g);
				float b = abs(_Color.b - c.b);

				if (!(abs(c.g - c.r) < _Rg && abs(c.r - c.b) < _Rb))
				{
					c.a = 1;
				}
				else if (r < _Cut && g < _Cut && b < _Cut)
				{
					c.a = 0;
				}
				else 
				{
					float m = (r + g + b) / 3;
					if (_Fade > 0 && m < _Fade)// 
					{
						c.a = m / _Fade;
					}
					else
					{
						c.a = 1;
					}
				}

				return c;
			}


			ENDCG
		}
	}

	SubShader
	{
		LOD 100

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
		}
	}
}
