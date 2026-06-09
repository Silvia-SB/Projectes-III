Shader "Custom/ProceduralClouds"
{
    Properties
    {
        _CloudColor ("Cloud Color", Color) = (0.16, 0.20, 0.28, 1)
        _CloudAlpha ("Cloud Alpha", Range(0,1)) = 0.22

        _CloudScale ("Cloud Scale", Range(0.5,20)) = 4.0
        _CloudCutoff ("Cloud Cutoff", Range(0,1)) = 0.52
        _CloudSoftness ("Cloud Softness", Range(0.01,1)) = 0.35

        _DetailScale ("Detail Scale", Range(1,50)) = 12.0
        _DetailStrength ("Detail Strength", Range(0,1)) = 0.25

        _SpeedX ("Speed X", Range(-1,1)) = 0.01
        _SpeedZ ("Speed Z", Range(-1,1)) = 0.003

        _HorizonFadeStart ("Horizon Fade Start", Range(-1,1)) = -0.05
        _HorizonFadeEnd ("Horizon Fade End", Range(-1,1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent+20"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "ProceduralCloudDome"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldDir : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _CloudColor;
                float _CloudAlpha;

                float _CloudScale;
                float _CloudCutoff;
                float _CloudSoftness;

                float _DetailScale;
                float _DetailStrength;

                float _SpeedX;
                float _SpeedZ;

                float _HorizonFadeStart;
                float _HorizonFadeEnd;
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
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x)
                     + (c - a) * u.y * (1.0 - u.x)
                     + (d - b) * u.x * u.y;
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;

                value += noise(p) * amplitude;
                p *= 2.02;
                amplitude *= 0.5;

                value += noise(p) * amplitude;
                p *= 2.03;
                amplitude *= 0.5;

                value += noise(p) * amplitude;

                return value;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldDir = normalize(positionWS - _WorldSpaceCameraPos);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.worldDir);

                float2 cloudCoord = dir.xz / max(dir.y + 0.35, 0.15);

                cloudCoord += float2(_Time.y * _SpeedX, _Time.y * _SpeedZ);

                float baseClouds = fbm(cloudCoord * _CloudScale);
                float details = fbm(cloudCoord * _DetailScale);

                float cloudShape = lerp(baseClouds, baseClouds * details, _DetailStrength);

                float clouds = smoothstep(
                    _CloudCutoff,
                    _CloudCutoff + _CloudSoftness,
                    cloudShape
                );

                float horizonMask = smoothstep(
                    _HorizonFadeStart,
                    _HorizonFadeEnd,
                    dir.y
                );

                float zenithFade = 1.0 - smoothstep(0.85, 1.0, dir.y) * 0.25;

                float alpha = clouds * _CloudAlpha * horizonMask * zenithFade;

                return half4(_CloudColor.rgb, alpha);
            }

            ENDHLSL
        }
    }
}