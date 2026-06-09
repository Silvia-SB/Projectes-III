Shader "Custom/ProceduralMoon"
{
    Properties
    {
        _MoonColor ("Moon Color", Color) = (0.78,0.84,1,1)
        _MoonBrightness ("Moon Brightness", Range(0,10)) = 2.8

        _CraterScale ("Crater Scale", Range(1,80)) = 32
        _CraterStrength ("Crater Strength", Range(0,1)) = 0.08
        _LargeCraterStrength ("Large Crater Strength", Range(0,1)) = 0.12

        _HaloColor ("Halo Color", Color) = (0.55,0.68,1,1)
        _HaloIntensity ("Halo Intensity", Range(0,5)) = 0.7
        _HaloSize ("Halo Size", Range(0.5,2)) = 1.15
        _HaloFalloff ("Halo Falloff", Range(0.1,5)) = 2.2
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MoonColor;
                float _MoonBrightness;

                float _CraterScale;
                float _CraterStrength;
                float _LargeCraterStrength;

                float4 _HaloColor;
                float _HaloIntensity;
                float _HaloSize;
                float _HaloFalloff;
            CBUFFER_END

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x)
                     + (c - a) * u.y * (1.0 - u.x)
                     + (d - b) * u.x * u.y;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 centeredUV = IN.uv - 0.5;
                float dist = length(centeredUV);

                float moonMask = smoothstep(0.5, 0.47, dist);

                float edgeShade = 1.0 - dist * dist * 0.7;
                edgeShade = saturate(edgeShade);

                float smallNoise = noise(IN.uv * _CraterScale);
                float largeNoise = noise(IN.uv * 8.0);

                float craters =
                    1.0
                    - (1.0 - smallNoise) * _CraterStrength
                    - (1.0 - largeNoise) * _LargeCraterStrength;

                craters = saturate(craters);

                float3 moonColor =
                    _MoonColor.rgb *
                    craters *
                    edgeShade *
                    _MoonBrightness;

                float haloDistance = dist / _HaloSize;
                float halo = 1.0 - smoothstep(0.25, 0.65, haloDistance);
                halo = pow(saturate(halo), _HaloFalloff);

                float outsideMoon = 1.0 - moonMask;
                float haloAlpha = halo * _HaloIntensity * outsideMoon;

                float3 haloColor = _HaloColor.rgb * haloAlpha;

                float3 finalColor =
                    moonColor * moonMask +
                    haloColor;

                float finalAlpha =
                    saturate(moonMask + haloAlpha);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}