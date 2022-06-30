Shader "Sprite Shaders/Sprite Outline"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[Toggle] PixelSnap("Pixel snap", Float) = 0

		// Value to determine if outlining is enabled and outline color + size.
		[Toggle] _Outline("Outline", Float) = 0
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
		_OutlineSize("Outline Size", Float) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma shader_feature ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"

			struct appdata_t
			{
				fixed4 vertex   : POSITION;
				fixed4 color    : COLOR;
				fixed2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				fixed4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				fixed2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			fixed _Outline;
			fixed4 _OutlineColor;
			fixed _OutlineSize;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
		#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
		#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float4 _MainTex_TexelSize;

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				//fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				fixed4 c = tex2D(_MainTex, IN.texcoord);

				//If outline mode is enabled and pixel exists, try to draw outline
				if (_Outline == 1 && c.a != 0)
				{
					/*fixed leftPixel = tex2D(_MainTex, IN.texcoord + float2(_OutlineSize * -_MainTex_TexelSize.x, 0)).a;
					fixed upPixel = tex2D(_MainTex, IN.texcoord + float2(0, _OutlineSize * _MainTex_TexelSize.y)).a;
					fixed rightPixel = tex2D(_MainTex, IN.texcoord + float2(_OutlineSize * _MainTex_TexelSize.x, 0)).a;
					fixed bottomPixel = tex2D(_MainTex, IN.texcoord + float2(0, _OutlineSize  * -_MainTex_TexelSize.y)).a;

					fixed outline = (1 - leftPixel * upPixel * rightPixel * bottomPixel) * c.a;

					c = lerp(c, _OutlineColor, outline) * c.a;

					return c;*/
					
					c.rgba = (1 - 0.8) * c.rgba + (1 - 0.2) * _OutlineColor;
				}

				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
}