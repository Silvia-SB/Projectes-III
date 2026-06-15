Shader "Custom/TorchHitPointURP"
{
    Properties
    {
        _BaseColor ("Base Ember Color", Color) = (0.7, 0.12, 0.03, 1)
        _HotColor ("Hot Core Color", Color) = (1.0, 0.45, 0.08, 1)
        _Intensity ("Intensity", Range(0, 5)) = 1.8
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.5
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.25
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
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
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _HotColor;
                float _Intensity;
                float _PulseSpeed;
                float _PulseAmount;
                float _RimPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs =
                    GetVertexPositionInputs(IN.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.viewDirWS = normalize(GetWorldSpaceViewDir(posInputs.positionWS));

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float ndotv = saturate(dot(normalize(IN.normalWS), normalize(IN.viewDirWS)));
                float rim = pow(1.0 - ndotv, _RimPower);

                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                float3 color = lerp(_BaseColor.rgb, _HotColor.rgb, rim);
                color *= _Intensity * pulse;

                float alpha = saturate(0.45 + rim * 0.45);

                return half4(color, alpha);
            }

            ENDHLSL
        }
    }
}