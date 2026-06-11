Shader "Custom/PuddleEdgeHaloURP"
{
    Properties
    {
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (0.8, 0.65, 0.2, 0.35)
        _EdgeWidth ("Edge Width", Range(0.001, 0.2)) = 0.04
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.2)) = 0.04
        _AlphaPower ("Alpha Power", Range(0, 2)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

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
                float4 _EdgeColor;
                float _EdgeWidth;
                float _EdgeSoftness;
                float _AlphaPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half GetMask(float2 uv)
            {
                half4 tex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);
                return max(tex.a, dot(tex.rgb, half3(0.299, 0.587, 0.114)));
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                half center = GetMask(uv);

                half right = GetMask(uv + float2(_EdgeWidth, 0));
                half left  = GetMask(uv - float2(_EdgeWidth, 0));
                half up    = GetMask(uv + float2(0, _EdgeWidth));
                half down  = GetMask(uv - float2(0, _EdgeWidth));

                half edgeDifference =
                    abs(center - right) +
                    abs(center - left) +
                    abs(center - up) +
                    abs(center - down);

                half insideMask = smoothstep(0.15, 0.45, center);
                half edge = smoothstep(0.05, _EdgeSoftness, edgeDifference);

                half alpha = edge * insideMask * _EdgeColor.a * _AlphaPower;

                return half4(_EdgeColor.rgb, alpha);
            }

            ENDHLSL
        }
    }
}