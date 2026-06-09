Shader "Custom/ProceduralStars"
{
    Properties
    {
        _SkyColor ("Sky Color", Color) = (0.015, 0.02, 0.05, 1)
        _StarColor ("Star Color", Color) = (0.86, 0.92, 1.0, 1)
        _StarDensity ("Star Density", Range(0.90, 0.999)) = 0.985
        _StarIntensity ("Star Intensity", Range(0, 10)) = 3
        _StarScale ("Star Scale", Range(20, 500)) = 180
        _TwinkleSpeed ("Twinkle Speed", Range(0, 5)) = 0.6
        _TwinkleAmount ("Twinkle Amount", Range(0, 1)) = 0.25
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
            Name "ProceduralStars"

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
                float4 _SkyColor;
                float4 _StarColor;
                float _StarDensity;
                float _StarIntensity;
                float _StarScale;
                float _TwinkleSpeed;
                float _TwinkleAmount;
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

                float horizonMask = smoothstep(-0.05, 0.25, dir.y);

                float3 grid = floor(dir * _StarScale);
                float rnd = hash(grid);

                float star = step(_StarDensity, rnd);

                float3 cell = frac(dir * _StarScale) - 0.5;
                float dist = length(cell);
                star *= smoothstep(0.45, 0.0, dist);

                float twinkle = 1.0 + sin(_Time.y * _TwinkleSpeed + rnd * 50.0) * _TwinkleAmount;

                float3 color = _SkyColor.rgb;
                color += _StarColor.rgb * star * _StarIntensity * twinkle * horizonMask;

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}