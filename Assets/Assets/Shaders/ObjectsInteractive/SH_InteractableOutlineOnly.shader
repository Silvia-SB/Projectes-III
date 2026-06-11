Shader "Custom/InteractableOutlineOnly"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,0.8,0.2,0.35)
        _OutlineWidth ("Outline Width", Range(0.001,0.08)) = 0.02
        _OutlineIntensity ("Outline Intensity", Range(0.5,3)) = 1
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
            Name "Outline"

            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha

            ZWrite Off
            ZTest LEqual

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
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 expandedPosition =
                    IN.positionOS.xyz +
                    IN.normalOS * _OutlineWidth;

                OUT.positionHCS =
                    TransformObjectToHClip(expandedPosition);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 color = _OutlineColor.rgb * _OutlineIntensity;
                return half4(color, _OutlineColor.a);
            }

            ENDHLSL
        }
    }
}