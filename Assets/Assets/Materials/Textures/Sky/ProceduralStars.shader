Shader "Custom/ProceduralStars"
{
    Properties
    {
        _ZenithColor ("Zenith Color", Color) = (0.008, 0.015, 0.045, 1)
        _HorizonColor ("Horizon Color", Color) = (0.035, 0.075, 0.16, 1)

        _StarDensity ("Star Density", Range(0.90, 0.999)) = 0.986
        _StarScale ("Star Scale", Range(20, 500)) = 220
        _StarIntensity ("Star Intensity", Range(0, 10)) = 2.3

        _TwinkleSpeed ("Twinkle Speed", Range(0, 5)) = 0.25
        _TwinkleAmount ("Twinkle Amount", Range(0, 1)) = 0.06

        _HorizonStarFade ("Horizon Star Fade", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Background"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "ProceduralStarsImproved"

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
                float4 _ZenithColor;
                float4 _HorizonColor;

                float _StarDensity;
                float _StarScale;
                float _StarIntensity;

                float _TwinkleSpeed;
                float _TwinkleAmount;

                float _HorizonStarFade;
            CBUFFER_END

            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
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

                float height01 = saturate(dir.y * 0.5 + 0.5);
                float gradient = smoothstep(0.0, 1.0, height01);

                float3 skyColor = lerp(
                    _HorizonColor.rgb,
                    _ZenithColor.rgb,
                    gradient
                );

                float horizonMask = smoothstep(
                    _HorizonStarFade,
                    0.75,
                    height01
                );

                float3 grid = floor(dir * _StarScale);
                float3 cell = frac(dir * _StarScale) - 0.5;

                float rnd = hash(grid);
                float rnd2 = hash(grid + 13.37);
                float rnd3 = hash(grid + 47.91);

                float starExists = step(_StarDensity, rnd);

                float starSize = lerp(0.08, 0.28, pow(rnd2, 8.0));

                float dist = length(cell);
                float starShape = smoothstep(starSize, 0.0, dist);

                float starBrightness = lerp(0.35, 1.0, pow(rnd3, 3.0));

                float rareBright = step(0.975, rnd2) * 1.8;

                float twinkle =
                    1.0 +
                    sin(_Time.y * _TwinkleSpeed + rnd * 60.0) *
                    _TwinkleAmount;

                float star =
                    starExists *
                    starShape *
                    starBrightness *
                    (1.0 + rareBright) *
                    twinkle *
                    horizonMask;

                float3 whiteStar = float3(0.86, 0.92, 1.0);
                float3 blueStar  = float3(0.65, 0.78, 1.0);
                float3 warmStar  = float3(1.0, 0.88, 0.65);

                float3 starColor = whiteStar;

                starColor = lerp(starColor, blueStar, step(0.82, rnd2) * 0.35);
                starColor = lerp(starColor, warmStar, step(0.94, rnd3) * 0.25);

                float3 finalColor =
                    skyColor +
                    starColor * star * _StarIntensity;

                return half4(finalColor, 1);
            }

            ENDHLSL
        }
    }
}