Shader "POSTAL: Onslaught/SpriteDropShadow"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_pixelsPerUnit("Pixels Per Unit", Float) = 32
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_ShadowColor ("Shadow", Color) = (0,0,0,1)
		_ShadowOffset ("ShadowOffset", Vector) = (0,-0.1,0,0)
		[MaterialToggle] _HurtFlash ("Hurt Flash", Float) = 0
		_HurtFlashColor ("Hurt Flash Color", Color) = (1,1,1,1)
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

		// Draw shadow...
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			fixed4 _ShadowColor;
			float4 _ShadowOffset;
			float _pixelsPerUnit;

			float4 AlignToPixelGrid(float4 vertex)
			{
				float4 worldPos = mul(unity_ObjectToWorld, vertex);

				// NOTE: Causes sprite children to jitter when moving! Removed!
				//worldPos.x = floor(worldPos.x * _pixelsPerUnit + 0.5) / _pixelsPerUnit;
				//worldPos.y = floor(worldPos.y * _pixelsPerUnit + 0.5) / _pixelsPerUnit;

				return mul(unity_WorldToObject, worldPos);
			}

			v2f vert(appdata_t IN)
			{
				float4 alignedPos = AlignToPixelGrid(IN.vertex + _ShadowOffset);

				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(alignedPos);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color *_ShadowColor;

				#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				color.rgb = _ShadowColor.rgb;

				#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
				#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}

			ENDCG
		}

		// Draw actual sprite
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			float _pixelsPerUnit;

			float4 AlignToPixelGrid(float4 vertex)
			{
				float4 worldPos = mul(unity_ObjectToWorld, vertex);

				// NOTE: Causes sprite children to jitter when moving! Removed!
				//worldPos.x = floor(worldPos.x * _pixelsPerUnit + 0.5) / _pixelsPerUnit;
				//worldPos.y = floor(worldPos.y * _pixelsPerUnit + 0.5) / _pixelsPerUnit;

				return mul(unity_WorldToObject, worldPos);
			}

			v2f vert(appdata_t IN)
			{
				float4 alignedPos = AlignToPixelGrid(IN.vertex);

				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(alignedPos);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			float _HurtFlash;
			fixed4 _HurtFlashColor;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

				#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
				#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;

				if (_HurtFlash > 0)
                {
                    c.rgb = lerp(c.rgb, _HurtFlashColor.rgb, c.a); // Blend hurt flash color based on alpha
                }

				return c;
			}

			ENDCG
		}
	}
}