Shader "POSTAL: Onslaught/SpriteOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Cull Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        Tags { "Queue" = "Transparent" }

        Pass
        {
            CGPROGRAM

            #pragma vertex vertexFunc
            #pragma fragment fragmentFunc
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct v2f
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
            };

            v2f vertexFunc(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;

                o.pos = UnityPixelSnap(o.pos);

                return o;
            }

            fixed4 _Color;
            float4 _MainTex_TexelSize;

            fixed4 fragmentFunc(v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);
                half4 outlineC = _Color;
                outlineC.rgb *= outlineC.a;
     
                fixed myAlpha = c.a;
                fixed upAlpha = tex2D(_MainTex, i.uv + fixed2(0, _MainTex_TexelSize.y)).a;
                fixed downAlpha = tex2D(_MainTex, i.uv - fixed2(0, _MainTex_TexelSize.y)).a;
                fixed rightAlpha = tex2D(_MainTex, i.uv + fixed2(_MainTex_TexelSize.x, 0)).a;
                fixed leftAlpha = tex2D(_MainTex, i.uv - fixed2(_MainTex_TexelSize.x, 0)).a;

                return lerp(c, outlineC, ceil( clamp(downAlpha + upAlpha + leftAlpha + rightAlpha, 0, 1) ) - ceil(myAlpha));
            }

            ENDCG
        }
    }
}
