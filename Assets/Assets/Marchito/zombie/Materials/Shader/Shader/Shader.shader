Shader "Custom/NeonEdgeURP"
{
    Properties
    {
        _Color ("Neon Color", Color) = (1, 0, 0, 1)
        _Intensity ("Glow Intensity", Range(0, 100)) = 100
        _FresnelPower ("Fresnel Power", Range(0, 5)) = 1.5
        _MainTex ("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" }
        Blend SrcAlpha One // Additive blending for neon glow

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD1;
                half3 viewDirWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _FresnelPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz); // Explicitly use .xyz
                OUT.uv = IN.uv;
    
                half3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - worldPos);
                
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                return OUT;
        }

            half4 frag(Varyings IN) : SV_Target
            {
                half fresnel = saturate(1.0 - dot(IN.viewDirWS, IN.normalWS));
                fresnel = pow(fresnel, _FresnelPower); // Apply Fresnel power
                half3 glow = _Color.rgb * _Intensity * fresnel;
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return half4(texColor.rgb + glow, texColor.a);
            }
            ENDHLSL
        }
    }
}