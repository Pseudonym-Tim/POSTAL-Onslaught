Shader "POSTAL: Onslaught/VortexEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeParam ("Time Parameter", Float) = 1.0
        _SpeedParam ("Speed Parameter", Float) = 1.0
        _Pixel ("Pixel", Float) = 50.0
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

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _TimeParam;
            float _SpeedParam;
            float _Pixel;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float3 path(float3 p)
            {
                //float speed = 2.25;
                p.x = sin(p.z * _SpeedParam);
                p.y = cos(p.z * _SpeedParam);
                return p.xyz;
            }

            float df(float3 p, float3 eye)
            {
                float tunnelRadius = 2.0 - 2.0 * (p.z - eye.z) / (1.4);
                p.xy += path(p);
                return tunnelRadius - length(p.xy);
            }

            float3 dcol(float3 p)
            {
                p.xy += path(p);
                float tau = 6.28318;
                return lerp(float3(1, 0.141, 0.188), float3(0.541, 0.110, 0.153), step(fmod(2.0 * (2.0 * p.z + atan2(p.y, p.x) / tau), 1.0), 0.5));
            }

            float3 ComputeLighting(float3 normal, float3 lightDir)
            {
                float3 ambient = float3(0.1, 0.1, 0.1);
                float3 diffuse = float3(1.0, 1.0, 1.0) * max(0.0, dot(normal, lightDir));
                return ambient + diffuse;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv * 2.0 - 1.0;
                uv.y *= -1.0;

                uv = floor(uv * _Pixel) / _Pixel;

                float t = _TimeParam * 0.35;
                float3 eye = float3(0, 0, t);
                eye.xy -= path(eye);

                float3 dir = normalize(float3(uv, 0.3));

                float dist = 0.1, tot = -0.2;
                float3 p = eye + dir * tot;
                float3 col = float3(0,0,0);

                for (int i = 0; i < 70; i++)
                {
                    if (dist < 0.001 || tot > 100.0) break;
                    p = eye + dir * tot;
                    tot += dist = df(p, eye) * 0.1;
                    col = dcol(p) * (1.5 - (p.z - eye.z) / 0.8);
                }

                // Basic lighting
                float3 normal = normalize(float3(p.xy, 1.0));
                float3 lightDir = normalize(float3(0.5, 0.5, -1.0));
                float3 lighting = ComputeLighting(normal, lightDir);

                col *= lighting;

                return fixed4(col, 1.0);
            }

            ENDCG
        }
    }
    FallBack "Unlit/Texture"
}
