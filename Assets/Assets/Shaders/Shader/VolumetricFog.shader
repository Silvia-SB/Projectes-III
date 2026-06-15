Shader "Custom/MasterShader"
{
    Properties
    {
        _Color("Color", Color) = (0, 0, 0, 1)
        _MaxDistance("Max distance", float) = 50
        _DistanceFogStart("Distance fog start", float) = 15
        _DistanceFogEnd("Distance fog end", float) = 40
        _DistanceFogIntensity("Distance fog intensity", Range(0, 50)) = 45
        _FogStrength("Fog strength", Range(0, 1)) = 1
        _StepSize("Step size", Range(0.1, 20)) = 1.3
        _DensityMultiplier("Density multiplier", Range(0, 10)) = 0.3
        _NoiseOffset("Noise offset", float) = 1
        
        _FogNoise("Fog noise", 3D) = "white" {}
        _NoiseTiling("Noise tiling", float) = 1
        _DensityThreshold("Density threshold", Range(0, 1)) = 0.4
        
        [HDR]_LightContribution("Light contribution", Color) = (1, 1, 1, 1)
        _LightScattering("Light scattering", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _Color;
            float _MaxDistance;
            float _DistanceFogStart;
            float _DistanceFogEnd;
            float _DistanceFogIntensity;
            float _FogStrength;
            float _DensityMultiplier;
            float _StepSize;
            float _NoiseOffset;
            TEXTURE3D(_FogNoise);
            float _DensityThreshold;
            float _NoiseTiling;
            float4 _LightContribution;
            float _LightScattering;

            float henyey_greenstein(float angle, float scattering)
            {
                return (1.0 - angle * angle) / (4.0 * PI * pow(1.0 + scattering * scattering - (2.0 * scattering) * angle, 1.5f));
            }
            
            float get_density(float3 worldPos)
            {
                float4 noise = _FogNoise.SampleLevel(sampler_TrilinearRepeat, worldPos * 0.01 * _NoiseTiling, 0);
                float density = dot(noise, noise);
                density = saturate(density - _DensityThreshold) * _DensityMultiplier;
                return density;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float depth = SampleSceneDepth(IN.texcoord);
                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);

                float3 entryPoint = _WorldSpaceCameraPos;
                float3 viewDir = worldPos - _WorldSpaceCameraPos;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;
                float distLimit = min(viewLength, _MaxDistance);
                float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x))) * _NoiseOffset;
                float transmittance = 1;
                float4 fogCol = _Color;

                while(distTravelled < distLimit)
                {
                    float3 rayPos = entryPoint + rayDir * distTravelled;
                    float baseDensity = get_density(rayPos);
                    
                    // Transición suave y controlada entre los dos puntos exactos
                    float smoothFactor = smoothstep(_DistanceFogStart, _DistanceFogEnd, distTravelled);
                    float absorptionDensity = baseDensity + (smoothFactor * _DistanceFogIntensity);

                    if (absorptionDensity > 0)
                    {
                        Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos));
                        fogCol.rgb += mainLight.color.rgb * _LightContribution.rgb * henyey_greenstein(dot(rayDir, mainLight.direction), _LightScattering) * baseDensity * mainLight.shadowAttenuation * _StepSize;
                        transmittance *= exp(-absorptionDensity * _StepSize);
                    }
                    distTravelled += _StepSize;
                }
                
                float fogAmount = (1.0 - saturate(transmittance)) * saturate(_FogStrength);
                return lerp(col, fogCol, fogAmount);
            }
            ENDHLSL
        }
    }
}

