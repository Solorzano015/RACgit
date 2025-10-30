Shader "Custom/StylizedSkyboxWithSunAndAtmosphere"
{
    Properties
    {
        _SkyColor("Sky Color", Color) = (0.3, 0.6, 0.9, 1)
        _AtmosphereColor("Atmosphere Color", Color) = (0.8, 0.6, 0.4, 1)
        _CloudColor("Cloud Color", Color) = (1, 1, 1, 1)
        _CloudScale("Cloud Scale", Float) = 1
        _CloudDensity("Cloud Density", Range(0,1)) = 0.5
        _CloudSpeed("Cloud Speed", Float) = 0.1
        _CloudDirection("Cloud Direction", Vector) = (1, 0, 0, 0)
        _CloudHeight("Cloud Height", Range(0,1)) = 0.5
        _CloudSoftness("Cloud Softness", Range(0.1, 5.0)) = 1.5

        _SunColor("Sun Color", Color) = (1, 0.9, 0.7, 1)
        _SunDirection("Sun Direction", Vector) = (0, 1, 0, 0)
        _SunSize("Sun Size", Range(0, 0.5)) = 0.05
        _SunSoftness("Sun Softness", Range(0.0001, 0.2)) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _SkyColor;
            fixed4 _AtmosphereColor;
            fixed4 _CloudColor;
            float _CloudScale;
            float _CloudDensity;
            float _CloudSpeed;
            float4 _CloudDirection;
            float _CloudHeight;
            float _CloudSoftness;

            fixed4 _SunColor;
            float4 _SunDirection;
            float _SunSize;
            float _SunSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(hash(i + float2(0, 0)), hash(i + float2(1, 0)), u.x),
                    lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), u.x),
                    u.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < 5; ++i)
                {
                    value += amplitude * noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float2 uv = dir.xz;

                float time = _Time.y * _CloudSpeed;
                float2 motion = uv * _CloudScale + time * _CloudDirection.xy;

                float clouds = fbm(motion);
                clouds = pow(clouds, _CloudSoftness);
                clouds = smoothstep(_CloudDensity - 0.1, _CloudDensity + 0.1, clouds);

                float heightFade = smoothstep(0.0, 1.0, saturate(dir.y - _CloudHeight));
                clouds *= heightFade;

                float atmosphereFactor = pow(saturate(dir.y), 1.5);
                float3 baseSky = lerp(_AtmosphereColor.rgb, _SkyColor.rgb, atmosphereFactor);
                float3 finalColor = lerp(baseSky, _CloudColor.rgb, clouds);

                // === Sol ===
                float3 sunDir = normalize(_SunDirection.xyz);
                float angle = acos(dot(dir, sunDir)); // Ángulo entre rayos y dirección del sol

                if (_SunSize > 0)
                {
                    float sunDisc = smoothstep(_SunSize + _SunSoftness, _SunSize, angle);
                    finalColor = lerp(finalColor, _SunColor.rgb, sunDisc);
                }

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
