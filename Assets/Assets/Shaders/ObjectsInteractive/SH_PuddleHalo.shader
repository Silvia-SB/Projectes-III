Shader "Custom/PuddleHaloMaskedURP"
{
    Properties
    {
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Color ("Halo Color", Color) = (0.3, 0.65, 1, 0.3)
        _AlphaPower ("Alpha Power", Range(0, 2)) = 0.8
        _EdgeSoftness ("Edge Softness", Range(0, 1)) = 0.25
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
                float4 _Color;
                float _AlphaPower;
                float _EdgeSoftness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, IN.uv).a;

                mask = smoothstep(_EdgeSoftness, 1.0, mask);

                half alpha = mask * _Color.a * _AlphaPower;

                return half4(_Color.rgb, alpha);
            }

            ENDHLSL
        }
    }
}